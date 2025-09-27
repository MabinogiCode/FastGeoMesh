using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing
{
    /// <summary>Triangle primitive (CCW order, typically cap fallback output).</summary>
    public sealed class Triangle
    {
        /// <summary>First vertex.</summary>
        public Vec3 V0 { get; }
        /// <summary>Second vertex.</summary>
        public Vec3 V1 { get; }
        /// <summary>Third vertex.</summary>
        public Vec3 V2 { get; }
        /// <summary>Optional quality score (reserved, currently unused for triangles).</summary>
        public double? QualityScore { get; init; }

        /// <summary>Create a triangle from three CCW vertices.</summary>
        public Triangle(Vec3 v0, Vec3 v1, Vec3 v2) => (V0, V1, V2) = (v0, v1, v2);
    }
}
