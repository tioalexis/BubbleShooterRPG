using Cyl.BubbleShooter.Bubbles;
using Cyl.Hexagons;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Prepares the game for the player to launch a bubble
    /// then waits for the player to launch a bubble.
    /// </summary>
    public class ReadyForActionState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "ReadyForAction";
        
        /// <summary>
        /// Wait for the player to launch a bubble.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            
            BubbleShooterGame.Launcher.OnBubbleLanded += OnBubbleLanded;
            
            BubbleShooterGame.Queue.PrepareQueue();
        }

        /// <summary>
        /// Unsubscribes from game events
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();
            
            BubbleShooterGame.Launcher.OnBubbleLanded -= OnBubbleLanded;
        }

        private void OnBubbleLanded(Bubble bubbleLaunched, Hex landingGridPosition)
        {
            Finish();
        }
    }
}