namespace FastGeoMesh.Domain {
    /// <summary>
    /// Quad defined by four corner vertices in CCW order.
    /// Optionally stores a quality metric for cap quads (null when not applicable).
    /// </summary>
    public readonly struct Quad : System.IEquatable<Quad> {

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> struct from four vertices (assumed CCW).
        /// </summary>
        /// <param name="v0">Vertex 0.</param>
        /// <param name="v1">Vertex 1.</param>
        /// <param name="v2">Vertex 2.</param>
        /// <param name="v3">Vertex 3.</param>
        public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3) {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
            this.V3 = v3;
            this.QualityScore = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> struct from four vertices with an optional quality score.
        /// </summary>
        /// <param name="v0">Vertex 0.</param>
        /// <param name="v1">Vertex 1.</param>
        /// <param name="v2">Vertex 2.</param>
        /// <param name="v3">Vertex 3.</param>
        /// <param name="qualityScore">Optional quality score.</param>
        public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3, double? qualityScore) {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
            this.V3 = v3;
            this.QualityScore = qualityScore;
        }

        /// <summary>Value equality comparison.</summary>
        public bool Equals(Quad other) {
            return this.V0.Equals(other.V0)
                && this.V1.Equals(other.V1)
                && this.V2.Equals(other.V2)
                && this.V3.Equals(other.V3)
                && Nullable.Equals(this.QualityScore, other.QualityScore);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) {
            return obj is Quad q && Equals(q);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return System.HashCode.Combine(this.V0, this.V1, this.V2, this.V3, this.QualityScore);
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(Quad left, Quad right) {
            return left.Equals(right);
        }

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Quad left, Quad right) {
            return !left.Equals(right);
        }
    }
}
