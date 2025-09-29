using System;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Geometry
{
    /// <summary>
    /// High-performance 2D vector optimized for .NET 8.
    /// Immutable value type with aggressive inlining for geometric operations in XY plane.
    /// </summary>
    public readonly record struct Vec2(double X, double Y)
    {
        /// <summary>Zero vector constant.</summary>
        public static readonly Vec2 Zero = new(0, 0);

        /// <summary>Unit vector along X axis.</summary>
        public static readonly Vec2 UnitX = new(1, 0);

        /// <summary>Unit vector along Y axis.</summary>
        public static readonly Vec2 UnitY = new(0, 1);

        /// <summary>Add two vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);

        /// <summary>Subtract b from a.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);

        /// <summary>Scale vector by k.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator *(Vec2 a, double k) => new(a.X * k, a.Y * k);

        /// <summary>Scale vector by k (commutative).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator *(double k, Vec2 a) => new(a.X * k, a.Y * k);

        /// <summary>Add two vectors using static method syntax.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Add(Vec2 a, Vec2 b) => a + b;

        /// <summary>Subtract two vectors using static method syntax.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Subtract(Vec2 a, Vec2 b) => a - b;

        /// <summary>Multiply vector by scalar using static method syntax.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Multiply(Vec2 a, double k) => a * k;

        /// <summary>Dot product with another vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(in Vec2 b) => X * b.X + Y * b.Y;

        /// <summary>Z component of 2D cross product (a × b).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Cross(in Vec2 b) => X * b.Y - Y * b.X;

        /// <summary>Euclidean length of the vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length() => Math.Sqrt(X * X + Y * Y);

        /// <summary>Squared length (faster than Length when only comparison is needed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared() => X * X + Y * Y;

        /// <summary>Normalize the vector to unit length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2 Normalize()
        {
            var length = Length();
            return length > 0 ? this * (1.0 / length) : Zero;
        }
    }
}
