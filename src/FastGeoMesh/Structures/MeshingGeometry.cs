using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Structures;

/// <summary>
/// Geometry to be enforced/integrated by the mesher: imposed points and 3D segments.
/// Pure geometry, independent from domain semantics.
/// </summary>
public sealed class MeshingGeometry
{
    private readonly List<Vec3> _points = new();
    private readonly List<Segment3D> _segments = new();

    public ReadOnlyCollection<Vec3> Points => _points.AsReadOnly();
    public ReadOnlyCollection<Segment3D> Segments => _segments.AsReadOnly();

    public MeshingGeometry AddPoint(Vec3 p)
    {
        _points.Add(p);
        return this;
    }

    public MeshingGeometry AddSegment(Segment3D s)
    {
        _segments.Add(s);
        return this;
    }
}
