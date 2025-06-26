namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Moves the bubble grid off-screen to prepare for the intro animation
    /// and then moves it into view. This state is typically used at the start of the game
    /// to show the bubble grid sliding into view from the bottom of the screen.
    /// </summary>
    public class IntroGridToViewState : MoveGridToViewState
    {
        public override string Name => "IntroGridToView";

        /// <summary>
        /// Moves the bubble grid off-screen to prepare for the intro animation and then moves it into view.
        /// </summary>
        public override void OnEnter()
        {
            MoveGridOffscreen();
            
            base.OnEnter();
        }

        private void MoveGridOffscreen()
        {
            const float verticalOffset = 10f;
            var topMostRow = BubbleGrid.Height - 1;
            var gridTransform = BubbleGrid.RootTransform;
            var offset = -BubbleGrid.GetWorldPosition(0, topMostRow).y - verticalOffset;
            var targetPosition = gridTransform.position;
            targetPosition.y = offset;
            gridTransform.position = targetPosition;
        }
    }
}