using Cyl.BubbleShooter.Bubbles;
using Cyl.Hexagons;

namespace Cyl.BubbleShooter.Fsm
{
    public class ReadyForActionState : BubbleShooterFsmState
    {
        public override string Name => "ReadyForAction";
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            BubbleShooterGame.Launcher.OnBubbleLanded += OnBubbleLanded;
            
            BubbleShooterGame.Queue.PrepareQueue();
        }

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