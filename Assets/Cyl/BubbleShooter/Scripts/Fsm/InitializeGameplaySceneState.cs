namespace Cyl.BubbleShooter.Fsm
{
    public class InitializeGameplaySceneState : BubbleShooterFsmState
    {
        public override string Name => "InitializeGameplayScene";
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // AlignCameraToGrid();
            // AlignWallsToGrid();
            Finish();
        }

        private void AlignCameraToGrid()
        {
            var gridSize = BubbleGrid.CalculateGridSize();
            var cameraPosition = Camera.transform.position;
            cameraPosition.x = (gridSize.x / 2f) - 0.5f; // TODO: remove magic number
            BubbleShooterGame.Camera.transform.position = cameraPosition;
        }

        private void AlignWallsToGrid()
        {
            var gridSize = BubbleGrid.CalculateGridSize();
            
            var ceilingPosition = View.Ceiling.transform.position;
            ceilingPosition.y = View.TopEdgeAnchor.position.y;
            View.Ceiling.transform.position = ceilingPosition;
            
            var leftWallPosition = View.LeftWall.transform.position;
            leftWallPosition.x = -BubbleGrid.CellInnerRadius;
            View.LeftWall.transform.position = leftWallPosition;
            
            var rightWallPosition = View.RightWall.transform.position;
            rightWallPosition.x = gridSize.x - BubbleGrid.CellInnerRadius;
            View.RightWall.transform.position = rightWallPosition;
        }
    }
}