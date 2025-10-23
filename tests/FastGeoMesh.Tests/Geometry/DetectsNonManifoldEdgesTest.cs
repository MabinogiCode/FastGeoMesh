using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class DetectsNonManifoldEdgesTest {
        [Fact]
        public void Test() {
            var builder = new IndexedMeshBuilder()
                .AddVertex(0, 0, 0)
                .AddVertex(1, 0, 0)
                .AddVertex(0, 1, 0)
                .AddVertex(1, 1, 0)
                .AddVertex(0, -1, 0)
                .AddVertex(1, -1, 0)
                .AddVertex(-1, 0, 0)
                .AddVertex(2, 0, 0);
            _ = builder.AddQuad(0, 1, 3, 2);
            _ = builder.AddQuad(4, 5, 1, 0);
            _ = builder.AddQuad(6, 0, 1, 7);
            var im = builder.Build();
            var adj = MeshAdjacency.Build(im);
            _ = adj.NonManifoldEdges.Should().ContainSingle();
            var e = adj.NonManifoldEdges[0];
            _ = (System.Math.Min(e.a, e.b), System.Math.Max(e.a, e.b)).Should().Be((0, 1));
        }
    }
}
