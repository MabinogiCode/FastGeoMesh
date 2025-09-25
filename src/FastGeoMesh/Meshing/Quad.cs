using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing;

/// <summary>A quad with 4 vertices in CCW order.</summary>
public sealed class Quad
{
    public Vec3 V0 { get; }
    public Vec3 V1 { get; }
    public Vec3 V2 { get; }
    public Vec3 V3 { get; }

    /// <summary>
    /// Optional quality score in [0,1] when available (e.g., caps quadification). Higher is better.
    /// Null if not evaluated (e.g., side faces).
    /// </summary>
    public double? QualityScore { get; init; }

    public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3) => (V0, V1, V2, V3) = (v0, v1, v2, v3);
}
