using System.Collections.Generic;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Gameplay;
using Cyl.Common.Utils;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    public class DropUnanchoredBubblesState : BubbleShooterFsmState
    {
        public override string Name => "DropUnanchoredBubbles";

        private int _bubblesBeingDropped;

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