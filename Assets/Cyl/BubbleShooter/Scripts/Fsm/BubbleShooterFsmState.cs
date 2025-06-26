using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Gameplay;
using Cyl.BubbleShooter.Grid;
using Cyl.BubbleShooter.Views;
using Cyl.StateMachines;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// The base class for all Bubble Shooter FSM states.
    /// The state is responsible for managing gameplay logic related to the Bubble Shooter game.
    /// </summary>
    public abstract class BubbleShooterFsmState : State
    {
        protected BubbleShooterGame BubbleShooterGame { get; private set; }
        
        protected BubbleLauncher BubbleLauncher => BubbleShooterGame.Launcher;
        
        protected BubbleGrid BubbleGrid => BubbleShooterGame.Grid;
        
        protected BubbleFactory BubbleFactory => BubbleShooterGame.Factory;
        
        protected BubbleQueue BubbleQueue => BubbleShooterGame.Queue;
        
        protected BubbleShooterGameView View => BubbleShooterGame.View;
        
        protected Camera Camera => BubbleShooterGame.Camera;

        /// <summary>
        /// Initializes the state with the given state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine to initialize with.</param>
        public override void Initialize(StateMachine stateMachine)
        {
            base.Initialize(stateMachine);
            
            BubbleShooterGame = stateMachine.GetComponent<BubbleShooterGame>();
        }

        /// <summary>
        /// Invoked when the state is entered.
        /// Override this method to implement custom logic when entering the state.
        /// </summary>
        public override void OnEnter()
        {
            // Intentionally left empty
        }

        /// <summary>
        /// Invoked every frame while the state is active.
        /// Override this method to implement custom logic that should run every frame.
        /// </summary>
        /// <param name="deltaTime">The time since the last frame.</param>
        public override void OnUpdate(float deltaTime)
        {
            // Intentionally left empty
        }

        /// <summary>
        /// Invoked every fixed frame-rate frame while the state is active.
        /// Override this method to implement custom logic that should run every fixed frame.
        /// </summary>
        /// <param name="fixedDeltaTime">The fixed time since the last frame.</param>
        public override void OnFixedUpdate(float fixedDeltaTime)
        {
            // Intentionally left empty
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// Override this method to implement custom logic when exiting the state.
        /// </summary>
        public override void OnExit()
        {
            
        }

        /// <summary>
        /// Invoked when the state is destroyed.
        /// Override this method to clean up any resources or references held by the state.
        /// </summary>
        public override void OnDestroy()
        {
            BubbleShooterGame = null;
        }
        
        protected void Finish()
        {
            StateMachine.FireTransitionEvent(TransitionEvent.Finished);
        }
    }
}