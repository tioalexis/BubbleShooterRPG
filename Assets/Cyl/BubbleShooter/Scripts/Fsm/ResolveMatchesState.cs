using System;
using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Handles the resolution of matches in the Bubble Shooter game.
    /// Finds connected bubbles of the same color as the last launched bubble,
    /// and removes them from the grid.
    /// </summary>
    public class ResolveMatchesState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "ResolveMatches";

        private readonly Bubble[] _connectedBubbles = new Bubble[320];
        
        /// <summary>
        /// Finds all bubbles connected to the last launched bubble that have the same color,
        /// and removes them from the grid.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            
            var bubbleLaunched = BubbleShooterGame.Launcher.LastBubbleLaunched;
            var numConnectedBubbles = BubbleGrid.FindConnectedBubblesNonAlloc(
                bubbleLaunched.GridPosition, IsSameColorAsLaunchedBubble, _connectedBubbles);
            if (numConnectedBubbles > 1)
            {
                for (var i = 0; i < numConnectedBubbles; i++)
                {
                    var bubble = _connectedBubbles[i];
                    BubbleGrid.RemoveElement(bubble.GridPosition);
                    BubbleFactory.ReturnBubble(bubble);
                }
            }
            
            Finish();
        }

        /// <summary>
        /// Cleans up any resources used by this state.
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();
            
            Array.Clear(_connectedBubbles, 0, _connectedBubbles.Length);
        }
        
        private bool IsSameColorAsLaunchedBubble(Bubble bubble)
        {
            var bubbleLaunched = BubbleShooterGame.Launcher.LastBubbleLaunched;
            if (bubbleLaunched == null)
                return false;
            
            var bubbleLaunchedColorComponent = bubbleLaunched.GetComponent<ColorComponent>();
            var otherBubbleColorComponent = bubble.GetComponent<ColorComponent>();
            if (bubbleLaunchedColorComponent == null || otherBubbleColorComponent == null)
                return false;
            
            return bubbleLaunchedColorComponent.Color == otherBubbleColorComponent.Color;
        }
    }
}