using FastGeoMesh.Geometry;

namespace FastGeoMesh.Elements;

/// <summary>A simple segment at a specific Z elevation (e.g., 'lierne').</summary>
public sealed class SegmentAtZ : IElement
{
    public string Kind => nameof(SegmentAtZ);
    public Segment2D Segment { get; }
    public double Z { get; }
    public SegmentAtZ(Segment2D segment, double z) => (Segment, Z) = (segment, z);
}
