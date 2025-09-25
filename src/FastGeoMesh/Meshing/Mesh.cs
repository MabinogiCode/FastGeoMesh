using System.Collections.Generic;
using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing;

public sealed class Mesh
{
    public ReadOnlyCollection<Quad> Quads => _quads.AsReadOnly();
    private readonly List<Quad> _quads = new();

    public ReadOnlyCollection<Vec3> Points => _points.AsReadOnly();
    private readonly List<Vec3> _points = new();

    public ReadOnlyCollection<Segment3D> InternalSegments => _internalSegments.AsReadOnly();
    private readonly List<Segment3D> _internalSegments = new();

    public void AddQuad(Quad quad) => _quads.Add(quad);
    public void AddPoint(Vec3 p) => _points.Add(p);
    public void AddInternalSegment(Segment3D s) => _internalSegments.Add(s);
}
