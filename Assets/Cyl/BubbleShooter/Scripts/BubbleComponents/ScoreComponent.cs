using Cyl.BubbleShooter.Bubbles;
using UnityEngine;

namespace Cyl.BubbleShooter.BubbleComponents
{
    public class ScoreComponent : BubbleComponent
    {
        [SerializeField] private int value = 100;
        
        /// <summary>
        /// The score value associated with this bubble.
        /// </summary>
        public int Value
        {
            get => value;
            set => this.value = value;
        }
    }
}