namespace FastGeoMesh.Domain {
    /// <summary>Triangle primitive (CCW order, typically cap fallback output).</summary>
    public readonly struct Triangle : IEquatable<Triangle> {
        /// <summary>First vertex.</summary>
        public Vec3 V0 { get; init; }

        /// <summary>Second vertex.</summary>
        public Vec3 V1 { get; init; }

        /// <summary>Third vertex.</summary>
        public Vec3 V2 { get; init; }

        /// <summary>Optional quality score (reserved, currently unused for triangles).</summary>
        public double? QualityScore { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> struct with three vertices (assumed CCW).
        /// </summary>
        /// <param name="v0">First vertex.</param>
        /// <param name="v1">Second vertex.</param>
        /// <param name="v2">Third vertex.</param>
        public Triangle(Vec3 v0, Vec3 v1, Vec3 v2)
        {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
            this.QualityScore = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> struct with three vertices and an optional quality score.
        /// </summary>
        /// <param name="v0">First vertex.</param>
        /// <param name="v1">Second vertex.</param>
        /// <param name="v2">Third vertex.</param>
        /// <param name="qualityScore">Optional quality score.</param>
        public Triangle(Vec3 v0, Vec3 v1, Vec3 v2, double? qualityScore)
        {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
            this.QualityScore = qualityScore;
        }

        /// <summary>Value equality comparison.</summary>
        /// <param name="other">Other triangle to compare.</param>
        /// <returns>True if both triangles have equal vertices and quality score; otherwise false.</returns>
        public bool Equals(Triangle other)
        {
            return this.V0.Equals(other.V0)
                && this.V1.Equals(other.V1)
                && this.V2.Equals(other.V2)
                && Nullable.Equals(this.QualityScore, other.QualityScore);
        }

        /// <inheritdoc />
        /// <returns>True if the specified object is a <see cref="Triangle"/> and is equal to this instance; otherwise false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Triangle t && Equals(t);
        }

        /// <inheritdoc />
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return System.HashCode.Combine(this.V0, this.V1, this.V2, this.QualityScore);
        }

        /// <summary>Equality operator.</summary>
        /// <returns>True if the triangles are equal; otherwise false.</returns>
        public static bool operator ==(Triangle left, Triangle right)
        {
            return left.Equals(right);
        }

        /// <summary>Inequality operator.</summary>
        /// <returns>True if the triangles are not equal; otherwise false.</returns>
        public static bool operator !=(Triangle left, Triangle right)
        {
            return !left.Equals(right);
        }
    }
}
