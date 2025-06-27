using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Initializes the gameplay scene.
    /// </summary>
    public class InitializeGameplaySceneState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "InitializeGameplayScene";
        
        /// <summary>
        /// Initializes the gameplay scene.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            
            AlignCameraToGrid();
            // AlignWallsToGrid();
            Finish();
        }

        private void AlignCameraToGrid()
        {
            var maxOrthographicSize = View.CameraSettings.maxOrthographicSize;
            var positionOffset = View.CameraSettings.positionOffset;
            
            // Align camera position to the center of the grid
            var gridSize = BubbleGrid.CalculateGridSize();
            var cameraPosition = Camera.transform.position;
            cameraPosition.x = gridSize.x / 2f;
            BubbleShooterGame.Camera.transform.position = cameraPosition + (Vector3)positionOffset;
            
            // Resize the camera orthographic size to fit the width of the grid
            var cameraAspect = Camera.aspect;
            var cameraOrthographicSize = gridSize.x / (2f * cameraAspect);
            Camera.orthographicSize = Mathf.Max(cameraOrthographicSize, maxOrthographicSize);
        }
        
        // private void AlignWallsToGrid()
        // {
        //     var gridSize = BubbleGrid.CalculateGridSize();
        //     
        //     var ceilingPosition = View.Ceiling.transform.position;
        //     ceilingPosition.y = View.TopEdgeAnchor.position.y;
        //     View.Ceiling.transform.position = ceilingPosition;
        //     
        //     var leftWallPosition = View.LeftWall.transform.position;
        //     leftWallPosition.x = -BubbleGrid.CellInnerRadius;
        //     View.LeftWall.transform.position = leftWallPosition;
        //     
        //     var rightWallPosition = View.RightWall.transform.position;
        //     rightWallPosition.x = gridSize.x - BubbleGrid.CellInnerRadius;
        //     View.RightWall.transform.position = rightWallPosition;
        // }
    }
}