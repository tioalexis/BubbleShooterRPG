using System;
using System.Collections.Generic;
using Cyl.BubbleShooter.BubbleComponents;
using Cyl.BubbleShooter.Bubbles;
using Cyl.BubbleShooter.Gameplay;
using Cyl.Common.Utils;
using Cyl.Hexagons;
using UnityEngine;

namespace Cyl.BubbleShooter.Grid
{
    /// <summary>
    /// Represents a cluster of bubbles in the game.
    /// A cluster is a collection of bubbles that are connected together.
    /// </summary>
    public class BubbleCluster
    {
        /// <summary>
        /// The bubbles that belong to this cluster.
        /// </summary>
        public HashSet<Bubble> Bubbles { get; }
        
        /// <summary>
        /// Whether this cluster is anchored to the grid or not.
        /// If one bubble in the cluster is anchored, the entire cluster is considered anchored.
        /// An anchored cluster will not fall when unanchored bubbles are dropped.
        /// This value is false by default and is used by <see cref="Cyl.BubbleShooter.Grid.BubbleGrid.FindClusters"/>
        /// to determine if the cluster is anchored or not.
        /// </summary>
        public bool IsAnchored { get; set; }
        
        /// <summary>
        /// Creates a new instance of <see cref="BubbleCluster"/> with the specified bubbles.
        /// </summary>
        /// <param name="bubbles">The bubbles that belong to this cluster.</param>
        public BubbleCluster(HashSet<Bubble> bubbles)
        {
            Bubbles = bubbles;
            IsAnchored = false; // Default to not anchored
        }
    }
    
    /// <summary>
    /// Represents a grid of bubbles in the Bubble Shooter game.
    /// </summary>
    public class BubbleGrid : HexGrid<Bubble>
    {
        [SerializeField] private int width = 11;
        [SerializeField] private int height = 20;
        [SerializeField] private float scale = 1f;
        [SerializeField] private Transform rootTransform;
        
        private Bubble[] _bubbles;

        /// <inheritdoc/>
        public override Bubble[] Elements => _bubbles;
        
        /// <inheritdoc/>
        public override int Width => width;
        
        /// <inheritdoc/>
        public override int Height => height;

        /// <inheritdoc/>
        public override float CellUnitScale => scale;

        /// <inheritdoc/>
        public override Transform RootTransform => rootTransform;// ?? transform;

        private void Awake()
        {
            _bubbles = new Bubble[width * height];
        }

        /// <summary>
        /// Adds a bubble to the grid at the specified coordinates.
        /// The bubble will be positioned in world space based on the grid's cell size and direction.
        /// </summary>
        /// <param name="element">The bubble to add.</param>
        /// <param name="col">The column index in the grid.</param>
        /// <param name="row">The row index in the grid.</param>
        /// <returns>True if the bubble was successfully added, false otherwise.</returns>
        public override bool AddElement(Bubble element, int col, int row)
        {
            if (!base.AddElement(element, col, row))
                return false;
            
            var parent = rootTransform ?? transform;
            element.name = $"Bubble ({col}, {row})";
            element.transform.SetParent(parent);
            element.transform.position = GetWorldPosition(col, row);
            element.transform.localScale = Vector3.one * CellUnitScale;
            element.SetColliderEnabled(true);
            
            return true;
        }

        /// <summary>
        /// Removes a bubble from the grid at the specified coordinates and
        /// destroys its GameObject.
        /// </summary>
        /// <param name="col">The column index of the bubble to remove.</param>
        /// <param name="row">The row index of the bubble to remove.</param>
        /// <returns>True if the bubble was successfully removed and destroyed, false otherwise.</returns>
        public override bool RemoveElement(int col, int row)
        {
            var element = GetElement(col, row);
            
            if (!base.RemoveElement(col, row))
                return false;

            if (element)
            {
                element.transform.SetParent(null);
                element.transform.localScale = Vector3.one * CellUnitScale;
                element.SetColliderEnabled(false);
            }
            
            return true;
        }
        
        /// <summary>
        /// Uses breadth-first search to find all connected bubbles starting from the given origin hex.
        /// This version returns a new array containing the found bubbles. If you need to call this method frequently,
        /// consider using <see cref="FindConnectedBubblesNonAlloc"/> instead to avoid unnecessary allocations.
        /// </summary>
        /// <param name="origin">The starting hex position to search from.</param>
        /// <param name="criteria">The criteria to filter bubbles. Only bubbles that match this predicate will be included in the results.</param>
        /// <returns>An array of bubbles that are connected to the origin hex and match the criteria.</returns>
        public Bubble[] FindConnectedBubbles(Hex origin, Predicate<Bubble> criteria)
        {
            var results = new Bubble[width * height];
            FindConnectedBubblesNonAlloc(origin, criteria, results);
            return results;
        }
        
        /// <summary>
        /// Uses breadth-first search to find all connected bubbles starting from the given origin hex.
        /// This version requires a pre-allocated results array to store the found bubbles for performance reasons.
        /// </summary>
        /// <param name="origin">The starting hex position to search from.</param>
        /// <param name="criteria">The criteria to filter bubbles. Only bubbles that match this predicate will be included in the results.</param>
        /// <param name="results">An array to store the found bubbles. It should be large enough to hold all possible connected bubbles.</param>
        /// <returns>The number of bubbles found that match the criteria.</returns>
        public int FindConnectedBubblesNonAlloc(Hex origin, Predicate<Bubble> criteria, Bubble[] results)
        {
            // Make sure the results array is cleared before use
            Array.Clear(results, 0, results.Length);

            // Assign a default criteria if none is provided
            criteria ??= (bubble) => bubble != null;
            
            var count = 0;
            var queue = new Queue<Hex>();
            queue.Enqueue(origin);
            
            var visited = new HashSet<Hex> { origin };
            while (queue.Count > 0)
            {
                var currentHex = queue.Dequeue();
                var bubble = GetElement(currentHex);

                if (bubble == null || !criteria(bubble)) 
                    continue;
                
                // Resize the connected bubbles array if necessary
                if (count >= results.Length)
                {
                    var nextSize = Mathf.NextPowerOfTwo(results.Length * 2);
                    Array.Resize(ref results, nextSize);
                    Debug.LogWarning($"Results array is full. Consider increasing its size. " +
                                     $"Current count: {count}, Max size: {results.Length}, Resized to: {nextSize}");
                }
                
                results[count++] = bubble;
                
                foreach (var direction in Hex.Directions)
                {
                    var neighborHex = currentHex + direction;
                    if (IsValidPosition(neighborHex) && visited.Add(neighborHex))
                    {
                        queue.Enqueue(neighborHex);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Finds all clusters of bubbles in the grid.
        /// A cluster is defined as a group of connected bubbles that are adjacent to each other.
        /// This method uses breadth-first search to find all connected bubbles and groups them into clusters.
        /// It will also determine if each cluster is anchored, meaning at least one bubble in the cluster is
        /// anchored to the ceiling.
        /// </summary>
        /// <returns>A list of <see cref="BubbleCluster"/> objects representing the clusters found in the grid.</returns>
        public List<BubbleCluster> FindClusters()
        {
            var result = new List<BubbleCluster>();
            
            // Find clusters of bubbles first
            var visited = new HashSet<Bubble>();
            for (var row = 0; row < Height; row++)
            for (var col = 0; col < Width; col++)
            {
                var bubble = GetElement(col, row);
                if (bubble == null || visited.Contains(bubble)) 
                    continue;
                
                // Start a new cluster
                var bubbles = new HashSet<Bubble>();
                var queue = new Queue<Bubble>();
                queue.Enqueue(bubble);

                while (queue.Count > 0)
                {
                    var currentBubble = queue.Dequeue();
                    if (visited.Contains(currentBubble))
                        continue;

                    visited.Add(currentBubble);
                    bubbles.Add(currentBubble);

                    // Check neighbors
                    for (var i = 0; i < Hex.Directions.Length; i++)
                    {
                        var neighborHex = currentBubble.GridPosition + Hex.Directions[i];
                        if (!IsValidPosition(neighborHex))
                            continue;
                        
                        var neighborBubble = GetElement(neighborHex);
                        if (neighborBubble != null && !visited.Contains(neighborBubble))
                        {
                            queue.Enqueue(neighborBubble);
                        }
                    }
                }
                
                var cluster = new BubbleCluster(bubbles);
                result.Add(cluster);
            }
            
            // Now check which clusters are anchored
            foreach (var cluster in result)
            {
                foreach (var bubble in cluster.Bubbles)
                {
                    if (!IsBubbleAnchored(bubble)) 
                        continue;
                    
                    cluster.IsAnchored = true;
                    break; // No need to check further bubbles in this cluster
                }
            }
            
            return result;
        }
        
        

        private bool IsBubbleAnchored(Bubble bubble)
        {
            if (bubble == null)
                return false;
            if (bubble.GridPosition.Row == height - 1)
                return true;
            if (bubble.TryGetComponent<AnchorComponent>(out _))
                return true;
            return false;
        }
    }
}