namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Prepares the bubble grid by populating it with bubbles from the top down.
    /// </summary>
    public class InitializeBubbleGridState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "InitializeBubbleGrid";
        
        /// <summary>
        /// Populates the bubble grid with bubbles from the top down and moves the grid off-screen to start.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            
            // TODO: Load the bubble grid from a configuration file or scriptable object.
            
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
            
            Finish();
        }
    }
}