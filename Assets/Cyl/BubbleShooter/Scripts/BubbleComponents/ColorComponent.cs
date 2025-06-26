using System;
using Cyl.BubbleShooter.Bubbles;
using UnityEngine;

namespace Cyl.BubbleShooter.BubbleComponents
{
    [Serializable]
    public struct ColorGameObjectMapping
    {
        public BubbleColor color;
        public GameObject gameObject;
    }
    
    public class ColorComponent : BubbleComponent
    {
        [SerializeField] private ColorGameObjectMapping[] colorGameObjectMappings;

        private BubbleColor _color;

        public BubbleColor Color
        {
            get => _color;
            set
            {
                _color = value;
                foreach (var mapping in colorGameObjectMappings)
                {
                    if (mapping.gameObject == null)
                    {
                        // Debug.LogError($"GameObject for color {mapping.color} is not assigned in ColorComponent on bubble {name}.");
                        continue;
                    }
                    
                    mapping.gameObject.SetActive(mapping.color == _color);
                }
            }
        }
        
        public override void Initialize()
        {
            
        }

        public override void OnSpawn()
        {
            
        }

        public override void OnDespawn()
        {
            
        }
    }
}