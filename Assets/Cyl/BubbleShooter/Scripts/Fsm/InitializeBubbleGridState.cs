namespace Cyl.BubbleShooter.Fsm
{
    public class InitializeBubbleGridState : BubbleShooterFsmState
    {
        public override string Name => "InitializeBubbleGrid";
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // Populate from the top down, so the first rows are occupied.
            const int initialOccupiedRows = 6;
            for (var row = BubbleGrid.Height - 1; row >= BubbleGrid.Height - initialOccupiedRows; row--)
            {
                for (var col = 0; col < BubbleGrid.Width; col++)
                {
                    var bubble = BubbleShooterGame.Queue.GenerateBubble();
                    BubbleGrid.AddElement(bubble, col, row);
                }
            }
            
            // Move the grid off-screen to start so it can be animated into view later.
            var verticalOffset = 10f;
            var topMostRow = BubbleGrid.Height - 1;
            var gridTransform = BubbleGrid.RootTransform;
            var offset = -BubbleGrid.GetWorldPosition(0, topMostRow).y - verticalOffset;
            var targetPosition = gridTransform.position;
            targetPosition.y = offset;
            gridTransform.position = targetPosition;
            
            Finish();
        }
    }
}