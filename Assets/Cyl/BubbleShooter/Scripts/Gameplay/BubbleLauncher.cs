using System;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Grid;
using Cyl.Common.Utils;
using Cyl.Hexagons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cyl.BubbleShooter.Gameplay
{
    /// <summary>
    /// The launcher is responsible for handling the aiming and launching of bubbles in the game.
    /// It receives input from the player to aim and launch bubbles, calculates the trajectory of the bubble,
    /// then launches the bubble along that trajectory.
    /// </summary>
    public class BubbleLauncher : MonoBehaviour
    {
        /// <summary>
        /// Maximum number of points in the trajectory.
        /// If the trajectory exceeds this number, it will be truncated.
        /// </summary>
        private const int MaxTrajectoryPoints = 8;
        
        /// <summary>
        /// The maximum length of the raycast used to determine the trajectory of the bubble.
        /// </summary>
        private const float MaxRaycastLength = 20f;
        
        /// <summary>
        /// The name for the input action that triggers the launch of a bubble.
        /// </summary>
        private const string LaunchAction = "Launch";
        
        /// <summary>
        /// The name for the input action that allows the player to aim the bubble launcher.
        /// </summary>
        private const string AimAction = "Aim";
        
        /// <summary>
        /// The name for the input action that cycles through the bubble queue.
        /// </summary>
        private const string CycleAction = "Cycle";

        private struct CalculateTrajectoryResult
        {
            public Vector2[] Points;
            public int Length;
            public float Angle;
            public Hex? LandingCoord;
        }

        /// <summary>
        /// Invoked when a bubble is launched.
        /// </summary>
        public event Action<Bubble, Hex> OnBubbleLaunched;
        
        /// <summary>
        /// Invoked when a bubble lands on the grid after being launched.
        /// </summary>
        public event Action<Bubble, Hex> OnBubbleLanded;
        
        [SerializeField] private Camera gameCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private BubbleQueue bubbleQueue;
        
        [Tooltip("Layer mask for objects that can bounce the bubble.")]
        [SerializeField] private LayerMask bounceLayerMask;
        
        [Tooltip("Layer mask for objects that terminate the bubble's trajectory.")]
        [SerializeField] private LayerMask terminateLayerMask;

        [SerializeField] private BubbleLauncherTrajectory trajectory;
        [SerializeField] private float minAimAngle = 20f;
        [SerializeField] private float maxAimAngle = 160f;

        private readonly Vector2[] _aimTrajectory = new Vector2[MaxTrajectoryPoints];
        private CalculateTrajectoryResult? _currentTrajectory = null;
        
        /// <summary>
        /// Where the player is aiming in screen coordinates.
        /// </summary>
        public Vector2 AimPosition { get; private set; }
        
        /// <summary>
        /// Whether the player is currently aiming.
        /// This is true when the player is holding down the aim button or the screen is being touched.
        /// </summary>
        public bool IsAiming { get; private set; }
        
        /// <summary>
        /// Whether the launcher is currently in the process of launching a bubble.
        /// This is true when the bubble is being moved along the trajectory after the launch action is performed.
        /// </summary>
        public bool IsLaunching { get; private set; }
        
        /// <summary>
        /// Reference to the last bubble that was launched.
        /// </summary>
        public Bubble LastBubbleLaunched { get; private set; }
        
        private void Awake()
        {
            playerInput.actions[LaunchAction].performed += OnLaunch;
            playerInput.actions[LaunchAction].canceled += OnLaunch;
            playerInput.actions[AimAction].performed += OnAim;
            playerInput.actions[CycleAction].performed += OnCycleQueue;
        }

        private void OnDestroy()
        {
            playerInput.actions[LaunchAction].performed -= OnLaunch;
            playerInput.actions[LaunchAction].canceled -= OnLaunch;
            playerInput.actions[AimAction].performed -= OnAim;
            playerInput.actions[CycleAction].performed -= OnCycleQueue;
        }
        
        private void HandleBubbleLaunch()
        {
            if (bubbleQueue.ActiveBubble == null)
            {
                Debug.LogError("Attempted to launch bubble without an active bubble in the queue.");
                return;
            }
            
            if (_currentTrajectory == null)
            {
                Debug.LogError("Attempted to launch bubble without a valid trajectory.");
                return;
            }
            
            var trajectoryResult = _currentTrajectory.Value;
            if (trajectoryResult.Length < 2)
            {
                Debug.LogError($"Attempted to launch bubble with an invalid trajectory length: {trajectoryResult.Length}. Expected at least 2 points.");
                return;
            }
                
            if (IsTrajectoryTooSteep(trajectoryResult.Angle))
            {
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
            
            var bubble = bubbleQueue.ActiveBubble;
            bubble.SetColliderEnabled(false);
            
            IsLaunching = true;
            OnBubbleLaunched?.Invoke(bubble, calculatedTrajectory.LandingCoord.Value);
            
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
                    bubbleQueue.ActiveBubble.transform.position = stepPosition;
                    await Awaitable.FixedUpdateAsync();
                }
            }
            
            bubble.SetColliderEnabled(true);
            bubbleGrid.AddElement(bubble, calculatedTrajectory.LandingCoord.Value);
            bubbleQueue.RemoveBubble(bubble);
            
            IsLaunching = false;
            LastBubbleLaunched = bubble;
            OnBubbleLanded?.Invoke(bubble, calculatedTrajectory.LandingCoord.Value);
        }

        private CalculateTrajectoryResult CalculateTrajectory()
        {
            var activeBubblePoint = bubbleQueue.ActiveBubbleSpawnPoint;
            var originWorldPosition = (Vector2)activeBubblePoint.position;
            _aimTrajectory[0] = originWorldPosition;
            
            var aimWorldPosition = (Vector2)gameCamera.ScreenToWorldPoint(new Vector3(AimPosition.x, AimPosition.y, gameCamera.nearClipPlane));
            var trajectoryIndex = 1; // Start from index 1 since index 0 is the origin
            var aimDirection = (aimWorldPosition - originWorldPosition).normalized;
            var combinedLayerMask = bounceLayerMask | terminateLayerMask;
            var bounceOffset = bubbleGrid.CellSize;
            while (trajectoryIndex < _aimTrajectory.Length)
            {
                var boxSize = 0.5f * bubbleGrid.CellSize * Vector2.one;
                var hit = Physics2D.BoxCast(originWorldPosition, boxSize, 0, aimDirection, MaxRaycastLength, combinedLayerMask);
                if (!hit)
                {
                    break;
                }
                
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
                    _aimTrajectory[trajectoryIndex] = hit.point - reverseDir * bubbleGrid.CellSize;
                    trajectoryIndex++;
                    break;
                }
                
                // We hit something, so we update the trajectory
                BoxCastDrawer.Draw(hit, originWorldPosition, boxSize, 0, aimDirection, MaxRaycastLength);
                aimDirection = new Vector2(-aimDirection.x, aimDirection.y);
                originWorldPosition = hit.point + aimDirection * bounceOffset;
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
            
            // Make sure the trajectory ends at the landing position
            if (safeLandingCoord.HasValue)
            {
                var landingWorldPosition = bubbleGrid.GetWorldPosition(safeLandingCoord.Value);
                _aimTrajectory[trajectoryIndex - 1] = landingWorldPosition;
            }
            
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
        
        private void OnCycleQueue(InputAction.CallbackContext context)
        {
            if (IsLaunching)
                return;
            
            // If the input is performed from a non-keyboard device, we first check if the
            // aim position matches the queue touch collider.
            if (!context.control.device.IsKeyboard())
            {
                var touchWorldPosition = gameCamera.ScreenToWorldPoint(new Vector3(AimPosition.x, AimPosition.y, gameCamera.nearClipPlane));
                var queueTouchCollider = bubbleQueue.TouchCollider;
                if (queueTouchCollider == null || !queueTouchCollider.OverlapPoint(touchWorldPosition))
                {
                    return;
                }
            }
            
            bubbleQueue.CycleQueue();
        }

        private void OnDrawGizmosSelected()
        {
            if (gameCamera == null || bubbleQueue == null)
                return;
            
            const float angleLineLength = 10f; // Length of the angle lines
            
            // Draw valid angles for aiming
            var activeBubblePoint = bubbleQueue.ActiveBubbleSpawnPoint;
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
        }
    }
}