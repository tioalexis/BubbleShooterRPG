using System.Collections.Generic;
using Cyl.BubbleShooter.Bubbles;

namespace Cyl.BubbleShooter.Gameplay
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
}