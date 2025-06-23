using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cyl.BubbleShooter.Gameplay
{
    public class BubbleShooter : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private PlayerInput playerInput;
        
        [Header("Grid")]
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private Bubble bubblePrefab;
        [SerializeField] private Transform ceiling;
        [SerializeField] private Transform leftWall;
        [SerializeField] private Transform rightWall;

        [Header("Positioning")]
        [SerializeField] private int visibleRows = 8;
        [Tooltip("Where the top edge of the grid should be anchored in world space.")]
        [SerializeField] private Transform topEdgeAnchor;
        [Tooltip("Where the bottom edge of the grid should be anchored in world space.")]
        [SerializeField] private Transform bottomEdgeAnchor;
        
        [Header("Shooting")]
        [SerializeField] private BubbleLauncher bubbleLauncher;
        [SerializeField] private BubbleQueue bubbleQueue;
        
        [Header("Debug")]
        [SerializeField] private int initialOccupiedRows = 6;
        
        private void Start()
        {
            PopulateGrid();
            AlignCameraToGrid();
            AlignWallsToGrid();
            // MoveGridToViewAsync(0f);
        }

        private void PopulateGrid()
        {
            // Populate from the top down, so the first rows are occupied.
            for (var row = bubbleGrid.Height - 1; row >= bubbleGrid.Height - initialOccupiedRows; row--)
            {
                for (var col = 0; col < bubbleGrid.Width; col++)
                {
                    var bubble = bubbleQueue.GenerateBubble();
                    bubbleGrid.AddElement(bubble, col, row);
                }
            }
        }

        private void AlignCameraToGrid()
        {
            var gridSize = bubbleGrid.CalculateGridSize();
            var cameraPosition = gameCamera.transform.position;
            cameraPosition.x = (gridSize.x / 2f) - 0.5f; // TODO: remove magic number
            gameCamera.transform.position = cameraPosition;
        }
        
        private void AlignWallsToGrid()
        {
            var gridSize = bubbleGrid.CalculateGridSize();
            
            var ceilingPosition = ceiling.transform.position;
            ceilingPosition.y = topEdgeAnchor.position.y;
            ceiling.transform.position = ceilingPosition;
            
            var leftWallPosition = leftWall.transform.position;
            leftWallPosition.x = -bubbleGrid.CellInnerRadius;
            leftWall.transform.position = leftWallPosition;
            
            var rightWallPosition = rightWall.transform.position;
            rightWallPosition.x = gridSize.x - bubbleGrid.CellInnerRadius;
            rightWall.transform.position = rightWallPosition;
        }


        private Vector3 CalculateAnchorPosition()
        {
            var lastOccupiedRow = bubbleGrid.FindLastOccupiedRow();
            var isAnchoredTop = lastOccupiedRow <= visibleRows;
            var offsetPadding = isAnchoredTop ? bubbleGrid.CellInnerRadius : 0f;
            var anchoredRow = isAnchoredTop ? 0 : lastOccupiedRow;
            var anchorPosition = isAnchoredTop ? topEdgeAnchor.position : bottomEdgeAnchor.position;
            var anchoredRowPosition = bubbleGrid.GetWorldPosition(0, anchoredRow);
            var offset = anchorPosition.y - anchoredRowPosition.y - offsetPadding;
            
            var targetPosition = bubbleGrid.transform.position;
            targetPosition.y += offset;
            return targetPosition;
        }
        
        private async Awaitable MoveGridToViewAsync(float duration)
        {
            var elapsedTime = 0f;
            var startPosition = bubbleGrid.transform.position.y;
            var targetPosition = CalculateAnchorPosition();
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                var newPosition = Vector3.Lerp(new Vector3(bubbleGrid.transform.position.x, startPosition, bubbleGrid.transform.position.z), targetPosition, t);
                bubbleGrid.transform.position = newPosition;
                await Awaitable.EndOfFrameAsync();
            }

            bubbleGrid.transform.position = targetPosition;
        }

        private void OnDebugDown()
        {
            var lastOccupiedRow = bubbleGrid.FindLastOccupiedRow();
            if (lastOccupiedRow < 0)
            {
                Debug.LogWarning("No bubbles to remove.");
                return;
            }
            
            for (var col = 0; col < bubbleGrid.Width; col++)
                bubbleGrid.RemoveElement(col, lastOccupiedRow);
            
            MoveGridToViewAsync(0.2f);
        }
    }

}
