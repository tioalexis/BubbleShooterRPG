using System;
using UnityEngine;

namespace Cyl.BubbleShooter.Views
{
    [Serializable]
    public struct MatchResolutionTiming
    {
        [Tooltip("Delay per ring when resolving matches.")]
        public float delayPerRing;

        [Tooltip("Decay factor for the match resolution delay.")]
        public float decayFactor;

        [Tooltip("Minimum delay for match resolution.")]
        public float minDelay;
    }
    
    /// <summary>
    /// Holds references to the game view components in the Bubble Shooter game.
    /// </summary>
    public class BubbleShooterGameView : MonoBehaviour
    {
        [Header("Walls")]
        [SerializeField] private Transform ceiling;
        public Transform Ceiling => ceiling;
        
        [SerializeField] private Transform leftWall;
        public Transform LeftWall => leftWall;
        
        [SerializeField] private Transform rightWall;
        public Transform RightWall => rightWall;
        
        [Header("Positioning")]
        [SerializeField] private Transform topEdgeAnchor;
        public Transform TopEdgeAnchor => topEdgeAnchor;
        
        [SerializeField] private Transform bottomEdgeAnchor;
        public Transform BottomEdgeAnchor => bottomEdgeAnchor;

        [Header("Timing Settings")] 
        [SerializeField] private MatchResolutionTiming matchResolutionTiming;
        public MatchResolutionTiming MatchResolutionTiming => matchResolutionTiming;
    }
}