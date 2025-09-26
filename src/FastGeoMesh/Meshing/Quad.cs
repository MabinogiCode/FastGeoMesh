using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing;

/// <summary>Axis-aligned quad (V0..V3) in CCW order.</summary>
public sealed class Quad
{
    /// <summary>First vertex.</summary>
    public Vec3 V0 { get; }
    /// <summary>Second vertex.</summary>
    public Vec3 V1 { get; }
    /// <summary>Third vertex.</summary>
    public Vec3 V2 { get; }
    /// <summary>Fourth vertex.</summary>
    public Vec3 V3 { get; }

    /// <summary>Optional quality score [0,1] (caps). Null if not evaluated.</summary>
    public double? QualityScore { get; init; }

    /// <summary>Create a quad from four vertices (assumed CCW).</summary>
    public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3) => (V0, V1, V2, V3) = (v0, v1, v2, v3);
}
