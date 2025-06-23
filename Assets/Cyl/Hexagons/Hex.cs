using System;
using UnityEngine;

namespace Cyl.Hexagons
{
    /// <summary>
    /// Represents a hexagonal coordinate.
    /// Internally, the coordinates are stored in cube coordinates (q, r, s),
    /// however, only the offset coordinates (Col, Row) are exposed publicly.
    ///
    /// Please refer to https://www.redblobgames.com/grids/hexagons
    /// for a detailed explanation of the hexagonal coordinate system.
    /// </summary>
    public struct Hex
    {
        /// <summary>
        /// Pre-calculated constant for the square root of 3. For performance reasons.
        /// </summary>
        public const float Sqrt3 = 1.7320508075688772f;
        
        /// <summary>
        /// Default offset for hexagonal coordinates.
        /// There shouldn't be any need to change this during runtime,
        /// so it is set to a constant value.
        /// </summary>
        internal const OffsetType DefaultOffset = OffsetType.OddR;
        
        /// <summary>
        /// Default orientation for hexagonal coordinates.
        /// There shouldn't be any need to change this during runtime,
        /// so it is set to a constant value.
        /// </summary>
        internal const OrientationType DefaultOrientation = OrientationType.PointyTopped;
        
        /// <summary>
        /// The q-coordinate (horizontal axis) in cube coordinates.
        /// </summary>
        internal readonly int _q;
        
        /// <summary>
        /// The r-coordinate (diagonal axis) in cube coordinates.
        /// </summary>
        internal readonly int _r;
        
        /// <summary>
        /// The s-coordinate (negative sum of q and r) in cube coordinates.
        /// </summary>
        internal readonly int _s;
        
        /// <summary>
        /// The column coordinate (horizontal axis) in offset coordinates.
        /// </summary>
        public int Col { get; private set; }
        
        /// <summary>
        /// The row coordinate (vertical axis) in offset coordinates.
        /// </summary>
        public int Row { get; private set; }
        
        /// <summary>
        /// Constructs a new Hex from cube coordinates (q, r, s).
        /// </summary>
        /// <param name="q">The q-coordinate (horizontal axis).</param>
        /// <param name="r">The r-coordinate (diagonal axis).</param>
        /// <param name="s">The s-coordinate (negative sum of q and r).</param>
        /// <exception cref="ArgumentException">Thrown if q + r + s != 0.</exception>
        internal Hex(int q, int r, int s)
        {
            _q = q;
            _r = r;
            _s = s;
            
            var o = DefaultOffset is OffsetType.OddQ or OffsetType.OddR ? -1 : 1;
            if (DefaultOffset is OffsetType.OddR or OffsetType.EvenR)
            {
                Row = _r;
                Col = _q + (_r + o * (_r & 1)) / 2;
            }
            else
            {
                Col = _q;
                Row = _r + (_q + o * (_q & 1) / 2);
            }
            
            if (_q + _r + _s != 0)
                throw new ArgumentException("Invalid cube coordinates: q + r + s must equal 0.");
        }

        /// <summary>
        /// Constructs a new Hex from offset coordinates (col, row).
        /// </summary>
        /// <param name="col">The column coordinate (horizontal axis).</param>
        /// <param name="row">The row coordinate (vertical axis).</param>
        internal Hex(int col, int row)
        {
            Col = col;
            Row = row;
            
            var o = DefaultOffset is OffsetType.OddQ or OffsetType.OddR ? -1 : 1;
            if (DefaultOffset is OffsetType.OddR or OffsetType.EvenR)
            {
                _q = col - (row + o * (row & 1)) / 2;
                _r = row;
            }
            else
            {
                _q = col;
                _r = row - (col + o * (col & 1)) / 2;
            }

            _s = -_q - _r;
            
            if (_q + _r + _s != 0)
                throw new ArgumentException("Invalid cube coordinates: q + r + s must equal 0.");
        }

        /// <summary>
        /// The coordinate representing the direction to the top neighbor.
        /// </summary>
        public static readonly Hex Up = new Hex(0, -1, 1);
        
        /// <summary>
        /// The coordinate representing the direction to the top-right neighbor.
        /// </summary>
        public static readonly Hex UpRight = new Hex(1, -1, 0);
        
        /// <summary>
        /// The coordinate representing the direction to the right neighbor.
        /// </summary>
        public static readonly Hex Right = new Hex(1, 0, -1);
        
        /// <summary>
        /// The coordinate representing the direction to the down-right neighbor.
        /// </summary>
        public static readonly Hex DownRight = DefaultOrientation == OrientationType.PointyTopped
             ? new Hex(0, 1, -1)
             : new Hex(1, 0, -1);
        
        /// <summary>
        /// The coordinate representing the direction to the down neighbor.
        /// </summary>
        public static readonly Hex Down = new Hex(0, 1, -1);
        
        /// <summary>
        /// The coordinate representing the direction to the down-left neighbor.
        /// </summary>
        public static readonly Hex DownLeft = new Hex(-1, 1, 0);
        
        /// <summary>
        /// The coordinate representing the direction to the left neighbor.
        /// </summary>
        public static readonly Hex Left = new Hex(-1, 0, 1);
        
        /// <summary>
        /// The coordinate representing the direction to the up-left neighbor.
        /// </summary>
        public static readonly Hex UpLeft = DefaultOrientation == OrientationType.PointyTopped
            ? new Hex(0, -1, 1)
            : new Hex(-1, 0, 1);
        
        /// <summary>
        /// The array of all possible directions in a hexagonal grid arranged in clockwise order starting from the top.
        /// </summary>
        public static readonly Hex[] Directions =
        {
            (DefaultOrientation == OrientationType.PointyTopped) ? UpRight : Up,
            (DefaultOrientation == OrientationType.PointyTopped) ? Right : UpRight,
            DownRight,
            (DefaultOrientation == OrientationType.PointyTopped) ? DownLeft : Down,
            (DefaultOrientation == OrientationType.PointyTopped) ? Left : DownLeft,
            UpLeft,
        };
        
        /// <summary>
        /// Adds two VectorHexes together.
        /// </summary>
        /// <param name="a">The first Hex.</param>
        /// <param name="b">The second Hex.</param>
        /// <returns>A new Hex that is the sum of a and b.</returns>
        public static Hex operator+(Hex a, Hex b)
        {
            return new Hex(a._q + b._q, a._r + b._r, a._s + b._s);
        }
        
        /// <summary>
        /// Subtracts one Hex from another.
        /// </summary>
        /// <param name="a">The Hex to subtract from.</param>
        /// <param name="b">The Hex to subtract.</param>
        /// <returns>A new Hex that is the result of a - b.</returns>
        public static Hex operator-(Hex a, Hex b)
        {
            return new Hex(a._q - b._q, a._r - b._r, a._s - b._s);
        }
        
        /// <summary>
        /// Multiplies a Hex by a scalar value.
        /// </summary>
        /// <param name="a">The Hex to multiply.</param>
        /// <param name="scalar">The scalar value to multiply by.</param>
        /// <returns></returns>
        public static Hex operator*(Hex a, int scalar)
        {
            return new Hex(a._q * scalar, a._r * scalar, a._s * scalar);
        }
        
        /// <summary>
        /// Multiplies a scalar value by a Hex.
        /// </summary>
        /// <param name="scalar">The scalar value to multiply by.</param>
        /// <param name="a">The Hex to multiply.</param>
        /// <returns></returns>
        public static Hex operator*(int scalar, Hex a)
        {
            return a * scalar;
        }

        /// <summary>
        /// Implicitly converts a Hex to a Vector2Int.
        /// The Vector2Int represents the offset coordinates (Col, Row) of the Hex.
        /// </summary>
        /// <param name="hex">The Hex to convert.</param>
        /// <returns>A Vector2Int representing the offset coordinates.</returns>
        public static implicit operator Vector2Int(Hex hex)
        {
            return new Vector2Int(hex.Col, hex.Row);
        }

        /// <summary>
        /// Implicitly converts a Vector2Int to a Hex.
        /// The Hex represents the axial coordinates derived from the offset coordinates.
        /// Uses <see cref="DefaultOffset"/> offset type to determine the resulting cube coordinates.
        /// </summary>
        /// <param name="offset">The Vector2Int to convert.</param>
        /// <returns>A new Hex representing the axial coordinates derived from the offset.</returns>
        public static implicit operator Hex(Vector2Int offset)
        {
            return new Hex(offset.x, offset.y);
        }
        
        /// <summary>
        /// Rotates the Hex counter-clockwise (left).
        /// </summary>
        /// <returns>A new Hex that is rotated left.</returns>
        public Hex RotateLeft()
        {
            return new Hex(-_r, -_s, -_q);
        }
        
        /// <summary>
        /// Rotates the Hex clockwise (right).
        /// </summary>
        /// <returns>A new Hex that is rotated right.</returns>
        public Hex RotateRight()
        {
            return new Hex(-_s, -_q, -_r);
        }
        
        /// <summary>
        /// Retrieves the length of the Hex.
        /// </summary>
        /// <returns>The length of the Hex, which is the distance from the origin (0, 0, 0) to this coordinate.</returns>
        public int GetLength()
        {
            return (int)((Mathf.Abs(_q) + Mathf.Abs(_r) + Mathf.Abs(_s)) / 2);
        }
        
        /// <summary>
        /// Retrieves the distance to another Hex.
        /// </summary>
        /// <param name="other">The other Hex to calculate the distance to.</param>
        /// <returns>The distance between this Hex and the other Hex.</returns>
        public int GetDistance(Hex other)
        {
            return (this - other).GetLength();
        }
        
        public override string ToString()
        {
            return $"{{{Col}, {Row}}} ({_q}, {_r}, {_s})";
        }
    }

    /// <summary>
    /// Represents the offset type for hexagonal coordinates.
    /// This is used to convert between axial coordinates and offset coordinates.
    /// The offset type determines how the hexagonal grid is laid out, specifically whether it uses odd or even offsets.
    /// </summary>
    public enum OffsetType
    {
        /// <summary>
        /// Every odd column has an offset of 1.
        /// </summary>
        OddQ,
        
        /// <summary>
        /// Every even column has an offset of 1.
        /// </summary>
        EvenQ,
        
        /// <summary>
        /// Every odd row has an offset of 1.
        /// </summary>
        OddR,
        
        /// <summary>
        /// Every even row has an offset of 1.
        /// </summary>
        EvenR,
    }

    /// <summary>
    /// Represents the orientation of hexagonal coordinates.
    /// </summary>
    public enum OrientationType
    {
        /// <summary>
        /// Pointy-topped hexagons, where the flat sides are on the top and bottom.
        /// </summary>
        PointyTopped,
        
        /// <summary>
        /// Flat-topped hexagons, where the flat sides are on the left and right.
        /// </summary>
        FlatTopped
    }

    /// <summary>
    /// Represents a fractional hex coordinate. This simply means that the q, r, and s coordinates
    /// are stored as doubles instead of integers. This is used for calculations that require sub-hexagonal precision.
    /// </summary>
    internal readonly struct FractionalHex
    {
        private readonly double _q;
        private readonly double _r;
        
        internal FractionalHex(double q, double r)
        {
            _q = q;
            _r = r;
        }

        public Hex RoundToHex()
        {
            var s = -_q - _r;
            
            var qi = (int)Math.Round(_q);
            var ri = (int)Math.Round(_r);
            var si = (int)Math.Round(s);

            var qDiff = Math.Abs(qi - _q);
            var rDiff = Math.Abs(ri - _r);
            var sDiff = Math.Abs(si - s);
            if (qDiff >= rDiff && qDiff >= sDiff)
            {
                qi = -ri - si;
            }
            else if (rDiff >= sDiff)
            {
                ri = -qi - si;
            }
            else
            {
                si = -qi - ri;
            }
            
            return new Hex(qi, ri, si);
        }
    }
    
    /// <summary>
    /// Contains the values needed to convert between hexagonal coordinates and pixel coordinates.
    /// The orientation defines how the hexagons are laid out in the grid, either pointy-topped or flat-topped.
    /// EAch orientation has its own set of transformation coefficients for converting between hexagonal and pixel coordinates.
    /// </summary>
    internal struct HexOrientation
    {
        public readonly double F0;
        public readonly double F1;
        public readonly double F2;
        public readonly double F3;
        public readonly double B0;
        public readonly double B1;
        public readonly double B2;
        public readonly double B3;

        public HexOrientation(
            double f0, double f1, double f2, double f3, 
            double b0, double b1, double b2, double b3)
        {
            F0 = f0; F1 = f1; F2 = f2; F3 = f3;
            B0 = b0; B1 = b1; B2 = b2; B3 = b3;
        }
        
        public HexOrientation(
            float f0, float f1, float f2, float f3, 
            float b0, float b1, float b2, float b3)
        {
            F0 = f0; F1 = f1; F2 = f2; F3 = f3;
            B0 = b0; B1 = b1; B2 = b2; B3 = b3;
        }
    }

    /// <summary>
    /// Represents the layout of hexagonal coordinates in a grid.
    /// The layout can either be pointy-topped or flat-topped, and it defines how hexagonal coordinates
    /// are converted to world coordinates and vice versa.
    /// </summary>
    public class HexLayout
    {
        /// <summary>
        /// Pre-calculated constant for the square root of 3 so we avoid calculating it every time.
        /// </summary>
        private const double Sqrt3 = 1.7320508075688772;
        
        /// <summary>
        /// The orientation of the hexagonal grid.
        /// </summary>
        internal readonly HexOrientation Orientation;

        /// <summary>
        /// Constructs a new HexLayout with the specified origin and size.
        /// </summary>
        public HexLayout()
        {
            Orientation = Hex.DefaultOrientation == OrientationType.PointyTopped 
                ? PointyTopped 
                : FlatTopped;
        }

        /// <summary>
        /// Pointy-topped hexagonal orientation where the flat sides are on the left and right sides of the hexagon.
        /// </summary>
        internal static HexOrientation PointyTopped => new HexOrientation(
            Sqrt3, Sqrt3 / 2.0, 0.0, 3.0 / 2.0, 
            Sqrt3 / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0);
        
        /// <summary>
        /// Flat-topped hexagonal orientation where the flat sides are on the top and bottom of the hexagon.
        /// </summary>
        internal static HexOrientation FlatTopped => new HexOrientation(
            3.0 / 2.0, 0.0, Sqrt3 / 2.0, Sqrt3, 
            2.0 / 3.0, 0.0, -1.0 / 3.0, Sqrt3 / 3.0);

        /// <summary>
        /// Converts a hexagonal coordinate to world coordinates using the offset coordinate system.
        /// </summary>
        /// <param name="coord">The hexagonal coordinate to convert.</param>
        /// <param name="origin">The origin of the hexagonal grid in world coordinates.</param>
        /// <param name="size">The scalar size of each hexagon in the grid.</param>
        /// <returns>The world coordinates corresponding to the hexagonal coordinate.</returns>
        public Vector3 OffsetHexToWorld(Hex coord, Vector3 origin, float size = 1f)
        {
            return OffsetHexToWorld(coord, origin, new Vector2(size, size));
        }
        
        /// <summary>
        /// Converts a hexagonal coordinate to world coordinates using the offset coordinate system.
        /// </summary>
        /// <param name="coord">The hexagonal coordinate to convert.</param>
        /// <param name="origin">The origin of the hexagonal grid in world coordinates.</param>
        /// <param name="size">The size of each hexagon in the grid.</param>
        /// <returns>The world coordinates corresponding to the hexagonal coordinate.</returns>
        public Vector3 OffsetHexToWorld(Hex coord, Vector3 origin, Vector2 size)
        {
            var world = Vector3.zero;
            
            var o = Hex.DefaultOffset is OffsetType.OddQ or OffsetType.OddR ? 1 : -1;
            if (Hex.DefaultOffset is OffsetType.OddR or OffsetType.EvenR)
            {
                world.x = (float)Sqrt3 * (coord.Col + o * 0.5f * (coord.Row & 1)) * size.x;
                world.y = (float)(coord.Row * 1.5f) * size.y;
            }
            else
            {
                world.x = (float)(coord.Col * 1.5f) * size.x;
                world.y = (float)Sqrt3 * (coord.Row + o * 0.5f * (coord.Col & 1)) * size.y;
            }
            
            return new Vector3(world.x + origin.x, world.y + origin.y, 0f);
        }
        
        /// <summary>
        /// Converts a hexagonal coordinate to world coordinates.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate to convert.</param>
        /// <param name="origin">The origin of the hexagonal grid in world coordinates.</param>
        /// <param name="size">The scalar size of each hexagon in the grid.</param>
        /// <returns>The world coordinates corresponding to the hexagonal coordinate.</returns>
        public Vector3 AxialHexToWorld(Hex hex, Vector3 origin, float size = 1f)
        {
            return AxialHexToWorld(hex, origin, new Vector2(size, size));
        }
        
        /// <summary>
        /// Converts a hexagonal coordinate to world coordinates.
        /// </summary>
        /// <param name="hex">The hexagonal coordinate to convert.</param>
        /// <param name="origin">The origin of the hexagonal grid in world coordinates.</param>
        /// <param name="size">The size of each hexagon in the grid.</param>
        /// <returns>The world coordinates corresponding to the hexagonal coordinate.</returns>
        public Vector3 AxialHexToWorld(Hex hex, Vector3 origin, Vector2 size)
        {
            var x = (Orientation.F0 * hex._q + Orientation.F1 * hex._r) * size.x;
            var y = (Orientation.F2 * hex._q + Orientation.F3 * hex._r) * size.y;
            return new Vector3((float)(origin.x + x), (float)(origin.y + y), 0f);
        }
        
        /// <summary>
        /// Converts a world position to a hexagonal coordinate.
        /// </summary>
        /// <param name="world">The world position to convert.</param>
        /// <param name="origin">The origin of the hexagonal grid in world coordinates.</param>
        /// <param name="size">The scalar size of each hexagon in the grid.</param>
        /// <returns>A hexagonal coordinate that represents the world position within the hexagonal grid.</returns>
        public Hex WorldToAxialHex(Vector3 world, Vector3 origin, float size = 1f)
        {
            return WorldToAxialHex(world, origin, new Vector2(size, size));
        }

        /// <summary>
        /// Converts a world position to a hexagonal coordinate.
        /// </summary>
        /// <param name="world">The world position to convert.</param>
        /// <param name="origin">The origin of the hexagonal grid in world coordinates.</param>
        /// <param name="size">The size of each hexagon in the grid.</param>
        /// <returns>A hexagonal coordinate that represents the world position within the hexagonal grid.</returns>
        public Hex WorldToAxialHex(Vector3 world, Vector3 origin, Vector2 size)
        {
            return WorldToFractionalHex(world, origin, size).RoundToHex();
        }

        private FractionalHex WorldToFractionalHex(Vector3 world, Vector3 origin, Vector2 size)
        {
            var local = new Vector2((world.x - origin.x) / size.x, (world.y - origin.y) / size.y);
            var q = Orientation.B0 * local.x + Orientation.B1 * local.y;
            var r = Orientation.B2 * local.x + Orientation.B3 * local.y;
            return new FractionalHex(q, r);
        }
    }
}