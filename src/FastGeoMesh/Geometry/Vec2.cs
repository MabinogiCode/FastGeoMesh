using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FastGeoMesh.Geometry
{
    /// <summary>
    /// High-performance 2D vector optimized for .NET 8.
    /// Immutable value type with aggressive inlining for geometric operations in XY plane.
    /// Now implemented as readonly struct (instead of record struct) to avoid extra generated members and reduce IL size.
    /// Provides additional batch/SIMD helper methods for hot paths.
    /// </summary>
    public readonly struct Vec2 : IEquatable<Vec2>
    {
        public double X { get; }
        public double Y { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Zero vector constant.</summary>
        public static readonly Vec2 Zero = new(0, 0);
        /// <summary>Unit vector along X axis.</summary>
        public static readonly Vec2 UnitX = new(1, 0);
        /// <summary>Unit vector along Y axis.</summary>
        public static readonly Vec2 UnitY = new(0, 1);

        // Operators ---------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator *(Vec2 a, double k) => new(a.X * k, a.Y * k);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator *(double k, Vec2 a) => new(a.X * k, a.Y * k);

        // Static helpers (API compatibility)
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec2 Add(Vec2 a, Vec2 b) => a + b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec2 Subtract(Vec2 a, Vec2 b) => a - b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vec2 Multiply(Vec2 a, double k) => a * k;

        // Basic metrics -----------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double Dot(in Vec2 b) => X * b.X + Y * b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double Cross(in Vec2 b) => X * b.Y - Y * b.X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double Length() => Math.Sqrt(X * X + Y * Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double LengthSquared() => X * X + Y * Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2 Normalize()
        {
            // Compute 1/len directly to allow potential HW rsqrt in future JITs
            double lsq = X * X + Y * Y;
            if (lsq <= 0d)
            {
                return Zero;
            }
            double inv = 1.0 / Math.Sqrt(lsq);
            return new Vec2(X * inv, Y * inv);
        }

        // SIMD / batch helpers ---------------------------------------------
        /// <summary>
        /// Accumulate sum of dot products of two spans (Σ a[i]·b[i]). Length is min of spans.
        /// Uses simple loop with unrolling; relies on JIT autovec / Vector where profitable.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double AccumulateDot(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b)
        {
            int len = Math.Min(a.Length, b.Length);
            double sum = 0d;
            int i = 0;

            // Process 4 at a time (manual unroll) - good balance for typical sizes
            for (; i <= len - 4; i += 4)
            {
                var a0 = a[i]; var b0 = b[i];
                var a1 = a[i + 1]; var b1 = b[i + 1];
                var a2 = a[i + 2]; var b2 = b[i + 2];
                var a3 = a[i + 3]; var b3 = b[i + 3];
                sum += a0.X * b0.X + a0.Y * b0.Y
                     + a1.X * b1.X + a1.Y * b1.Y
                     + a2.X * b2.X + a2.Y * b2.Y
                     + a3.X * b3.X + a3.Y * b3.Y;
            }
            for (; i < len; i++)
            {
                var av = a[i]; var bv = b[i];
                sum += av.X * bv.X + av.Y * bv.Y;
            }
            return sum;
        }

        /// <summary>
        /// Element-wise addition of two spans into destination span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Add(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b, Span<Vec2> dest)
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

        // Equality / hashing -----------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(Vec2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Vec2 v && Equals(v);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(Vec2 left, Vec2 right) => left.Equals(right);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(Vec2 left, Vec2 right) => !left.Equals(right);

        /// <summary>Deconstruct for syntactic convenience (kept for compatibility with previous record struct).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Deconstruct(out double x, out double y) { x = X; y = Y; }
        public override string ToString() => $"({X}, {Y})"; // Simple, cheaper than record-generated
    }
}
