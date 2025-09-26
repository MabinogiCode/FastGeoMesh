using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Structures;

/// <summary>
/// Collection of auxiliary geometry (points / 3D segments) to be preserved in the output mesh.
/// Used to derive refinement levels or exported as-is.
/// </summary>
public sealed class MeshingGeometry
{
    private readonly List<Vec3> _points = new();
    private readonly List<Segment3D> _segments = new();

    /// <summary>Read-only list of registered points.</summary>
    public ReadOnlyCollection<Vec3> Points => _points.AsReadOnly();
    /// <summary>Read-only list of registered 3D segments.</summary>
    public ReadOnlyCollection<Segment3D> Segments => _segments.AsReadOnly();

    /// <summary>Add a point to the geometry set.</summary>
    public MeshingGeometry AddPoint(Vec3 p) { _points.Add(p); return this; }
    /// <summary>Add a 3D segment to the geometry set.</summary>
    public MeshingGeometry AddSegment(Segment3D s) { _segments.Add(s); return this; }
}
