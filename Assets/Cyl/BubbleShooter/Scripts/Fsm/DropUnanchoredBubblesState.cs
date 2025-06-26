using Cyl.BubbleShooter.Bubbles;
using Cyl.Common.Utils;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// This state is responsible for dropping all unanchored bubbles in the grid.
    /// The state will find all clusters of bubbles in the grid, check if they are anchored,
    /// and if not, drop them down to the bottom of the grid. This state ends when all unanchored bubbles have been dropped.
    /// </summary>
    public class DropUnanchoredBubblesState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "DropUnanchoredBubbles";

        private int _bubblesBeingDropped;

        /// <summary>
        /// Find all clusters of bubbles in the grid, check if they are anchored,
        /// and if not, drop them down to the bottom of the grid. Otherwise, finish the state.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();

            var clusters = BubbleGrid.FindClusters();
            if (clusters is null || clusters.Count == 0)
            {
                Finish();
                return;
            }

            var numUnanchoredClusters = 0;
            foreach (var cluster in clusters)
            {
                if (cluster.IsAnchored)
                    continue;
                
                numUnanchoredClusters++;
                foreach (var bubble in cluster.Bubbles)
                {
                    DropBubbleAsync(bubble).FireAndForget();
                }
            }
            
            if (numUnanchoredClusters == 0)
            {
                Finish();
            }
        }

        private async Awaitable DropBubbleAsync(Bubble bubble)
        {
            await Awaitable.MainThreadAsync();
            
            _bubblesBeingDropped++;
            
            var bounceDistanceX = Random.Range(0.5f, 1f);
            var bounceDistanceY = Random.Range(0.5f, 1f);
            var bounceDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            var bounceSway = Random.Range(0.5f, 1.5f);
            
            var p0 = bubble.transform.position;
            var p1 = new Vector3(bounceDistanceX * bounceDirection, bounceDistanceY, 0f);
            var p2 = new Vector3(p1.x * bounceSway, -5f, 0f);
            
            var duration = 0.5f;
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                bubble.transform.position = BezierCurve.QuadraticBezier(p0, p0 + p1, p0 + p2, t);
                await Awaitable.NextFrameAsync();
            }
            
            BubbleShooterGame.Factory.ReturnBubble(bubble);
            
            _bubblesBeingDropped--;
            if (_bubblesBeingDropped <= 0)
            {
                Finish();
            }
        }
    }
}