using FastGeoMesh.Geometry;

namespace FastGeoMesh.Elements;

/// <summary>2D segment tagged with a Z elevation level.</summary>
public sealed class SegmentAtZ : IElement
{
    /// <summary>Discriminator value.</summary>
    public string Kind => nameof(SegmentAtZ);
    /// <summary>2D segment in footprint plane.</summary>
    public Segment2D Segment { get; }
    /// <summary>Z elevation associated to the segment.</summary>
    public double Z { get; }
    /// <summary>Create a segment-at-Z container.</summary>
    public SegmentAtZ(Segment2D segment, double z) => (Segment, Z) = (segment, z);
}
