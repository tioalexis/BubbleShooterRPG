using System;
using Cyl.BubbleShooter.Grid;
using Cyl.Common.Utils;
using Cyl.Hexagons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cyl.BubbleShooter.Gameplay
{
    public class BubbleLauncher : MonoBehaviour
    {
        private const int MaxTrajectoryPoints = 8; // Maximum number of points in the trajectory
        
        private struct CalculateTrajectoryResult
        {
            public Vector2[] Points;
            public int Length;
            public float Angle;
            public Hex? LandingCoord;
        }

        public event Action<Vector2[], Hex> OnBubbleLaunched;
        
        [SerializeField] private Camera gameCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private Transform activeBubblePoint;
        
        [Tooltip("Layer mask for objects that can bounce the bubble.")]
        [SerializeField] private LayerMask bounceLayerMask;
        
        [Tooltip("Layer mask for objects that terminate the bubble's trajectory.")]
        [SerializeField] private LayerMask terminateLayerMask;

        [SerializeField] private BubbleLauncherTrajectory trajectory;
        [SerializeField] private float minAimAngle = 20f;
        [SerializeField] private float maxAimAngle = 160f;

        private readonly Vector2[] _aimTrajectory = new Vector2[MaxTrajectoryPoints];
        private Hex? _landingCoord = null;
        private CalculateTrajectoryResult? _currentTrajectory = null;
        
        public Vector2 AimPosition { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsLaunching { get; private set; }
        
        private void Awake()
        {
            playerInput.actions["Launch"].performed += OnLaunch;
            playerInput.actions["Launch"].canceled += OnLaunch;
            playerInput.actions["Aim"].performed += OnAim;
        }

        private void OnDestroy()
        {
            playerInput.actions["Launch"].performed -= OnLaunch;
            playerInput.actions["Launch"].canceled -= OnLaunch;
            playerInput.actions["Aim"].performed -= OnAim;
        }
        
        private void HandleBubbleLaunch()
        {
            if (_currentTrajectory == null)
            {
                Debug.LogError("Attempted to launch bubble without a valid trajectory. Ignoring launch.");
                return;
            }
            
            var trajectoryResult = _currentTrajectory.Value;
            if (trajectoryResult.Length < 2)
            {
                Debug.LogError($"Invalid trajectory length: {trajectoryResult.Length}. Cannot launch bubble.");
                return;
            }
                
            if (IsTrajectoryTooSteep(trajectoryResult.Angle))
            {
                Debug.LogWarning("Trajectory is too steep. Launch aborted.");
                return;
            }

            if (trajectoryResult.LandingCoord == null)
            {
                Debug.LogWarning($"Could not find a valid grid position for the bubble. Launch aborted.");
                return;
            }

            LaunchBubbleAsync(trajectoryResult);
        }

        private void HandleTrajectoryView()
        {
            if (!IsAiming || _currentTrajectory == null)
            {
                trajectory.SetLineRendererVisible(false);
                trajectory.SetLandingIndicatorActive(false);
                return;
            }

            var trajectoryResult = _currentTrajectory.Value;
            if (trajectoryResult.Length < 2 || IsTrajectoryTooSteep(trajectoryResult.Angle))
            {
                trajectory.SetLineRendererVisible(false);
                trajectory.SetLandingIndicatorActive(false);
                return;
            }
            
            if (trajectoryResult.LandingCoord.HasValue)
            {
                var landingWorldPosition = bubbleGrid.GetWorldPosition(trajectoryResult.LandingCoord.Value);
                trajectory.SetLandingIndicatorPosition(landingWorldPosition);
                trajectory.SetLandingIndicatorActive(true);
            }
            else
            {
                trajectory.SetLandingIndicatorActive(false);
            }
            
            trajectory.SetTrajectory(trajectoryResult.Points, trajectoryResult.Length);
            trajectory.SetAngle(trajectoryResult.Angle);
        }

        private async Awaitable LaunchBubbleAsync(CalculateTrajectoryResult calculatedTrajectory)
        {
            if (!calculatedTrajectory.LandingCoord.HasValue)
                throw new InvalidOperationException("Cannot launch bubble without a valid trajectory.");
            
            IsLaunching = true;
            
            var landingWorldPosition = bubbleGrid.GetWorldPosition(calculatedTrajectory.LandingCoord.Value);
            for (var i = 0; i < calculatedTrajectory.Length - 1; i++)
            {
                var from = calculatedTrajectory.Points[i];
                var to = calculatedTrajectory.Points[i + 1];
                if (i == calculatedTrajectory.Length - 1)
                    to = landingWorldPosition;
                
                var distance = Vector2.Distance(from, to);
                var stepCount = Mathf.CeilToInt(distance / bubbleGrid.CellSize) + 1;
                for (var j = 0; j < stepCount; j++)
                {
                    var stepPosition = Vector2.Lerp(from, to, ((float)j / stepCount));
                    DebugDraw.X(stepPosition, Color.aquamarine);
                    await Awaitable.FixedUpdateAsync();
                }
            }
            
            DebugDraw.X(landingWorldPosition, Color.green);

            IsLaunching = false;
        }

        private CalculateTrajectoryResult CalculateTrajectory()
        {
            const float maxLength = 20f; // Maximum length of the trajectory
            
            var originWorldPosition = (Vector2)activeBubblePoint.position;
            _aimTrajectory[0] = originWorldPosition;
            
            var aimWorldPosition = (Vector2)gameCamera.ScreenToWorldPoint(new Vector3(AimPosition.x, AimPosition.y, gameCamera.nearClipPlane));
            var trajectoryIndex = 1; // Start from index 1 since index 0 is the origin
            var aimDirection = (aimWorldPosition - originWorldPosition).normalized;
            var combinedLayerMask = bounceLayerMask | terminateLayerMask;
            var bounceOffset = bubbleGrid.CellSize;
            while (trajectoryIndex < _aimTrajectory.Length)
            {
                var boxSize = bubbleGrid.CellSize * Vector2.one;
                var hit = Physics2D.BoxCast(originWorldPosition, boxSize, 0, aimDirection, maxLength, combinedLayerMask);
                if (!hit)
                {
                    break;
                }

                // We hit something, so we update the trajectory
                BoxCastDrawer.Draw(hit, originWorldPosition, boxSize, 0, aimDirection, maxLength);
                aimDirection = new Vector2(-aimDirection.x, aimDirection.y);
                originWorldPosition = hit.point + aimDirection * bounceOffset;

                // If we hit a wall, we store the hit point and continue
                var hitLayer = hit.collider.gameObject.layer;
                if (bounceLayerMask.Contains(hitLayer))
                {
                    _aimTrajectory[trajectoryIndex] = hit.point;
                    trajectoryIndex++;
                }

                // If we hit a layer that terminates the trajectory (a bubble or the ceiling), we stop
                else if (terminateLayerMask.Contains(hitLayer))
                {
                    var reverseDir = (hit.point - originWorldPosition).normalized;
                    _aimTrajectory[trajectoryIndex] = hit.point + reverseDir * bounceOffset;
                    trajectoryIndex++;
                    break;
                }
            }
            
            // Remove any segments that are too close together
            var minSegmentLength = bubbleGrid.CellSize * 1.5f;
            for (var i = 1; i < trajectoryIndex; i++)
            {
                var from = _aimTrajectory[i];
                var to = _aimTrajectory[i - 1];
                if (!(Vector2.Distance(from, to) < minSegmentLength)) 
                    continue;
                
                for (var j = i; j < trajectoryIndex - 1; j++)
                    _aimTrajectory[j] = _aimTrajectory[j + 1];
                
                trajectoryIndex--;
                i--;
            }
            
            // Try to find where the bubble is likely to land
            Hex? safeLandingCoord = null;
            var lastPoint = _aimTrajectory[trajectoryIndex - 1];
            var rawLandingCoord = bubbleGrid.GetGridPosition(lastPoint);
            if (bubbleGrid.IsValidPosition(rawLandingCoord))
                if (bubbleGrid.FindNearestUnoccupiedGridPosition(lastPoint, out var nearestUnoccupiedHex))
                    safeLandingCoord = nearestUnoccupiedHex;
            
            var result = new CalculateTrajectoryResult
            {
                Points = _aimTrajectory,
                Length = trajectoryIndex,
                Angle = CalculateTrajectoryAngle(_aimTrajectory),
                LandingCoord = safeLandingCoord
            };
            
            return result;
        }

        private float CalculateTrajectoryAngle(Vector2[] points = null)
        {
            if (points == null || points.Length < 2)
                return 0f;
            
            var firstSegment = points[1] - points[0];
            var angle = Mathf.Atan2(firstSegment.y, firstSegment.x) * Mathf.Rad2Deg;
            angle = (angle + 360) % 360; // Normalize angle to [0, 360)
            return angle;
        }

        private bool IsTrajectoryTooSteep(float angle)
        {
            return angle < minAimAngle || angle > maxAimAngle;
        }
        
        private void OnAim(InputAction.CallbackContext context)
        {
            AimPosition = context.ReadValue<Vector2>();

            if (IsAiming)
            {
                _currentTrajectory = CalculateTrajectory();
                HandleTrajectoryView();
            }
        }

        private void OnLaunch(InputAction.CallbackContext context)
        {
            // Ignore input if we are currently launching a bubble
            if (IsLaunching)
                return;
            
            // Immediately calculate the trajectory once the launch action is performed
            // This allows clicks and taps to perform the launch action
            if (context.performed)
            {
                IsAiming = true;
                _currentTrajectory = CalculateTrajectory();
                HandleTrajectoryView();
                return;
            }

            // Once we release the launch action, we launch the bubble
            if (context.canceled)
            {
                // TODO: Only allow launching if the input is old enough to combat fat-finger inputs
                HandleBubbleLaunch();
                
                IsAiming = false;
                _currentTrajectory = null;
                
                HandleTrajectoryView();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (gameCamera == null || activeBubblePoint == null)
                return;
            
            // Draw valid angles for aiming
            const float angleLineLength = 10f; // Length of the angle lines
            var originWorldPosition = activeBubblePoint.position;
            var minAngle = minAimAngle * Mathf.Deg2Rad;
            var maxAngle = maxAimAngle * Mathf.Deg2Rad;
            var minDirection = new Vector2(Mathf.Cos(minAngle), Mathf.Sin(minAngle));
            var maxDirection = new Vector2(Mathf.Cos(maxAngle), Mathf.Sin(maxAngle));
            var minAimWorldPosition = originWorldPosition + (Vector3)minDirection * angleLineLength;
            var maxAimWorldPosition = originWorldPosition + (Vector3)maxDirection * angleLineLength;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(originWorldPosition, minAimWorldPosition);
            Gizmos.DrawLine(originWorldPosition, maxAimWorldPosition);
            
            // Draw a circle at the aim position
            var aimWorldPosition = gameCamera.ScreenToWorldPoint(new Vector3(AimPosition.x, AimPosition.y, gameCamera.nearClipPlane));
            aimWorldPosition.z = 0f; // Ensure the position is in the 2D plane
            Gizmos.color = IsAiming ? Color.green : Color.red;
            Gizmos.DrawWireSphere(aimWorldPosition, 0.5f);
            
            // Draw the landing position if it has been calculated
            if (_landingCoord.HasValue)
            {
                var landingWorldPosition = bubbleGrid.GetWorldPosition(_landingCoord.Value);
                Gizmos.color = Color.orangeRed;
                Gizmos.DrawWireSphere(landingWorldPosition, 0.5f);
            }
        }
    }
}