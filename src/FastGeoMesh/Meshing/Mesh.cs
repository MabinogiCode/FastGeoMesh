using System.Collections.Generic;
using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing;

/// <summary>Raw mesh made of quads + triangles + auxiliary points and internal segments.</summary>
public sealed class Mesh
{
    /// <summary>Collection of quads.</summary>
    public ReadOnlyCollection<Quad> Quads => _quads.AsReadOnly();
    private readonly List<Quad> _quads = new();

    /// <summary>Collection of cap triangles (when enabled).</summary>
    public ReadOnlyCollection<Triangle> Triangles => _triangles.AsReadOnly();
    private readonly List<Triangle> _triangles = new();

    /// <summary>Auxiliary standalone points.</summary>
    public ReadOnlyCollection<Vec3> Points => _points.AsReadOnly();
    private readonly List<Vec3> _points = new();

    /// <summary>Internal 3D segments.</summary>
    public ReadOnlyCollection<Segment3D> InternalSegments => _internalSegments.AsReadOnly();
    private readonly List<Segment3D> _internalSegments = new();

    /// <summary>Add a quad.</summary>
    public void AddQuad(Quad quad) => _quads.Add(quad);
    /// <summary>Add a triangle.</summary>
    public void AddTriangle(Triangle tri) => _triangles.Add(tri);
    /// <summary>Add a point.</summary>
    public void AddPoint(Vec3 p) => _points.Add(p);
    /// <summary>Add an internal segment.</summary>
    public void AddInternalSegment(Segment3D s) => _internalSegments.Add(s);
}
