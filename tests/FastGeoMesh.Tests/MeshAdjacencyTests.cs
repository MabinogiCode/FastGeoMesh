using FastGeoMesh.Meshing;
using FastGeoMesh.Geometry;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class MeshAdjacencyTests
{
    [Fact]
    public void AdjacencyDetectsNeighborsAndBoundaryEdges()
    {
        // Build a 1x2 grid of quads sharing a vertical edge
        // Vertices: 0--1--2 on bottom, 3--4--5 on top
        var im = new IndexedMeshBuilder()
            .AddVertex(0, 0, 0) //0
            .AddVertex(1, 0, 0) //1
            .AddVertex(2, 0, 0) //2
            .AddVertex(0, 1, 0) //3
            .AddVertex(1, 1, 0) //4
            .AddVertex(2, 1, 0) //5
            .AddQuad(0,1,4,3)   // q0
            .AddQuad(1,2,5,4)   // q1
            .Build();

        var adj = MeshAdjacency.Build(im);

        adj.Neighbors[0][1].Should().Be(1);
        adj.Neighbors[1][3].Should().Be(0);

        adj.BoundaryEdges.Should().HaveCount(6);
    }
}

internal sealed class IndexedMeshBuilder
{
    private readonly List<Vec3> _verts = new();
    private readonly List<(int,int,int,int)> _quads = new();

    public IndexedMeshBuilder AddVertex(double x,double y,double z)
    { _verts.Add(new Vec3(x,y,z)); return this; }
    public IndexedMeshBuilder AddQuad(int v0,int v1,int v2,int v3)
    { _quads.Add((v0,v1,v2,v3)); return this; }

    public IndexedMesh Build()
    {
        // Build via Mesh -> IndexedMesh pipeline (maintains edges correctly)
        var mesh = new Mesh();
        // We add quads directly using the provided vertex positions order.
        var verts = _verts.ToArray();
        foreach (var q in _quads)
        {
            var quad = new Quad(verts[q.Item1], verts[q.Item2], verts[q.Item3], verts[q.Item4]);
            mesh.AddQuad(quad);
        }
        return IndexedMesh.FromMesh(mesh); // default epsilon
    }
}
