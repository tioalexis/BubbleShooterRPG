using Cyl.Common.Utils;

namespace Cyl.BubbleShooter.Fsm
{
    public class CycleBubbleQueueState : BubbleShooterFsmState
    {
        public const string TransitionEvent = "CycleBubbleQueue";
        
        public override string Name => "CycleBubbleQueue";

        public override void OnEnter()
        {
            base.OnEnter();
            
            BubbleQueue.CycleQueueAsync(Finish).FireAndForget();
        }
    }
}