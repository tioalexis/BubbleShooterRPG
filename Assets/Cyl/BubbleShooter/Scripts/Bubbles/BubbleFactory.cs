using System.Collections.Generic;
using UnityEngine;

namespace Cyl.BubbleShooter.Bubbles
{
    /// <summary>
    /// This class manages the creation and pooling of bubble instances.
    /// When a bubble is requested, it checks if a suitable instance is available in the pool.
    /// If not, it creates a new instance from a prefab. Otherwise, it returns an existing instance from the pool.
    /// Once a bubble is no longer needed, it can be returned to the pool for reuse.
    /// </summary>
    public class BubbleFactory : MonoBehaviour
    {
        [Tooltip("The maximum number of bubbles that can be pooled for each type.")]
        [SerializeField] private int maxPoolSize = 1024;
        
        [Tooltip("The bubble prefabs to use for creating new bubble instances. " +
                 "If there are duplicates, only the first instance will be used.")]
        [SerializeField] private Bubble[] bubblePrefabs;

        private readonly Dictionary<string, Bubble> _bubblePrefabMap = new();
        private readonly Dictionary<string, Queue<Bubble>> _bubblePools = new();

        private void Awake()
        {
            foreach (var bubblePrefab in bubblePrefabs)
            {
                if (bubblePrefab == null)
                    continue;

                var key = bubblePrefab.name;
                if (!_bubblePrefabMap.TryAdd(key, bubblePrefab))
                {
                    Debug.LogWarning($"Duplicate bubble prefab found: {key}. Only the first will be used.");
                    continue;
                }
                
                var queue = new Queue<Bubble>();
                _bubblePools.Add(key, queue);
            }
        }

        /// <summary>
        /// Retrieves a bubble from the pool based on its type.
        /// If no bubble of that type is available, a new one is created.
        /// </summary>
        /// <param name="bubbleType">The type of bubble to retrieve. This should match the name of a bubble prefab.</param>
        /// <returns>A bubble instance of the requested type.</returns>
        public Bubble GetBubble(string bubbleType)
        {
            if (!_bubblePools.ContainsKey(bubbleType))
            {
                _bubblePools.Add(bubbleType, new Queue<Bubble>());
            }
            
            var pool = _bubblePools[bubbleType];
            if (pool.Count == 0)
            {
                var bubbleInstance = CreateBubble(bubbleType);
                var bubble = bubbleInstance.GetComponent<Bubble>();
                pool.Enqueue(bubble);
            }
            
            var bubbleToGet = pool.Dequeue();
            OnGetBubbleFromPool(bubbleToGet);
            return bubbleToGet;
        }
        
        /// <summary>
        /// Returns a bubble back to the pool.
        /// If the pool for that bubble type exceeds the maximum size,
        /// the bubble will be destroyed instead of returned to the pool.
        /// </summary>
        /// <param name="bubble">The bubble instance to return to the pool.</param>
        public void ReturnBubble(Bubble bubble)
        {
            if (bubble == null)
            {
                return;
            }

            var bubbleType = bubble.BubbleType;
            if (!_bubblePools.ContainsKey(bubbleType))
            {
                _bubblePools.Add(bubbleType, new Queue<Bubble>());
            }
            
            var pool = _bubblePools[bubbleType];
            if (pool.Count >= maxPoolSize)
            {
                OnDestroyBubble(bubble);
                return;
            }
            
            OnReturnBubbleToPool(bubble);
            pool.Enqueue(bubble);
        }

        private GameObject CreateBubble(string bubbleType)
        {
            if (!_bubblePrefabMap.TryGetValue(bubbleType, out var bubblePrefab))
            {
                Debug.LogError($"Could not find bubble prefab with name: {bubbleType}");
                return null;
            }

            var bubbleInstance = Instantiate(bubblePrefab, transform, true);
            bubbleInstance.BubbleType = bubbleType;
            bubbleInstance.name = bubbleType;
            return bubbleInstance.gameObject;
        }

        private void OnGetBubbleFromPool(Bubble bubble)
        {
            if (bubble == null)
            {
                return;
            }
            
            bubble.OnSpawn();
            bubble.transform.SetParent(transform);
            bubble.gameObject.SetActive(true);
        }

        private void OnReturnBubbleToPool(Bubble bubble)
        {
            if (bubble == null)
            {
                return;
            }
            
            bubble.name = bubble.BubbleType;
            bubble.OnDespawn();
            bubble.transform.SetParent(transform);
            bubble.gameObject.SetActive(false);
        }

        private void OnDestroyBubble(Bubble bubble)
        {
            if (bubble == null)
            {
                return;
            }
            
            bubble.OnDespawn();
            bubble.transform.SetParent(transform);
            Destroy(bubble.gameObject);
        }
    }
}