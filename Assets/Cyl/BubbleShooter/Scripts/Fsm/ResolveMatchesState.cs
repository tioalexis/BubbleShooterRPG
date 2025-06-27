using System;
using System.Collections.Generic;
using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;
using Cyl.Common.Utils;
using UnityEngine;

namespace Cyl.BubbleShooter.Fsm
{
    /// <summary>
    /// Handles the resolution of matches in the Bubble Shooter game.
    /// Finds connected bubbles of the same color as the last launched bubble,
    /// and removes them from the grid.
    /// </summary>
    public class ResolveMatchesState : BubbleShooterFsmState
    {
        /// <inheritdoc />
        public override string Name => "ResolveMatches";

        private readonly Bubble[] _connectedBubbles = new Bubble[320];
        
        /// <summary>
        /// Finds all bubbles connected to the last launched bubble that have the same color,
        /// and removes them from the grid.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            
            var bubbleLaunched = BubbleLauncher.LastBubbleLaunched;
            if (bubbleLaunched == null)
            {
                Finish();
                return;
            }
            
            var minRequiredBubblesForMatch = View.MatchResolutionSettings.minRequiredBubblesForMatch;
            var numConnectedBubbles = BubbleGrid.FindConnectedBubblesNonAlloc(
                bubbleLaunched.GridPosition, IsSameColorAsLaunchedBubble, _connectedBubbles);
            if (numConnectedBubbles < minRequiredBubblesForMatch)
            {
                Finish();
                return;
            }
            
            // Find the bubbles that are connected to the last launched bubble
            var damageAppliedToMatchedBubbles = View.MatchResolutionSettings.damageAppliedToMatchedBubbles;
            for (var i = 0; i < numConnectedBubbles; i++)
            {
                var bubble = _connectedBubbles[i];
                if (bubble && bubble.TryGetBubbleComponent<HealthComponent>(out var healthComponent))
                {
                    healthComponent.ApplyDamage(damageAppliedToMatchedBubbles);
                }
            }

            // Remove the connected bubbles from the grid over time, starting from the launched bubble.
            RemoveBubblesAsync(bubbleLaunched, _connectedBubbles, numConnectedBubbles).FireAndForget();
        }

        /// <summary>
        /// Cleans up any resources used by this state.
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();
            
            Array.Clear(_connectedBubbles, 0, _connectedBubbles.Length);
        }

        private async Awaitable RemoveBubblesAsync(Bubble bubbleLaunched, Bubble[] connectedBubbles, int numConnectedBubbles)
        {
            var delayPerDistance = View.MatchResolutionSettings.delayPerRing;
            var delayDecayFactor = View.MatchResolutionSettings.decayFactor;
            var minDelay = View.MatchResolutionSettings.minDelay;
            
            MarkBubblesForDespawn(connectedBubbles, numConnectedBubbles);
            
            var bubblesByDistance = 
                SortBubblesByDistance(bubbleLaunched, connectedBubbles, numConnectedBubbles);
            
            var currentDelay = delayPerDistance;
            foreach (var (distance, bubbles) in bubblesByDistance)
            {
                await ShrinkBubblesAsync(bubbles, distance * currentDelay);
                RemoveBubblesFromGrid(bubbles);
                BubbleShooterGame.DropUnanchoredBubbles();
                currentDelay = Mathf.Max(minDelay, currentDelay * delayDecayFactor);
            }

            await Awaitable.MainThreadAsync();
            await Awaitable.EndOfFrameAsync();

            Finish();
        }
        
        private void MarkBubblesForDespawn(Bubble[] connectedBubbles, int numConnectedBubbles)
        {
            for (var i = 0; i < numConnectedBubbles; i++)
                if (connectedBubbles[i] != null)
                    connectedBubbles[i].IsScheduledForDespawn = true;
        }

        private Dictionary<int, List<Bubble>> SortBubblesByDistance(Bubble bubbleLaunched, Bubble[] connectedBubbles, int numConnectedBubbles)
        {
            var bubblesByDistance = new Dictionary<int, List<Bubble>>();
            for (var i = 0; i < numConnectedBubbles; i++)
            {
                var bubble = connectedBubbles[i];
                if (bubble == null)
                    continue;
                if (bubble.TryGetBubbleComponent<HealthComponent>(out var healthComponent) && healthComponent.Value > 0)
                    continue;
                
                var distanceFromLaunchedBubble = bubbleLaunched.GridPosition.GetDistance(bubble.GridPosition);
                if (!bubblesByDistance.ContainsKey(distanceFromLaunchedBubble))
                    bubblesByDistance[distanceFromLaunchedBubble] = new List<Bubble>();
                bubblesByDistance[distanceFromLaunchedBubble].Add(bubble);
            }
            
            return bubblesByDistance;
        }

        private async Awaitable ShrinkBubblesAsync(List<Bubble> bubbles, float duration)
        {
            var shrinkTarget = Vector3.zero;
            var shrinkElapsed = 0f;
            while (shrinkElapsed < duration)
            {
                var t = shrinkElapsed / duration;
                var targetScale = Vector3.Lerp(Vector3.one, shrinkTarget, t);
                foreach (var bubble in bubbles)
                    if (bubble)
                        bubble.transform.localScale = targetScale;
                shrinkElapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }
        }

        private void RemoveBubblesFromGrid(List<Bubble> bubbles)
        {
            foreach (var bubble in bubbles)
            {
                if (bubble == null)
                    continue;
                
                BubbleGrid.RemoveElement(bubble.GridPosition);
                BubbleFactory.ReturnBubble(bubble);
            }
        }
        
        private bool IsSameColorAsLaunchedBubble(Bubble bubble)
        {
            var bubbleLaunched = BubbleLauncher.LastBubbleLaunched;
            if (bubbleLaunched == null)
                return false;
            
            var bubbleLaunchedColorComponent = bubbleLaunched.GetComponent<ColorComponent>();
            var otherBubbleColorComponent = bubble.GetComponent<ColorComponent>();
            if (bubbleLaunchedColorComponent == null || otherBubbleColorComponent == null)
                return false;
            
            return bubbleLaunchedColorComponent.Color == otherBubbleColorComponent.Color;
        }
    }
}