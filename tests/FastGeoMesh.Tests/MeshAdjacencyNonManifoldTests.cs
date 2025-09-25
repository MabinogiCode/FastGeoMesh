using FastGeoMesh.Meshing;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class MeshAdjacencyNonManifoldTests
{
    [Fact]
    public void DetectsNonManifoldEdges()
    {
        // Create 3 quads sharing the same edge (0-1) -> non-manifold
        // Vertices
        var builder = new IndexedMeshBuilder()
            .AddVertex(0,0,0)  //0
            .AddVertex(1,0,0)  //1
            .AddVertex(0,1,0)  //2
            .AddVertex(1,1,0)  //3
            .AddVertex(0,-1,0) //4
            .AddVertex(1,-1,0) //5
            .AddVertex(-1,0,0) //6
            .AddVertex(2,0,0); //7

        // Quads all include edge (0,1)
        builder.AddQuad(0,1,3,2); // top
        builder.AddQuad(4,5,1,0); // bottom
        builder.AddQuad(6,0,1,7); // side sharing same (0,1)

        var im = builder.Build();
        var adj = MeshAdjacency.Build(im);

        adj.NonManifoldEdges.Should().ContainSingle();
        var e = adj.NonManifoldEdges[0];
        (Math.Min(e.a,e.b), Math.Max(e.a,e.b)).Should().Be((0,1));
    }
}
