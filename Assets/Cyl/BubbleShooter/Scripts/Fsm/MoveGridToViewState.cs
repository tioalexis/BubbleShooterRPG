using Cyl.Common.Utils;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Moves the bubble grid to the view, anchoring it to either the top or bottom edge of the view
    /// depending on the remaining rows in the grid.
    /// </summary>
    public class MoveGridToViewState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "MoveGridToView";
        
        /// <summary>
        /// How fast the bubble grid moves to the view.
        /// </summary>
        public virtual float Duration => 0.5f;

        /// <summary>
        /// Move the bubble grid to the view, anchoring it to either the top or bottom edge of the view
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            
            MoveGridToViewAsync(Duration)
                .FireAndForget();
        }
        
        private Vector3 CalculateAnchorPosition()
        {
            var visibleRows = View.CameraSettings.minVisibleRows;
            var gridTransform = BubbleGrid.RootTransform;
            var topMostRow = BubbleGrid.FindTopMostOccupiedRow(); // top of grid
            var bottomMostRow = BubbleGrid.FindBottomMostOccupiedRow(); // bottom of grid
            var totalRows = Mathf.Abs(topMostRow - bottomMostRow);
            var isAnchoredTop = totalRows < visibleRows;
            var offsetPadding = isAnchoredTop ? -BubbleGrid.CellInnerRadius : 0f;
            var anchoredRow = isAnchoredTop ? topMostRow : bottomMostRow;
            var anchorPosition = isAnchoredTop ? View.TopEdgeAnchor.position : View.BottomEdgeAnchor.position;
            var anchoredRowPosition = BubbleGrid.GetWorldPosition(0, anchoredRow);
            var offset = (anchorPosition.y - anchoredRowPosition.y) + offsetPadding;
            
            var targetPosition = gridTransform.position;
            targetPosition.y += offset;
            return targetPosition;
        }
        
        private async Awaitable MoveGridToViewAsync(float duration)
        {
            var elapsedTime = 0f;
            var gridTransform = BubbleGrid.RootTransform;
            var startPosition = gridTransform.position.y;
            var targetPosition = CalculateAnchorPosition();
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                var newPosition = gridTransform.position;
                newPosition.y = Mathf.Lerp(startPosition, targetPosition.y, t);
                gridTransform.position = newPosition;
                await Awaitable.EndOfFrameAsync();
            }
        
            gridTransform.position = targetPosition;
            
            Finish();
        }
    }
}