using UnityEngine;

namespace Cyl.BubbleShooter.Views
{
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
    }
}