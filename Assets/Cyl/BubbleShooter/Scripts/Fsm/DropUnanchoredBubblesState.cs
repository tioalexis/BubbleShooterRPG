using Cyl.BubbleShooter.Bubbles;
using Cyl.Common.Utils;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// This state is responsible for dropping all unanchored bubbles in the grid.
    /// The state will find all clusters of bubbles in the grid, check if they are anchored,
    /// and if not, drop them down to the bottom of the grid. This state ends when all unanchored bubbles have been dropped.
    /// </summary>
    public class DropUnanchoredBubblesState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "DropUnanchoredBubbles";

        private int _bubblesBeingDropped;

        /// <summary>
        /// Find all clusters of bubbles in the grid, check if they are anchored,
        /// and if not, drop them down to the bottom of the grid. Otherwise, finish the state.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();

            BubbleShooterGame.DropUnanchoredBubbles(Finish);
        }
    }
}