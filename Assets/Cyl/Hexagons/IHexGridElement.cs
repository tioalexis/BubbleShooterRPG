namespace Cyl.Hexagons
{
    /// <summary>
    /// Represents an element that can be placed on a hexagonal grid.
    /// </summary>
    public interface IHexGridElement
    {
        /// <summary>
        /// The position of the element in the hexagonal grid.
        /// </summary>
        Hex GridPosition { get; set; }

        /// <summary>
        /// Invoked when the element is added to a hexagonal grid.
        /// </summary>
        /// <param name="grid">The grid to which the element is added.</param>
        void OnAddedToGrid(HexGrid<IHexGridElement> grid);
    }
}