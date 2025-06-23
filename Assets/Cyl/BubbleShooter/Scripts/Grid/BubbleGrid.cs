using Cyl.BubbleShooter.Bubbles;
using Cyl.Hexagons;
using UnityEngine;

namespace Cyl.BubbleShooter.Grid
{
    public class BubbleGrid : HexGrid<Bubble>
    {
        [SerializeField] private int width = 11;
        [SerializeField] private int height = 20;
        [SerializeField] private float scale = 1f;
        [SerializeField] private Transform bubbleRoot;
        
        private Bubble[] _bubbles;

        /// <inheritdoc/>
        public override Bubble[] Elements => _bubbles;
        
        /// <inheritdoc/>
        public override int Width => width;
        
        /// <inheritdoc/>
        public override int Height => height;

        /// <inheritdoc/>
        public override float CellUnitScale => scale;

        private void Awake()
        {
            _bubbles = new Bubble[width * height];
            Debug.Log("BubbleGrid initialized with " + _bubbles.Length + " slots.");
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
            
            var parent = bubbleRoot ?? transform;
            element.transform.SetParent(parent);
            element.transform.position = GetWorldPosition(col, row);
            element.name = $"Bubble ({col}, {row})";
            
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
                Destroy(element.gameObject);
            }
            
            return true;
        }
    }
}