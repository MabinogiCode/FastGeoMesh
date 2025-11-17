using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.ComplexScenario
{
    public sealed class HighRefinementStressTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(0.8, 0.8), new Vec2(1.2, 0.8), new Vec2(1.2, 1.2), new Vec2(0.8, 1.2) });
            var structure = new PrismStructureDefinition(outer, 0, 0.5).AddHole(hole).AddConstraintSegment(new Segment2D(new Vec2(0, 1), new Vec2(2, 1)), 0.25);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.05)
                .WithTargetEdgeLengthZ(0.05)
                .WithHoleRefinement(0.02, 0.5)
                .WithSegmentRefinement(0.02, 0.3)
                .WithMinCapQuadQuality(0.2)
                .Build()
                .UnwrapForTests();
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(100);
            indexed.QuadCount.Should().BeGreaterThan(80);
        }
    }
}
