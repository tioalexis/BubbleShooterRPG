using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Gameplay;
using Cyl.BubbleShooter.Grid;
using Cyl.BubbleShooter.Views;
using Cyl.StateMachines;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    public abstract class BubbleShooterFsmState : State
    {
        protected BubbleShooterGame BubbleShooterGame { get; private set; }
        
        protected BubbleLauncher BubbleLauncher => BubbleShooterGame.Launcher;
        
        protected BubbleGrid BubbleGrid => BubbleShooterGame.Grid;
        
        protected BubbleFactory BubbleFactory => BubbleShooterGame.Factory;
        
        protected BubbleQueue BubbleQueue => BubbleShooterGame.Queue;
        
        protected BubbleShooterGameView View => BubbleShooterGame.View;
        
        protected Camera Camera => BubbleShooterGame.Camera;

        public override void Initialize(StateMachine stateMachine)
        {
            base.Initialize(stateMachine);
            
            BubbleShooterGame = stateMachine.GetComponent<BubbleShooterGame>();
        }

        public override void OnEnter()
        {
            // Debug.Log($"Entered state: {Name}");
        }

        public override void OnUpdate(float deltaTime)
        {
            
        }

        public override void OnFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public override void OnExit()
        {
            
        }

        public override void OnDestroy()
        {
            BubbleShooterGame = null;
        }

        public void Finish()
        {
            StateMachine.FireTransitionEvent(TransitionEvent.Finished);
        }
    }
}