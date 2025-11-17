using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Integration
{
    public sealed class AdjacencyOnGeneratedMeshHasNoNonManifoldEdgesTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
