using System;
using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Fsm;
using Cyl.BubbleShooter.Grid;
using Cyl.BubbleShooter.Views;
using Cyl.StateMachines;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cyl.BubbleShooter.Gameplay
{
    public class BubbleShooterGame : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private BubbleLauncher bubbleLauncher;
        [SerializeField] private BubbleQueue bubbleQueue;
        [SerializeField] private BubbleFactory bubbleFactory;
        [SerializeField] private BubbleShooterGameView gameView;
        [SerializeField] private StateMachine stateMachine;
        
        public Camera Camera => gameCamera;
        
        public BubbleGrid Grid => bubbleGrid;
        
        public BubbleQueue Queue => bubbleQueue;
        
        public BubbleFactory Factory => bubbleFactory;
        
        public BubbleLauncher Launcher => bubbleLauncher;
        
        public BubbleShooterGameView View => gameView;

        private void Awake()
        {
            var initializeGameplaySceneState = new InitializeGameplaySceneState();
            var initializeBubbleGridState = new InitializeBubbleGridState();
            var moveGridToViewState = new MoveGridToViewState();
            var readyForActionState = new ReadyForActionState();
            var resolveMatchesState = new ResolveMatchesState();
            var dropUnanchoredBubblesState = new DropUnanchoredBubblesState();
            
            stateMachine.RegisterState(initializeGameplaySceneState);
            stateMachine.RegisterState(initializeBubbleGridState);
            stateMachine.RegisterState(moveGridToViewState);
            stateMachine.RegisterState(readyForActionState);
            stateMachine.RegisterState(resolveMatchesState);
            stateMachine.RegisterState(dropUnanchoredBubblesState);
            
            stateMachine.RegisterTransition(initializeGameplaySceneState, initializeBubbleGridState);
            stateMachine.RegisterTransition(initializeBubbleGridState, moveGridToViewState);
            stateMachine.RegisterTransition(moveGridToViewState, readyForActionState);
            stateMachine.RegisterTransition(readyForActionState, resolveMatchesState);
            stateMachine.RegisterTransition(resolveMatchesState, dropUnanchoredBubblesState);
            stateMachine.RegisterTransition(dropUnanchoredBubblesState, readyForActionState);
            
            stateMachine.InitialStateName = initializeGameplaySceneState.Name;
        }

        private void Start()
        {
            stateMachine.Begin();
        }
    }
}
