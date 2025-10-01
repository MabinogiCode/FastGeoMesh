using System;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Geometry
{
    /// <summary>
    /// High-performance 3D vector using .NET 8 optimizations.
    /// Converted to readonly struct (from record struct) to cut generated members and reduce IL.
    /// Adds batch helpers for dot/cross operations in hot paths.
    /// </summary>
    public readonly struct Vec3 : IEquatable<Vec3>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        /// <summary>Zero vector constant.</summary>
        public static readonly Vec3 Zero = new(0, 0, 0);
        /// <summary>Unit vector along X axis.</summary>
        public static readonly Vec3 UnitX = new(1, 0, 0);
        /// <summary>Unit vector along Y axis.</summary>
        public static readonly Vec3 UnitY = new(0, 1, 0);
        /// <summary>Unit vector along Z axis.</summary>
        public static readonly Vec3 UnitZ = new(0, 0, 1);

        // Operators ---------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 operator *(Vec3 a, double k) => new(a.X * k, a.Y * k, a.Z * k);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 operator *(double k, Vec3 a) => new(a.X * k, a.Y * k, a.Z * k);

        // API compatibility static helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 Add(Vec3 a, Vec3 b) => a + b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 Subtract(Vec3 a, Vec3 b) => a - b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec3 Multiply(Vec3 a, double k) => a * k;

        // Basic metrics -----------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double Dot(in Vec3 b) => X * b.X + Y * b.Y + Z * b.Z;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Vec3 Cross(in Vec3 b) => new(
            Y * b.Z - Z * b.Y,
            Z * b.X - X * b.Z,
            X * b.Y - Y * b.X);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double LengthSquared() => X * X + Y * Y + Z * Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3 Normalize()
        {
            double lsq = X * X + Y * Y + Z * Z;
            if (lsq <= 0d)
            {
                return Zero;
            }
            double inv = 1.0 / Math.Sqrt(lsq);
            return new Vec3(X * inv, Y * inv, Z * inv);
        }

        // Batch helpers -----------------------------------------------------
        /// <summary>Accumulate sum of dot products for spans (Σ a[i]·b[i]).</summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double AccumulateDot(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b)
        {
            int len = Math.Min(a.Length, b.Length);
            double sum = 0d;
            int i = 0;
            for (; i <= len - 4; i += 4)
            {
                var a0 = a[i]; var b0 = b[i];
                var a1 = a[i + 1]; var b1 = b[i + 1];
                var a2 = a[i + 2]; var b2 = b[i + 2];
                var a3 = a[i + 3]; var b3 = b[i + 3];
                sum += a0.X * b0.X + a0.Y * b0.Y + a0.Z * b0.Z
                     + a1.X * b1.X + a1.Y * b1.Y + a1.Z * b1.Z
                     + a2.X * b2.X + a2.Y * b2.Y + a2.Z * b2.Z
                     + a3.X * b3.X + a3.Y * b3.Y + a3.Z * b3.Z;
            }
            for (; i < len; i++)
            {
                var av = a[i]; var bv = b[i];
                sum += av.X * bv.X + av.Y * bv.Y + av.Z * bv.Z;
            }
            return sum;
        }

        /// <summary>Element-wise addition of two spans into destination span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Add(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest)
        {
            int len = Math.Min(Math.Min(a.Length, b.Length), dest.Length);
            int i = 0;
            for (; i <= len - 4; i += 4)
            {
                dest[i] = a[i] + b[i];
                dest[i + 1] = a[i + 1] + b[i + 1];
                dest[i + 2] = a[i + 2] + b[i + 2];
                dest[i + 3] = a[i + 3] + b[i + 3];
            }
            for (; i < len; i++)
            {
                dest[i] = a[i] + b[i];
            }
        }

        /// <summary>Compute cross product for element pairs of spans into destination.</summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Cross(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest)
        {
            int len = Math.Min(Math.Min(a.Length, b.Length), dest.Length);
            for (int i = 0; i < len; i++)
            {
                var av = a[i]; var bv = b[i];
                dest[i] = new Vec3(
                    av.Y * bv.Z - av.Z * bv.Y,
                    av.Z * bv.X - av.X * bv.Z,
                    av.X * bv.Y - av.Y * bv.X);
            }
        }

        // Equality / hashing ------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(Vec3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object? obj) => obj is Vec3 v && Equals(v);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(Vec3 l, Vec3 r) => l.Equals(r);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(Vec3 l, Vec3 r) => !l.Equals(r);

        /// <summary>Deconstruct for compatibility with previous record struct.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Deconstruct(out double x, out double y, out double z) { x = X; y = Y; z = Z; }
        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
