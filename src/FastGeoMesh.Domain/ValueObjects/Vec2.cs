using System.Runtime.CompilerServices;

namespace FastGeoMesh.Domain {
    /// <summary>
    /// High-performance 2D vector optimized for .NET 8.
    /// Immutable value type with aggressive inlining for geometric operations in XY plane.
    /// Now implemented as readonly struct (instead of record struct) to avoid extra generated members and reduce IL size.
    /// Provides additional batch/SIMD helper methods for hot paths.
    /// </summary>
    public readonly struct Vec2 : IEquatable<Vec2> {
        /// <summary>Zero vector constant.</summary>
        public static readonly Vec2 Zero = new(0, 0);

        /// <summary>Unit vector along X axis.</summary>
        public static readonly Vec2 UnitX = new(1, 0);

        /// <summary>Unit vector along Y axis.</summary>
        public static readonly Vec2 UnitY = new(0, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vec2"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2(double x, double y) {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the X coordinate of the vector.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the Y coordinate of the vector.
        /// </summary>
        public double Y { get; }

        // Operators ---------------------------------------------------------

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Sum of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Difference of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="k">The scalar multiplier.</param>
        /// <returns>Scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator *(Vec2 a, double k)
        {
            return new Vec2(a.X * k, a.Y * k);
        }

        /// <summary>
        /// Multiplies a vector by a scalar (commutative version).
        /// </summary>
        /// <param name="k">The scalar multiplier.</param>
        /// <param name="a">The vector.</param>
        /// <returns>Scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 operator *(double k, Vec2 a)
        {
            return new Vec2(a.X * k, a.Y * k);
        }

        // Static helpers (API compatibility)

        /// <summary>
        /// Adds two vectors (static method version).
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Sum of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Add(Vec2 a, Vec2 b)
        {
            return a + b;
        }

        /// <summary>
        /// Subtracts the second vector from the first (static method version).
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Difference of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Subtract(Vec2 a, Vec2 b)
        {
            return a - b;
        }

        /// <summary>
        /// Multiplies a vector by a scalar (static method version).
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="k">The scalar multiplier.</param>
        /// <returns>Scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Multiply(Vec2 a, double k)
        {
            return a * k;
        }

        // Basic metrics -----------------------------------------------------

        /// <summary>
        /// Computes the dot product of this vector with another vector.
        /// </summary>
        /// <param name="b">The other vector.</param>
        /// <returns>Dot product result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(in Vec2 b)
        {
            return (this.X * b.X) + (this.Y * b.Y);
        }

        /// <summary>
        /// Computes the 2D cross product (determinant) of this vector with another vector.
        /// </summary>
        /// <param name="b">The other vector.</param>
        /// <returns>Cross product result (scalar in 2D).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Cross(in Vec2 b)
        {
            return (this.X * b.Y) - (this.Y * b.X);
        }

        /// <summary>
        /// Computes the length (magnitude) of the vector.
        /// </summary>
        /// <returns>Length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length()
        {
            return Math.Sqrt((this.X * this.X) + (this.Y * this.Y));
        }

        /// <summary>
        /// Computes the squared length of the vector (faster than Length() as it avoids square root).
        /// </summary>
        /// <returns>Squared length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared()
        {
            return (this.X * this.X) + (this.Y * this.Y);
        }

        /// <summary>
        /// Returns a normalized (unit length) version of this vector.
        /// </summary>
        /// <returns>Normalized vector, or zero vector if original has zero length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2 Normalize()
        {
            double lsq = (this.X * this.X) + (this.Y * this.Y);
            if (lsq <= 0d)
            {
                return Zero;
            }

            double inv = 1.0 / Math.Sqrt(lsq);
            return new Vec2(this.X * inv, this.Y * inv);
        }

        // SIMD / batch helpers ---------------------------------------------

        /// <summary>
        /// Accumulate sum of dot products of two spans (Σ a[i]·b[i]). Length is min of spans.
        /// Uses simple loop with unrolling; relies on JIT autovec / Vector where profitable.
        /// </summary>
        /// <returns>Sum of dot products for the overlapping length of the two spans.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double AccumulateDot(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b)
        {
            int len = Math.Min(a.Length, b.Length);
            double sum = 0d;
            int i = 0;

            // Process 4 at a time (manual unroll) - good balance for typical sizes
            for (; i <= len - 4; i += 4)
            {
                var a0 = a[i];
                var b0 = b[i];

                var a1 = a[i + 1];
                var b1 = b[i + 1];

                var a2 = a[i + 2];
                var b2 = b[i + 2];

                var a3 = a[i + 3];
                var b3 = b[i + 3];

                sum += (a0.X * b0.X) + (a0.Y * b0.Y);
                sum += (a1.X * b1.X) + (a1.Y * b1.Y);
                sum += (a2.X * b2.X) + (a2.Y * b2.Y);
                sum += (a3.X * b3.X) + (a3.Y * b3.Y);
            }

            for (; i < len; i++)
            {
                var av = a[i];
                var bv = b[i];
                sum += (av.X * bv.X) + (av.Y * bv.Y);
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

        /// <summary>
        /// Determines whether the specified <see cref="Vec2"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The other vector to compare.</param>
        /// <returns>True if the vectors are equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vec2 other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Vec2 v && Equals(v);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.X, this.Y);
        }

        /// <summary>
        /// Determines whether two vectors are equal.
        /// </summary>
        /// <param name="left">First vector.</param>
        /// <param name="right">Second vector.</param>
        /// <returns>True if the vectors are equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vec2 left, Vec2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two vectors are not equal.
        /// </summary>
        /// <param name="left">First vector.</param>
        /// <param name="right">Second vector.</param>
        /// <returns>True if the vectors are not equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vec2 left, Vec2 right)
        {
            return !left.Equals(right);
        }

        /// <summary>Deconstruct for syntactic convenience (kept for compatibility with previous record struct).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out double x, out double y)
        {
            x = this.X;
            y = this.Y;
        }

        /// <summary>
        /// Returns a string representation of this vector.
        /// </summary>
        /// <returns>String representation in the format "(X, Y)".</returns>
        public override string ToString()
        {
            return $"({this.X}, {this.Y})"; // Simple, cheaper than record-generated
        }
    }
}
