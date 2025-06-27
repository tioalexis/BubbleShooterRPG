using System;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Fsm;
using Cyl.BubbleShooter.Grid;
using Cyl.BubbleShooter.Views;
using Cyl.Common.Utils;
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

        private int _bubblesBeingDropped;

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
        
        /// <summary>
        /// Finds all clusters of bubbles in the grid, checks if they are anchored,
        /// and if not, drops them down to the bottom of the grid.
        /// </summary>
        /// <param name="onComplete">Invoked when all unanchored bubbles have been dropped.</param>
        public void DropUnanchoredBubbles(Action onComplete = null)
        {
            var clusters = bubbleGrid.FindClusters();
            if (clusters == null || clusters.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _bubblesBeingDropped = 0;
            foreach (var cluster in clusters)
            {
                if (cluster.IsAnchored)
                    continue;
                
                foreach (var bubble in cluster.Bubbles)
                {
                    if (bubble == null || bubble.IsScheduledForDespawn)
                        continue;
                    
                    _bubblesBeingDropped++;
                    DropBubbleAsync(bubble, onComplete).FireAndForget();
                }
            }
            
            if (_bubblesBeingDropped == 0)
            {
                onComplete?.Invoke();
            }
        }
        
        private async Awaitable DropBubbleAsync(Bubble bubble, Action onComplete)
        {
            if (bubble == null)
                return;

            bubbleGrid.RemoveElement(bubble.GridPosition);
            
            var bounceDistanceX = UnityEngine.Random.Range(0.5f, 1f);
            var bounceDistanceY = UnityEngine.Random.Range(0.5f, 1f);
            var bounceDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;
            var bounceSway = UnityEngine.Random.Range(0.5f, 1.5f);
            
            var p0 = bubble.transform.position;
            var p1 = new Vector3(bounceDistanceX * bounceDirection, bounceDistanceY, 0f);
            var p2 = new Vector3(p1.x * bounceSway, -5f, 0f);
            
            var duration = 0.5f;
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                bubble.transform.position = BezierCurve.QuadraticBezier(p0, p0 + p1, p0 + p2, t);
                await Awaitable.NextFrameAsync();
            }
            
            bubbleFactory.ReturnBubble(bubble);

            _bubblesBeingDropped--;
            if (_bubblesBeingDropped <= 0)
            {
                onComplete?.Invoke();
            }
            else
            {
                Debug.Log($"Remaining bubbles being dropped: {_bubblesBeingDropped}");
            }
        }
    }
}
