using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Fsm;
using Cyl.BubbleShooter.Grid;
using Cyl.BubbleShooter.Views;
using Cyl.StateMachines;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cyl.BubbleShooter.Gameplay
{
    [RequireComponent(typeof(StateMachine))]
    public class BubbleShooterGame : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private BubbleLauncher bubbleLauncher;
        [SerializeField] private BubbleQueue bubbleQueue;
        [SerializeField] private BubbleFactory bubbleFactory;
        [SerializeField] private BubbleShooterGameView gameView;
        
        private StateMachine _stateMachine;
        
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
            var introGridToViewState = new IntroGridToViewState();
            var readyForActionState = new ReadyForActionState();
            var cycleBubbleQueueState = new CycleBubbleQueueState();
            var resolveMatchesState = new ResolveMatchesState();
            var dropUnanchoredBubblesState = new DropUnanchoredBubblesState();
            var moveGridToViewState = new MoveGridToViewState();
            
            if (!TryGetComponent(out _stateMachine))
                _stateMachine = gameObject.AddComponent<StateMachine>();
            
            _stateMachine.RegisterState(initializeGameplaySceneState);
            _stateMachine.RegisterState(initializeBubbleGridState);
            _stateMachine.RegisterState(introGridToViewState);
            _stateMachine.RegisterState(readyForActionState);
            _stateMachine.RegisterState(cycleBubbleQueueState);
            _stateMachine.RegisterState(resolveMatchesState);
            _stateMachine.RegisterState(dropUnanchoredBubblesState);
            _stateMachine.RegisterState(moveGridToViewState);
            
            _stateMachine.RegisterTransition(initializeGameplaySceneState, initializeBubbleGridState);
            _stateMachine.RegisterTransition(initializeBubbleGridState, introGridToViewState);
            _stateMachine.RegisterTransition(introGridToViewState, readyForActionState);
            _stateMachine.RegisterTransition(readyForActionState, resolveMatchesState);
            _stateMachine.RegisterTransition(readyForActionState, cycleBubbleQueueState, CycleBubbleQueueState.TransitionEvent);
            _stateMachine.RegisterTransition(cycleBubbleQueueState, readyForActionState);
            _stateMachine.RegisterTransition(resolveMatchesState, dropUnanchoredBubblesState);
            _stateMachine.RegisterTransition(dropUnanchoredBubblesState, moveGridToViewState);
            _stateMachine.RegisterTransition(moveGridToViewState, readyForActionState);
            
            _stateMachine.InitialStateName = initializeGameplaySceneState.Name;
        }

        private void Start()
        {
            _stateMachine.Begin();
        }
    }
}
