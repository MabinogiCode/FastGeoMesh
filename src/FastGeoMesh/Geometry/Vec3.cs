using System;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Geometry
{
    /// <summary>
    /// High-performance 3D vector using .NET 8 optimizations.
    /// Immutable value type with aggressive inlining for geometric operations.
    /// </summary>
    public readonly record struct Vec3(double X, double Y, double Z)
    {
        /// <summary>Zero vector constant.</summary>
        public static readonly Vec3 Zero = new(0, 0, 0);

        /// <summary>Unit vector along X axis.</summary>
        public static readonly Vec3 UnitX = new(1, 0, 0);

        /// <summary>Unit vector along Y axis.</summary>
        public static readonly Vec3 UnitY = new(0, 1, 0);

        /// <summary>Unit vector along Z axis.</summary>
        public static readonly Vec3 UnitZ = new(0, 0, 1);

        /// <summary>Add two vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        /// <summary>Subtract b from a.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        /// <summary>Scale vector by k.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(Vec3 a, double k) => new(a.X * k, a.Y * k, a.Z * k);

        /// <summary>Scale vector by k (commutative).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(double k, Vec3 a) => new(a.X * k, a.Y * k, a.Z * k);

        /// <summary>Add two vectors using static method syntax.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Add(Vec3 a, Vec3 b) => a + b;

        /// <summary>Subtract two vectors using static method syntax.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Subtract(Vec3 a, Vec3 b) => a - b;

        /// <summary>Multiply vector by scalar using static method syntax.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Multiply(Vec3 a, double k) => a * k;

        /// <summary>Dot product with another vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(in Vec3 b) => X * b.X + Y * b.Y + Z * b.Z;

        /// <summary>Cross product with another vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3 Cross(in Vec3 b) => new(
            Y * b.Z - Z * b.Y,
            Z * b.X - X * b.Z,
            X * b.Y - Y * b.X);

        /// <summary>Euclidean length of the vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>Squared length (faster than Length when only comparison is needed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared() => X * X + Y * Y + Z * Z;

        /// <summary>Normalize the vector to unit length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3 Normalize()
        {
            var length = Length();
            return length > 0 ? this * (1.0 / length) : Zero;
        }
    }
}
