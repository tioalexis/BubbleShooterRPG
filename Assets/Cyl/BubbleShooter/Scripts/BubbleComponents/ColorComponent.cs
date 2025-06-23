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
                    mapping.gameObject.SetActive(mapping.color == _color);
                    Debug.Log($"gameObject:{mapping.gameObject.name} color:{_color} active:{mapping.gameObject.activeSelf}");
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