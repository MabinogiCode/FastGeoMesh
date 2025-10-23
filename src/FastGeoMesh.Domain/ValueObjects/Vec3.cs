using System.Runtime.CompilerServices;

namespace FastGeoMesh.Domain {
    /// <summary>
    /// High-performance 3D vector using .NET 8 optimizations.
    /// Converted to readonly struct (from record struct) to cut generated members and reduce IL.
    /// Adds batch helpers for dot/cross operations in hot paths.
    /// </summary>
    public readonly struct Vec3 : IEquatable<Vec3> {
        // Static members first (SA1204)

        /// <summary>Zero vector constant.</summary>
        public static readonly Vec3 Zero = new(0, 0, 0);

        /// <summary>Unit vector along X axis.</summary>
        public static readonly Vec3 UnitX = new(1, 0, 0);

        /// <summary>Unit vector along Y axis.</summary>
        public static readonly Vec3 UnitY = new(0, 1, 0);

        /// <summary>Unit vector along Z axis.</summary>
        public static readonly Vec3 UnitZ = new(0, 0, 1);

        // Operators (static)
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Sum of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator +(Vec3 a, Vec3 b) {
            return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Difference of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator -(Vec3 a, Vec3 b) {
            return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="k">The scalar multiplier.</param>
        /// <returns>Scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(Vec3 a, double k) {
            return new Vec3(a.X * k, a.Y * k, a.Z * k);
        }

        /// <summary>
        /// Multiplies a vector by a scalar (commutative version).
        /// </summary>
        /// <param name="k">The scalar multiplier.</param>
        /// <param name="a">The vector.</param>
        /// <returns>Scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(double k, Vec3 a) {
            return new Vec3(a.X * k, a.Y * k, a.Z * k);
        }

        // API compatibility static helpers
        /// <summary>
        /// Adds two vectors (static method version).
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Sum of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Add(Vec3 a, Vec3 b) {
            return a + b;
        }

        /// <summary>
        /// Subtracts the second vector from the first (static method version).
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Difference of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Subtract(Vec3 a, Vec3 b) {
            return a - b;
        }

        /// <summary>
        /// Multiplies a vector by a scalar (static method version).
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="k">The scalar multiplier.</param>
        /// <returns>Scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Multiply(Vec3 a, double k) {
            return a * k;
        }

        // Constructor next (SA1201 expects constructors before properties)
        /// <summary>
        /// Initializes a new instance of the <see cref="Vec3"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }

        // Instance properties
        /// <summary>Gets the X coordinate of the vector.</summary>
        public double X { get; }

        /// <summary>Gets the Y coordinate of the vector.</summary>
        public double Y { get; }

        /// <summary>Gets the Z coordinate of the vector.</summary>
        public double Z { get; }

        // Instance methods -------------------------------------------------
        /// <summary>
        /// Computes the dot product of this vector with another vector.
        /// </summary>
        /// <param name="b">The other vector.</param>
        /// <returns>Dot product result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(in Vec3 b) {
            return (this.X * b.X) + (this.Y * b.Y) + (this.Z * b.Z);
        }

        /// <summary>
        /// Computes the cross product of this vector with another vector.
        /// </summary>
        /// <param name="b">The other vector.</param>
        /// <returns>Cross product result vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3 Cross(in Vec3 b) {
            double cx = (this.Y * b.Z) - (this.Z * b.Y);
            double cy = (this.Z * b.X) - (this.X * b.Z);
            double cz = (this.X * b.Y) - (this.Y * b.X);
            return new Vec3(cx, cy, cz);
        }

        /// <summary>
        /// Computes the length (magnitude) of the vector.
        /// </summary>
        /// <returns>Length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length() {
            return Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));
        }

        /// <summary>
        /// Computes the squared length of the vector (faster than Length() as it avoids square root).
        /// </summary>
        /// <returns>Squared length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared() {
            return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
        }

        /// <summary>
        /// Returns a normalized (unit length) version of this vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3 Normalize() {
            double lsq = (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
            if (lsq <= 0d) {
                return Zero;
            }

            double inv = 1.0 / Math.Sqrt(lsq);
            return new Vec3(this.X * inv, this.Y * inv, this.Z * inv);
        }

        // Batch helpers -----------------------------------------------------
        /// <summary>Accumulate sum of dot products for spans (Σ a[i]·b[i]).</summary>
        /// <returns>Sum of dot products for the overlapping length of the two spans.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double AccumulateDot(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b) {
            int len = Math.Min(a.Length, b.Length);
            double sum = 0d;
            int i = 0;
            for (; i <= len - 4; i += 4) {
                var a0 = a[i];
                var b0 = b[i];
                var a1 = a[i + 1];
                var b1 = b[i + 1];
                var a2 = a[i + 2];
                var b2 = b[i + 2];
                var a3 = a[i + 3];
                var b3 = b[i + 3];

                sum += (a0.X * b0.X) + (a0.Y * b0.Y) + (a0.Z * b0.Z);
                sum += (a1.X * b1.X) + (a1.Y * b1.Y) + (a1.Z * b1.Z);
                sum += (a2.X * b2.X) + (a2.Y * b2.Y) + (a2.Z * b2.Z);
                sum += (a3.X * b3.X) + (a3.Y * b3.Y) + (a3.Z * b3.Z);
            }

            for (; i < len; i++) {
                var av = a[i];
                var bv = b[i];
                sum += (av.X * bv.X) + (av.Y * bv.Y) + (av.Z * bv.Z);
            }

            return sum;
        }

        /// <summary>Element-wise addition of two spans into destination span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Add(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest) {
            int len = Math.Min(Math.Min(a.Length, b.Length), dest.Length);
            int i = 0;
            for (; i <= len - 4; i += 4) {
                dest[i] = a[i] + b[i];
                dest[i + 1] = a[i + 1] + b[i + 1];
                dest[i + 2] = a[i + 2] + b[i + 2];
                dest[i + 3] = a[i + 3] + b[i + 3];
            }

            for (; i < len; i++) {
                dest[i] = a[i] + b[i];
            }
        }

        /// <summary>Compute cross product for element pairs of spans into destination.</summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Cross(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest) {
            int len = Math.Min(Math.Min(a.Length, b.Length), dest.Length);
            for (int i = 0; i < len; i++) {
                var av = a[i];
                var bv = b[i];
                dest[i] = new Vec3(
                    (av.Y * bv.Z) - (av.Z * bv.Y),
                    (av.Z * bv.X) - (av.X * bv.Z),
                    (av.X * bv.Y) - (av.Y * bv.X));
            }
        }

        // Equality / hashing ------------------------------------------------
        /// <summary>
        /// Determines whether the specified <see cref="Vec3"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The other vector to compare.</param>
        /// <returns>True if the vectors are equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vec3 other) {
            return (this.X == other.X) && (this.Y == other.Y) && (this.Z == other.Z);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj) {
            return obj is Vec3 v && Equals(v);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode() {
            return HashCode.Combine(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Determines whether two vectors are equal.
        /// </summary>
        /// <param name="l">First vector.</param>
        /// <param name="r">Second vector.</param>
        /// <returns>True if the vectors are equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vec3 l, Vec3 r) {
            return l.Equals(r);
        }

        /// <summary>
        /// Determines whether two vectors are not equal.
        /// </summary>
        /// <param name="l">First vector.</param>
        /// <param name="r">Second vector.</param>
        /// <returns>True if the vectors are not equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vec3 l, Vec3 r) {
            return !l.Equals(r);
        }

        /// <summary>Deconstruct for compatibility with previous record struct.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out double x, out double y, out double z) {
            x = this.X;
            y = this.Y;
            z = this.Z;
        }

        /// <summary>
        /// Returns a string representation of this vector.
        /// </summary>
        /// <returns>String representation in the format "(X, Y, Z)".</returns>
        public override string ToString() {
            return $"({this.X}, {this.Y}, {this.Z})";
        }
    }
}
