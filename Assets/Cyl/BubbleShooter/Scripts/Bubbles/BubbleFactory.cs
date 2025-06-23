using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Cyl.BubbleShooter.Bubbles
{
    public class BubbleFactory : MonoBehaviour
    {
        [SerializeField] private Bubble[] bubblePrefabs;

        private readonly Dictionary<string, Bubble> _bubblePrefabMap = new();
        private readonly Dictionary<string, ObjectPool<Bubble>> _bubblePools = new();

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

                var objectPool = new ObjectPool<Bubble>(
                    () => CreateBubble(key),
                    OnGetBubbleFromPool,
                    OnReturnBubbleToPool,
                    OnDestroyBubble,
                    false, 0, 512
                );
                _bubblePools.Add(key, objectPool);
            }
        }

        public Bubble GetBubble(string bubbleType)
        {
            if (_bubblePools.TryGetValue(bubbleType, out var bubblePool))
                return bubblePool.Get();

            Debug.LogError($"Could not find bubble pool with name: {bubbleType}");
            return null;
        }

        private Bubble CreateBubble(string bubbleType)
        {
            if (!_bubblePrefabMap.TryGetValue(bubbleType, out var bubblePrefab))
            {
                Debug.LogError($"Could not find bubble prefab with name: {bubbleType}");
                return null;
            }

            var bubbleInstance = Instantiate(bubblePrefab);
            bubbleInstance.name = bubbleType;
            return bubbleInstance;
        }

        private void OnGetBubbleFromPool(Bubble bubble)
        {
            if (bubble == null)
                return;

            bubble.OnSpawn();
            bubble.transform.SetParent(null, false);
            bubble.gameObject.SetActive(true);
        }

        private void OnReturnBubbleToPool(Bubble bubble)
        {
            if (bubble == null)
                return;

            bubble.OnDespawn();
            bubble.transform.SetParent(transform, false);
            bubble.gameObject.SetActive(false);
        }

        private void OnDestroyBubble(Bubble bubble)
        {
            if (bubble == null)
                return;

            bubble.OnDespawn();
            bubble.transform.SetParent(transform, false);
            Destroy(bubble.gameObject);
        }
    }
}