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
        /// <summary>
        /// How much damage is applied to each bubble when resolving matches.
        /// </summary>
        private const int DamageAppliedOnMatch = 1;
        
        /// <summary>
        /// How many bubbles are required to form a match.
        /// </summary>
        private const int MinRequiredBubblesForMatch = 3;
        
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
            
            var bubbleLaunched = BubbleShooterGame.Launcher.LastBubbleLaunched;
            var numConnectedBubbles = BubbleGrid.FindConnectedBubblesNonAlloc(
                bubbleLaunched.GridPosition, IsSameColorAsLaunchedBubble, _connectedBubbles);
            if (numConnectedBubbles < MinRequiredBubblesForMatch)
            {
                Finish();
                return;
            }
            
            // Find the bubbles that are connected to the last launched bubble
            for (var i = 0; i < numConnectedBubbles; i++)
            {
                var bubble = _connectedBubbles[i];
                if (bubble && bubble.TryGetBubbleComponent<HealthComponent>(out var healthComponent))
                {
                    healthComponent.ApplyDamage(DamageAppliedOnMatch);
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
            var delayPerDistance = BubbleShooterGame.View.MatchResolutionTiming.delayPerRing;
            var delayDecayFactor = BubbleShooterGame.View.MatchResolutionTiming.decayFactor;
            var minDelay = BubbleShooterGame.View.MatchResolutionTiming.minDelay;
            
            // Sort the bubbles by distance from the launched bubble
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

            // Remove bubbles in order of distance
            var currentDelay = delayPerDistance;
            foreach (var (distance, bubbles) in bubblesByDistance)
            {
                var totalDelay = distance * currentDelay;
                await Awaitable.WaitForSecondsAsync(totalDelay);
                foreach (var bubble in bubbles)
                {
                    if (bubble == null)
                        continue;
                    
                    // TODO: Spawn VFX for bubble removal.
                
                    BubbleGrid.RemoveElement(bubble.GridPosition);
                    BubbleFactory.ReturnBubble(bubble);
                }
                
                currentDelay = Mathf.Max(minDelay, currentDelay * delayDecayFactor);
            }

            await Awaitable.MainThreadAsync();
            await Awaitable.EndOfFrameAsync();

            Finish();
        }
        
        private bool IsSameColorAsLaunchedBubble(Bubble bubble)
        {
            var bubbleLaunched = BubbleShooterGame.Launcher.LastBubbleLaunched;
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