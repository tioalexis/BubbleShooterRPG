using System;
using System.Collections.Generic;
using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

namespace Cyl.BubbleShooter.Gameplay
{
    /// <summary>
    /// The queue is responsible for managing the bubbles that are currently in the queue to be launched.
    /// It is also responsible for generating new bubbles and ensuring that the queue is always filled with bubbles.
    /// </summary>
    public class BubbleQueue : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private BubbleFactory bubbleFactory;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private Collider2D touchCollider;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private string[] defaultColorPool;
        
        [Header("Animation")]
        [SerializeField] private float prepareQueueAnimationDuration = 0.1f;
        [SerializeField] private float cycleQueueAnimationDuration = 0.1f;
        
        private readonly Random _random = new();
        private readonly List<string> _bubbleColorHistory = new();
        private Bubble[] _queue;
        
        public Bubble ActiveBubble => _queue[0];
        
        public Transform ActiveBubbleSpawnPoint => spawnPoints[0];
        
        public Collider2D TouchCollider => touchCollider;

        private void Awake()
        {
            _queue = new Bubble[spawnPoints.Length];
        }
        
        /// <summary>
        /// Checks if the queue is filled with bubbles.
        /// </summary>
        /// <returns>True if all bubbles in the queue are not null, false otherwise.</returns>
        public bool IsQueueReady()
        {
            foreach (var bubble in _queue)
                if (bubble == null)
                    return false;

            return true;
        }
        
        public async Awaitable PrepareQueueAsync(Action onComplete = null)
        {
            // Move remaining bubbles to the beginning of the queue
            for (var i = 0; i < _queue.Length; i++)
            {
                if (_queue[i] != null) 
                    continue;
                
                for (var j = i + 1; j < _queue.Length; j++)
                {
                    if (_queue[j] == null) 
                        continue;
                    
                    _queue[i] = _queue[j];
                    _queue[j] = null;
                    break;
                }
            }
            
            // Generate new bubbles for empty slots
            for (var i = 0; i < _queue.Length; i++)
            {
                if (_queue[i] != null) 
                    continue;
                
                var bubble = GenerateBubble();
                if (bubble == null)
                    continue;
                
                bubble.transform.SetParent(transform);
                bubble.transform.position = spawnPoints[i].position;
                bubble.SetColliderEnabled(false);
                _queue[i] = bubble;
            }
            
            // Move the bubbles to their respective spawn points
            // while scaling them to their final size
            var duration = prepareQueueAnimationDuration;
            var elapsedTime = 0f;
            var targetScale = Vector3.one * bubbleGrid.CellUnitScale;
            while (elapsedTime < duration)
            {
                var t = Mathf.Clamp01(elapsedTime / duration);
                for (var i = 0; i < _queue.Length; i++)
                {
                    if (_queue[i] == null) 
                        continue;
                    
                    var bubble = _queue[i];
                    bubble.transform.position = Vector3.Lerp(bubble.transform.position, spawnPoints[i].position, t);
                    bubble.transform.rotation = Quaternion.Lerp(bubble.transform.rotation, spawnPoints[i].rotation, t);
                    bubble.transform.localScale = Vector3.Lerp(bubble.transform.localScale, targetScale, t);
                }
                elapsedTime += Time.deltaTime;
                await Awaitable.EndOfFrameAsync();
            }
            
            for (var i = 0; i < _queue.Length; i++)
            {
                if (_queue[i] == null) 
                    continue;
                    
                var bubble = _queue[i];
                bubble.transform.position = spawnPoints[i].position;
                bubble.transform.rotation = spawnPoints[i].rotation;
                bubble.transform.localScale = targetScale;
            }

            await Awaitable.MainThreadAsync();
            
            onComplete?.Invoke();
        }

        public async Awaitable CycleQueueAsync(Action onComplete = null)
        {
            if (_queue.Length == 0)
            {
                await Awaitable.MainThreadAsync();
                onComplete?.Invoke();
                return;
            }
            
            var firstBubble = _queue[0];
            for (var i = 0; i < _queue.Length - 1; i++)
            {
                _queue[i] = _queue[i + 1];
            }
            _queue[^1] = firstBubble;
            
            // Move the bubbles in the world to their respective spawn points
            var duration = cycleQueueAnimationDuration;
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                var t = Mathf.Clamp01(elapsedTime / duration);
                for (var i = 0; i < _queue.Length; i++)
                {
                    if (_queue[i] == null) 
                        continue;
                    
                    var bubble = _queue[i];
                    bubble.transform.position = Vector3.Lerp(bubble.transform.position, spawnPoints[i].position, t);
                    bubble.transform.rotation = Quaternion.Lerp(bubble.transform.rotation, spawnPoints[i].rotation, t);
                }
                elapsedTime += Time.deltaTime;
                await Awaitable.EndOfFrameAsync();
            }
            
            await Awaitable.MainThreadAsync();
            onComplete?.Invoke();
        }

        public Bubble GenerateBubble()
        {
            var bubble = bubbleFactory.GetBubble(BubbleType.ColoredBubble);
            if (bubble == null)
            {
                Debug.LogError("Failed to generate bubble: BubbleFactory returned null.");
                return null;
            }
            
            var bubbleColor = EvaluateNextBubbleColor();
            if (bubble.TryGetBubbleComponent<ColorComponent>(out var colorComponent))
                colorComponent.Color = bubbleColor;
            
            RecordBubbleColor(bubbleColor);
            return bubble;
        }
        
        public void RemoveBubble(Bubble bubble)
        {
            for (var i = 0; i < _queue.Length; i++)
            {
                if (_queue[i] != bubble) 
                    continue;
                
                _queue[i] = null;
                return;
            }
            
            Debug.LogWarning("Bubble not found in queue: " + bubble.name);
        }

        private string EvaluateNextBubbleColor()
        {
            var colorPool = new List<string>(defaultColorPool);
            
            // Make sure the next bubble color is not the same as the last three colors
            if (_bubbleColorHistory.Count > defaultColorPool.Length)
                for (var i = 0; i < _bubbleColorHistory.Count - defaultColorPool.Length; i++)
                    colorPool.Remove(_bubbleColorHistory[i]);
            
            // Make sure that only colors from the grid are available
            // var colorsInGrid = EvaluateColorsInGrid();
            // if (colorsInGrid.Count > 0)
            //     colorPool.RemoveAll(color => !colorsInGrid.Contains(color));
            //
            // // If we have no more colors to choose from, just use the remaining colors in the grid
            // if (colorPool.Count == 0)
            //     colorPool.AddRange(colorsInGrid);
            
            // If we still have no colors, fallback to the default color pool
            if (colorPool.Count == 0)
                colorPool.AddRange(defaultColorPool);
            
            // Randomly select a color from the pool
            var randomIndex = _random.Next(colorPool.Count);
            return colorPool[randomIndex];
        }

        private HashSet<string> EvaluateColorsInGrid()
        {
            var result = new HashSet<string>();

            for (var row = 0; row < bubbleGrid.Height; row++)
            for (var col = 0; col < bubbleGrid.Width; col++)
            {
                var bubble = bubbleGrid.GetElement(col, row);
                if (bubble == null)
                    continue;
                
                if (bubble.TryGetBubbleComponent<ColorComponent>(out var colorComponent))
                    result.Add(colorComponent.Color);
                
                if (result.Count >= defaultColorPool.Length)
                    return result; // Early exit if we have enough colors
            }
            
            return result;
        }
        
        private void RecordBubbleColor(string bubbleColor)
        {
            const int maxHistorySize = 8;
            
            if (_bubbleColorHistory.Count >= maxHistorySize)
                _bubbleColorHistory.RemoveAt(_bubbleColorHistory.Count - 1);
            
            _bubbleColorHistory.Insert(0, bubbleColor);
        }
    }
}