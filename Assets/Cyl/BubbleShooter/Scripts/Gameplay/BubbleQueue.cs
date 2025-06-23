using System;
using System.Collections.Generic;
using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Grid;
using UnityEngine;
using Random = System.Random;

namespace Cyl.BubbleShooter.Gameplay
{
    public class BubbleQueue : MonoBehaviour
    {
        [SerializeField] private BubbleFactory bubbleFactory;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private Transform[] spawnPoints;
        
        private readonly Random _random = new();
        private readonly List<BubbleColor> _bubbleColorHistory = new();
        
        private static readonly BubbleColor[] DefaultColorPool = 
        {
            BubbleColor.ColorA,
            BubbleColor.ColorB,
            BubbleColor.ColorC
        };

        public Bubble GenerateBubble()
        {
            var bubbleColor = EvaluateNextBubbleColor();
            var bubble = bubbleFactory.GetBubble(BubbleType.ColoredBubble);
            if (bubble.TryGetBubbleComponent<ColorComponent>(out var colorComponent))
                colorComponent.Color = bubbleColor;
            RecordBubbleColor(bubbleColor);
            return bubble;
        }

        private BubbleColor EvaluateNextBubbleColor()
        {
            var colorPool = new List<BubbleColor>(DefaultColorPool);
            
            // Make sure the next bubble color is not the same as the last three colors
            if (_bubbleColorHistory.Count > DefaultColorPool.Length)
                for (var i = 0; i < _bubbleColorHistory.Count - DefaultColorPool.Length; i++)
                    colorPool.Remove(_bubbleColorHistory[i]);
            
            // Make sure that only colors from the grid are available
            // var colorsInGrid = EvaluateColorsInGrid();
            // if (colorsInGrid.Count > 0)
            //     colorPool.RemoveAll(color => !colorsInGrid.Contains(color));
            //
            // // If we have no more colors to choose from, just use the remaining colors in the grid
            // if (colorPool.Count == 0)
            //     colorPool.AddRange(colorsInGrid);
            
            // If we still have no colors, fallback to the default color pool
            if (colorPool.Count == 0)
                colorPool.AddRange(DefaultColorPool);
            
            // Randomly select a color from the pool
            var randomIndex = _random.Next(colorPool.Count);
            return colorPool[randomIndex];
        }

        private HashSet<BubbleColor> EvaluateColorsInGrid()
        {
            var result = new HashSet<BubbleColor>();

            for (var row = 0; row < bubbleGrid.Height; row++)
            for (var col = 0; col < bubbleGrid.Width; col++)
            {
                var bubble = bubbleGrid.GetElement(col, row);
                if (bubble == null)
                    continue;
                
                if (bubble.TryGetBubbleComponent<ColorComponent>(out var colorComponent))
                    result.Add(colorComponent.Color);
                
                if (result.Count >= DefaultColorPool.Length)
                    return result; // Early exit if we have enough colors
            }
            
            return result;
        }
        
        private void RecordBubbleColor(BubbleColor bubbleColor)
        {
            const int maxHistorySize = 8;
            
            if (_bubbleColorHistory.Count >= maxHistorySize)
                _bubbleColorHistory.RemoveAt(_bubbleColorHistory.Count - 1);
            
            _bubbleColorHistory.Insert(0, bubbleColor);
        }
    }
}