using FastGeoMesh.Application;
using FastGeoMesh.Domain;

using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Integration test verifying generated mesh adjacency produces no non-manifold edges.
    /// </summary>
    public sealed class AdjacencyIntegrationTests
    {
        /// <summary>
        /// Builds a sample mesh and asserts adjacency analysis contains zero non-manifold edges.
        /// </summary>
        [Fact]
        public void AdjacencyOnGeneratedMeshHasNoNonManifoldEdges()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
