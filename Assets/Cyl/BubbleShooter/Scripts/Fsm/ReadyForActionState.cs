using Cyl.BubbleShooter.Bubbles;
using Cyl.Common.Utils;
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
            
            BubbleLauncher.OnCycleQueueRequested += OnCycleQueueRequested;
            BubbleLauncher.OnBubbleLanded += OnBubbleLanded;

            if (!BubbleQueue.IsQueueReady())
            {
                BubbleLauncher.AllowInput = false;
                BubbleQueue.PrepareQueueAsync(OnQueueReady).FireAndForget();
            }
        }

        /// <summary>
        /// Unsubscribes from game events
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();
            
            BubbleLauncher.OnCycleQueueRequested -= OnCycleQueueRequested;
            BubbleLauncher.OnBubbleLanded -= OnBubbleLanded;
        }
        
        private void OnQueueReady()
        {
            BubbleLauncher.AllowInput = true;
        }
        
        private void OnCycleQueueRequested()
        {
            StateMachine.FireTransitionEvent(CycleBubbleQueueState.TransitionEvent);
        }

        private void OnBubbleLanded(Bubble bubbleLaunched, Hex landingGridPosition)
        {
            BubbleLauncher.AllowInput = false;
            Finish();
        }
    }
}