using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class MeshAdjacencyTests
    {
        [Fact]
        public void AdjacencyDetectsNeighborsAndBoundaryEdges()
        {
            var im = new IndexedMeshBuilder()
                .AddVertex(0, 0, 0)
                .AddVertex(1, 0, 0)
                .AddVertex(2, 0, 0)
                .AddVertex(0, 1, 0)
                .AddVertex(1, 1, 0)
                .AddVertex(2, 1, 0)
                .AddQuad(0, 1, 4, 3)
                .AddQuad(1, 2, 5, 4)
                .Build();
            var adj = MeshAdjacency.Build(im);
            adj.Neighbors[0][1].Should().Be(1);
            adj.Neighbors[1][3].Should().Be(0);
            adj.BoundaryEdges.Should().HaveCount(6);
        }
    }
}
