namespace FastGeoMesh.Domain
{
    /// <summary>Triangle primitive (CCW order, typically cap fallback output).</summary>
    public readonly struct Triangle : System.IEquatable<Triangle>
    {
        /// <summary>First vertex.</summary>
        public Vec3 V0 { get; init; }
        /// <summary>Second vertex.</summary>
        public Vec3 V1 { get; init; }
        /// <summary>Third vertex.</summary>
        public Vec3 V2 { get; init; }
        /// <summary>Optional quality score (reserved, currently unused for triangles).</summary>
        public double? QualityScore { get; init; }

        /// <summary>Create a triangle from three CCW vertices.</summary>
        public Triangle(Vec3 v0, Vec3 v1, Vec3 v2) : this()
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            QualityScore = null;
        }

        /// <summary>Create a triangle from three vertices with quality score.</summary>
        public Triangle(Vec3 v0, Vec3 v1, Vec3 v2, double? qualityScore) : this()
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            QualityScore = qualityScore;
        }

        /// <summary>Value equality comparison.</summary>
        public bool Equals(Triangle other) => V0.Equals(other.V0) && V1.Equals(other.V1) && V2.Equals(other.V2) && QualityScore.Equals(other.QualityScore);
        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Triangle t && Equals(t);
        /// <inheritdoc />
        public override int GetHashCode() => System.HashCode.Combine(V0, V1, V2, QualityScore);
        /// <summary>Equality operator.</summary>
        public static bool operator ==(Triangle left, Triangle right) => left.Equals(right);
        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Triangle left, Triangle right) => !left.Equals(right);
    }
}
