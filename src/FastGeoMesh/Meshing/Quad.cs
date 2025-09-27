using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing
{
    /// <summary>Quad defined by four corner vertices in CCW order. Optionally stores a quality metric for cap quads (null when not applicable).</summary>
    public sealed class Quad
    {
        /// <summary>Corner vertex 0.</summary>
        public Vec3 V0 { get; }
        /// <summary>Corner vertex 1.</summary>
        public Vec3 V1 { get; }
        /// <summary>Corner vertex 2.</summary>
        public Vec3 V2 { get; }
        /// <summary>Corner vertex 3.</summary>
        public Vec3 V3 { get; }
        /// <summary>Optional quality score in [0,1] for cap quads; null for side quads or when not computed.</summary>
        public double? QualityScore { get; init; }
        /// <summary>Create a quad from four vertices (assumed CCW).</summary>
        public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3) => (V0, V1, V2, V3) = (v0, v1, v2, v3);
    }
}
