using System;
using Cyl.BubbleShooter.Bubbles;
using UnityEngine;

namespace Cyl.BubbleShooter.BubbleComponents
{
    /// <summary>
    /// Maps a color name to a GameObject that should be activated when the color is set.
    /// </summary>
    [Serializable]
    public struct ColorGameObjectMapping
    {
        public string colorName;
        public GameObject gameObject;
    }
    
    /// <summary>
    /// Represents the color component of a bubble.
    /// </summary>
    public class ColorComponent : BubbleComponent
    {
        [Tooltip("Maps color names to GameObjects that should be activated when the color is set.")]
        [SerializeField] private ColorGameObjectMapping[] colorGameObjectMappings;

        private string _color;

        /// <summary>
        /// The name of the color of the bubble.
        /// </summary>
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                foreach (var mapping in colorGameObjectMappings)
                {
                    if (mapping.gameObject == null)
                        continue;
                    
                    mapping.gameObject.SetActive(mapping.colorName == _color);
                }
            }
        }
    }
}