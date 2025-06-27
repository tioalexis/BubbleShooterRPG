using System;
using UnityEngine;

namespace Cyl.Hexagons
{
    /// <summary>
    /// Represents a hexagonal grid in Unity.
    /// </summary>
    /// <typeparam name="TGridElement">The type of elements that can be placed in the grid,
    /// which must implement <see cref="IHexGridElement"/>.</typeparam>
    public abstract class HexGrid<TGridElement> : MonoBehaviour
        where TGridElement : IHexGridElement
    {
        /// <summary>
        /// The elements of the grid, stored in a 1D array for efficient access.
        /// </summary>
        public abstract TGridElement[] Elements { get; }
        
        /// <summary>
        /// The width of the grid in terms of hexagonal columns.
        /// </summary>
        public abstract int Width { get; }
        
        /// <summary>
        /// The height of the grid in terms of hexagonal rows.
        /// </summary>
        public abstract int Height { get; }
        
        /// <summary>
        /// The scale of the hexagonal cells in the grid in Unity units.
        /// </summary>
        public abstract float CellUnitScale { get; }
        
        /// <summary>
        /// The layout of the hexagonal grid, which determines how the hexagons are arranged.
        /// </summary>
        public virtual HexLayout Layout { get; } = new HexLayout();

        /// <summary>
        /// The transform that holds the grid.
        /// </summary>
        public virtual Transform RootTransform => transform;

        /// <summary>
        /// The radius of the outer circle of a hexagonal cell in the grid.
        /// Also referred to as the size of a hexagonal cell.
        /// A scale of 1 results in a cell size of ~0.57735.
        /// </summary>
        public float CellOuterRadius => CellUnitScale / Hex.Sqrt3;
        
        /// <summary>
        /// Short-hand for the size of a hexagonal cell in the grid.
        /// </summary>
        public float CellSize => CellOuterRadius;
        
        /// <summary>
        /// The radius of the inner circle of a hexagonal cell in the grid.
        /// </summary>
        public float CellInnerRadius => CellWidth / 2f;

        /// <summary>
        /// The height of a hexagonal cell in the grid.
        /// This value is specifically to be used for pointy-topped hexagons,
        /// </summary>
        public float CellHeight => CellOuterRadius * 2f;
        
        /// <summary>
        /// The width of a hexagonal cell in the grid.
        /// This value is specifically to be used for pointy-topped hexagons,
        /// </summary>
        public float CellWidth => CellOuterRadius * Hex.Sqrt3;

        /// <summary>
        /// Vertical distance between the centers of two adjacent hexagonal cells.
        /// </summary>
        public float VerticalSpacing => CellOuterRadius * 1.5f;
        
        /// <summary>
        /// Horizontal distance between the centers of two adjacent hexagonal cells.
        /// </summary>
        public float HorizontalSpacing => CellWidth;

        /// <summary>
        /// Checks if the specified hexagonal coordinate is a valid position on the grid.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate to check.</param>
        /// <returns>True if the position is valid, false otherwise.</returns>
        public bool IsValidPosition(Hex hex)
        {
            return IsValidPosition(hex.Col, hex.Row);
        }
        
        /// <summary>
        /// Checks if the specified 2D grid position is a valid position on the grid.
        /// </summary>
        /// <param name="col">The x-coordinate in the grid.</param>
        /// <param name="row">The y-coordinate in the grid.</param>
        /// <returns>True if the position is valid, false otherwise.</returns>
        public bool IsValidPosition(int col, int row)
        {
            return col >= 0 && col < Width && row >= 0 && row < Height;
        }
        
        /// <summary>
        /// Retrieves the element at the specified hexagonal coordinate.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate to retrieve the element from.</param>
        /// <returns>The element at the specified coordinate, or null if the position is invalid or unoccupied.</returns>
        public TGridElement GetElement(Hex hex)
        {
            return GetElement(hex.Col, hex.Row);
        }
        
        /// <summary>
        /// Retrieves the element at the specified 2D grid position.
        /// </summary>
        /// <param name="col">The x-coordinate in the grid.</param>
        /// <param name="row">The y-coordinate in the grid.</param>
        /// <returns>The element at the specified position, or null if the position is invalid or unoccupied.</returns>
        public TGridElement GetElement(int col, int row)
        {
            return Elements[GetIndex(col, row)];
        }
        
        /// <summary>
        /// Retrieves the neighbors of a element at the specified hexagonal coordinate.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate of the element whose neighbors are to be retrieved.</param>
        /// <returns>An array of neighboring elements, or null if there are no neighbors.</returns>
        public TGridElement[] GetNeighbors(Hex hex)
        {
            var neighbors = new TGridElement[6];
            GetNeighborsNonAlloc(hex, neighbors);
            return neighbors;
        }
        
        /// <summary>
        /// Retrieves the neighbors of a element at the specified hexagonal coordinate, filling the provided array.
        /// This method does not allocate memory for the neighbors array, allowing for more efficient memory usage.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate of the element whose neighbors are to be retrieved.</param>
        /// <param name="neighbors">The array to fill with neighboring elements. Must have a length of at least 6.</param>
        /// <returns>The number of neighbors found.</returns>
        /// <exception cref="ArgumentException">Thrown if the neighbors array is null or has a length less than 6.</exception>
        public int GetNeighborsNonAlloc(Hex hex, TGridElement[] neighbors)
        {
            if (neighbors == null || neighbors.Length < 6)
            {
                throw new ArgumentException("Neighbors array must have a length of at least 6.");
            }

            var count = 0;
            foreach (var direction in Hex.Directions)
            {
                var neighborPos = hex + direction;
                if (!IsValidPosition(neighborPos))
                    continue;
                
                var element = GetElement(neighborPos);
                if (element == null)
                    continue;
                
                neighbors[count++] = element;
            }
            return count;
        }

        /// <summary>
        /// Counts the number of neighbors for a element at the specified hexagonal coordinate.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate of the element whose neighbors are to be counted.</param>
        /// <returns>The number of neighbors found at the specified coordinate.</returns>
        public int CountNeighbors(Hex hex)
        {
            return CountNeighbors(hex.Col, hex.Row);
        }
        
        /// <summary>
        /// Counts the number of neighbors for a element at the specified 2D grid position.
        /// </summary>
        /// <param name="col">The x-coordinate in the grid of the element whose neighbors are to be counted.</param>
        /// <param name="row"> The y-coordinate in the grid of the element whose neighbors are to be counted.</param>
        /// <returns>The number of neighbors found at the specified position.</returns>
        public int CountNeighbors(int col, int row)
        {
            var count = 0;
            foreach (var direction in Hex.Directions)
            {
                var neighborCol = col + direction.Col;
                var neighborRow = row + direction.Row;
                if (!IsValidPosition(neighborCol, neighborRow))
                    continue;
                
                if (GetElement(neighborCol, neighborRow) != null)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// Retrieves the world position of a hexagonal coordinate.
        /// </summary>
        /// <param name="coord">The hexagonal coordinate to convert to a world position.</param>
        /// <returns>The world position corresponding to the hexagonal coordinate.</returns>
        public Vector3 GetWorldPosition(Hex coord)
        {
            return GetWorldPosition(coord.Col, coord.Row);
        }
        
        /// <summary>
        /// Retrieves the world position of a hexagonal cell at the specified column and row.
        /// </summary>
        /// <param name="col">The column index of the hexagonal cell.</param>
        /// <param name="row">The row index of the hexagonal cell.</param>
        /// <returns>The world position of the hexagonal cell at the specified column and row.</returns>
        public Vector3 GetWorldPosition(int col, int row)
        {
            return Layout.OffsetHexToWorld(new Hex(col, row), RootTransform.position, CellSize);
        }
        
        /// <summary>
        /// Retrieves the hexagonal coordinate corresponding to a world position.
        /// </summary>
        /// <param name="worldPosition">The world position to convert to a hexagonal coordinate.</param>
        /// <returns>The grid position in hexagonal coordinates corresponding to the world position.</returns>
        public Hex GetGridPosition(Vector3 worldPosition)
        {
            return Layout.WorldToAxialHex(worldPosition, RootTransform.position, CellSize);
        }
        
        /// <summary>
        /// Adds a element to the grid at the specified hexagonal coordinate.
        /// The element will be placed at the world position corresponding to the hexagonal coordinate
        /// and scaled according to the grid's cell size.
        /// </summary>
        /// <param name="element"> The element to add to the grid.</param>
        /// <param name="pos"> The hexagonal coordinate where the element should be placed.</param>
        /// <returns>True if the element was successfully added, false if the position is invalid or already occupied.</returns>
        public virtual bool AddElement(TGridElement element, Vector2Int pos)
        {
            return AddElement(element, pos.x, pos.y);
        }
        
        /// <summary>
        /// Adds a element to the grid at the specified 2D grid position.
        /// </summary>
        /// <param name="element"> The element to add to the grid.</param>
        /// <param name="col">The x-coordinate in the grid where the element should be placed.</param>
        /// <param name="row">The y-coordinate in the grid where the element should be placed.</param>
        /// <returns>True if the element was successfully added, false if the position is invalid or already occupied.</returns>
        public virtual bool AddElement(TGridElement element, int col, int row)
        {
            if (!IsValidPosition(col, row))
            {
                Debug.LogError($"Attempted to add element at invalid position: ({col}, {row})");
                return false;
            }

            var index = GetIndex(col, row);
            if (!IsValidIndex(index))
            {
                Debug.LogError($"Attempted to add element at invalid position: ({col}, {row}) = {index}");
                return false;
            }
            
            if (Elements[index] != null)
            {
                Debug.LogError($"Attempted to add element at occupied position: ({col}, {row}) = {index}");
                return false; // Position already occupied
            }

            Elements[index] = element;
            element.GridPosition = new Hex(col, row);
            return true;
        }

        /// <summary>
        /// Removes the element at the specified hexagonal coordinate.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate of the element to remove.</param>
        /// <returns>True if the element was successfully removed, false if the element is null, not found, or already unoccupied.</returns>
        public virtual bool RemoveElement(Hex hex)
        {
            return RemoveElement(hex.Col, hex.Row);
        }

        /// <summary>
        /// Removes the element at the specified hexagonal coordinate.
        /// </summary>
        /// <param name="col">The x-coordinate in the grid of the element to remove.</param>
        /// <param name="row">The y-coordinate in the grid of the element to remove.</param>
        /// <returns>True if the element was successfully removed, false if the element is null, not found, or already unoccupied.</returns>
        public virtual bool RemoveElement(int col, int row)
        {
            var element = GetElement(col, row);
            if (element == null)
            {
                Debug.LogError($"Attempted to remove element at invalid position: ({col}, {row})");
                return false; // Element not found
            }
            
            var index = GetIndex(col, row);
            Elements[index] = default;
            return true;
        }
        
        /// <summary>
        /// Calculates the width of the grid in world units.
        /// </summary>
        /// <returns>A Vector2 representing the width and height of the grid in world units.</returns>
        public Vector2 CalculateGridSize()
        {
            var width = (Width + 0.5f) * CellWidth;
            var height = (Height * VerticalSpacing) + (CellHeight / 4f);
            return new Vector2(width, height);
        }

        /// <summary>
        /// Finds the first occupied row in the grid.
        /// This would be the bottom-most row that contains at least one occupied cell.
        /// </summary>
        /// <returns>The index of the first occupied row, or -1 if no rows are occupied.</returns>
        public int FindBottomMostOccupiedRow()
        {
            for (var row = 0; row < Height; row++)
            {
                var occupied = false;
                for (var col = 0; col < Width; col++)
                {
                    if (GetElement(col, row) == null)
                        continue;
                    
                    occupied = true;
                    break;
                }

                if (occupied)
                    return row;
            }

            return -1;
        }
        
        /// <summary>
        /// Finds the last occupied row in the grid.
        /// This would be the top-most row that contains at least one occupied cell.
        /// </summary>
        /// <returns>The index of the last occupied row, or -1 if no rows are occupied.</returns>
        public int FindTopMostOccupiedRow()
        {
            for (var row = Height - 1; row >= 0; row--)
            {
                var occupied = false;
                for (var col = 0; col < Width; col++)
                {
                    if (GetElement(col, row) == null)
                        continue;
                    
                    occupied = true;
                    break;
                }
                
                if (occupied)
                    return row;
            }
            
            return -1;
        }
        
        /// <summary>
        /// Finds the nearest unoccupied grid position to the specified world position.
        /// This may fail if the grid is completely occupied or if the search radius is exceeded.
        /// </summary>
        /// <param name="worldPosition">The world position to search from.</param>
        /// <param name="gridPosition">The output parameter that will hold the nearest unoccupied grid position if found.</param>
        /// <returns>True if an unoccupied position was found, false otherwise.</returns>
        public bool FindNearestUnoccupiedGridPosition(Vector3 worldPosition, out Hex gridPosition)
        {
            var hexCoord = GetGridPosition(worldPosition);
            if (IsValidPosition(hexCoord) && GetElement(hexCoord) == null)
            {
                gridPosition = hexCoord;
                return true; // The position is already valid
            }

            // Search for the nearest unoccupied position
            const int maxRadius = 2; // no need to search too far
            for (var radius = 1; radius < maxRadius; radius++)
            for (var col = -radius; col <= radius; col++)
            for (var row = -radius; row <= radius; row++)
            {
                if (Math.Abs(col) + Math.Abs(row) > radius)
                    continue; // Skip positions outside the current radius

                var newCol = hexCoord.Col + col;
                var newRow = hexCoord.Row + row;
                if (!IsValidPosition(newCol, newRow) || 
                    GetElement(newCol, newRow) != null ||
                    CountNeighbors(newCol, newRow) < 1)
                    continue;
                        
                gridPosition = new Hex(newCol, newRow);
                return true; // Found an unoccupied position
            }

            gridPosition = default;
            return false; // No unoccupied position found
        }
        
        private int GetIndex(int col, int row)
        {
            return row * Width + col;
        }
        
        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < Elements.Length;
        }

        protected void OnDrawGizmosSelected()
        {
            // Draw the cells as circles
            Gizmos.color = Color.gray1;
            for (var col = 0; col < Width; col++)
            {
                for (var row = 0; row < Height; row++)
                {
                    var position = GetWorldPosition(col, row);
                    Gizmos.DrawWireSphere(position, CellInnerRadius);
                }
            }

            if (Application.isPlaying)
            {
                // Draw line indicating the first occupied row
                var firstOccupiedRow = FindBottomMostOccupiedRow();
                if (firstOccupiedRow >= 0)
                {
                    var firstOccupiedPositionA = GetWorldPosition(0, firstOccupiedRow);
                    var firstOccupiedPositionB = GetWorldPosition(Width - 1, firstOccupiedRow);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(firstOccupiedPositionA, firstOccupiedPositionB);
                }
            
                // Draw line indicating the last occupied row
                var lastOccupiedRow = FindTopMostOccupiedRow();
                if (lastOccupiedRow >= 0)
                {
                    var lastOccupiedPositionA = GetWorldPosition(0, lastOccupiedRow);
                    var lastOccupiedPositionB = GetWorldPosition(Width - 1, lastOccupiedRow);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(lastOccupiedPositionA, lastOccupiedPositionB);
                }
            }
        }
    }
}