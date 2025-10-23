namespace FastGeoMesh.Domain {
    /// <summary>3D line segment defined by start and end points.</summary>
    public readonly record struct Segment3D(Vec3 Start, Vec3 End);
}
