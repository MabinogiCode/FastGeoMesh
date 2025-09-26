namespace FastGeoMesh.Geometry;

/// <summary>2D line segment.</summary>
public readonly record struct Segment2D(Vec2 A, Vec2 B)
{
    /// <summary>Segment length.</summary>
    public double Length() => (B - A).Length();
}
