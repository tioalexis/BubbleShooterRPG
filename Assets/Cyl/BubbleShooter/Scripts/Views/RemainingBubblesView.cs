using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Grid;
using TMPro;
using UnityEngine;

namespace Cyl.BubbleShooter.Views
{
    public class RemainingBubblesView : MonoBehaviour
    {
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private TMP_Text remainingBubblesText;

        private void Awake()
        {
            if (bubbleGrid == null)
            {
                bubbleGrid = FindFirstObjectByType<BubbleGrid>();
                if (bubbleGrid == null)
                {
                    Debug.LogError("BubbleGrid not found in the scene. Please ensure it is present.");
                    return;
                }
            }
            
            bubbleGrid.OnBubbleAdded += RefreshView;
            bubbleGrid.OnBubbleRemoved += RefreshView;
        }

        private void OnDestroy()
        {
            bubbleGrid.OnBubbleAdded -= RefreshView;
            bubbleGrid.OnBubbleRemoved -= RefreshView;
        }

        private void RefreshView(Bubble bubble)
        {
            var remainingBubbles = bubbleGrid.CountBubbles(bubbleOnGrid => bubbleOnGrid.BubbleType == BubbleType.ColoredBubble);
            remainingBubblesText.text = remainingBubbles.ToString();   
        }
    }
}