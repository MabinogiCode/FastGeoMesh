namespace FastGeoMesh.Geometry;

public readonly record struct Segment2D(Vec2 A, Vec2 B)
{
    public double Length() => (B - A).Length();
}
