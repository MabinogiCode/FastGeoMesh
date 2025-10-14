namespace FastGeoMesh.Domain
{
    /// <summary>Quad defined by four corner vertices in CCW order. Optionally stores a quality metric for cap quads (null when not applicable).</summary>
    public readonly struct Quad : System.IEquatable<Quad>
    {
        /// <summary>Corner vertex 0.</summary>
        public Vec3 V0 { get; init; }
        /// <summary>Corner vertex 1.</summary>
        public Vec3 V1 { get; init; }
        /// <summary>Corner vertex 2.</summary>
        public Vec3 V2 { get; init; }
        /// <summary>Corner vertex 3.</summary>
        public Vec3 V3 { get; init; }
        /// <summary>Optional quality score in [0,1] for cap quads; null for side quads or when not computed.</summary>
        public double? QualityScore { get; init; }

        /// <summary>Create a quad from four vertices (assumed CCW).</summary>
        public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3) : this()
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
            QualityScore = null;
        }

        /// <summary>Create a quad from four vertices with quality score.</summary>
        public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3, double? qualityScore) : this()
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
            QualityScore = qualityScore;
        }

        /// <summary>Value equality comparison.</summary>
        public bool Equals(Quad other) => V0.Equals(other.V0) && V1.Equals(other.V1) && V2.Equals(other.V2) && V3.Equals(other.V3) && QualityScore.Equals(other.QualityScore);
        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Quad q && Equals(q);
        /// <inheritdoc />
        public override int GetHashCode() => System.HashCode.Combine(V0, V1, V2, V3, QualityScore);
        /// <summary>Equality operator.</summary>
        public static bool operator ==(Quad left, Quad right) => left.Equals(right);
        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Quad left, Quad right) => !left.Equals(right);
    }
}
