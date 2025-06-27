using System;
using UnityEngine;

namespace Cyl.BubbleShooter.Views
{
    [Serializable]
    public struct CameraSettings
    {
        [Tooltip("Pre-calculated maximum aspect ratio for the camera.")]
        public float maxOrthographicSize;
        
        [Tooltip("Added to the camera's position to ensure it is at the center of the grid.")]
        public Vector2 positionOffset;
        
        [Tooltip("Minimum number of rows that should be visible in the game view.")]
        public int minVisibleRows;
    }
    
    [Serializable]
    public struct MatchResolutionSettings
    {
        [Tooltip("How many bubbles are required to trigger a match?")]
        public int minRequiredBubblesForMatch;

        [Tooltip("How much damage is applied to the bubbles that are part of a match?")]
        public int damageAppliedToMatchedBubbles;
        
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

        [Header("Runtime Settings")] 
        [SerializeField] private CameraSettings cameraSettings;
        public CameraSettings CameraSettings => cameraSettings;
        
        [SerializeField] private MatchResolutionSettings matchResolutionSettings;
        public MatchResolutionSettings MatchResolutionSettings => matchResolutionSettings;
    }
}