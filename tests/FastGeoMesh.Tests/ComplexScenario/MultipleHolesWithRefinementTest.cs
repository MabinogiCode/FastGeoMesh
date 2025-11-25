using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.ComplexScenario
{
    public sealed class MultipleHolesWithRefinementTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(6, 3),
                new Vec2(6, 5), new Vec2(8, 5), new Vec2(8, 8), new Vec2(0, 8)
            });
            var hole1 = Polygon2D.FromPoints(new[] { new Vec2(1, 1), new Vec2(2, 1), new Vec2(2, 2), new Vec2(1, 2) });
            var hole2 = Polygon2D.FromPoints(new[] { new Vec2(6.5, 1), new Vec2(7.5, 1), new Vec2(7.5, 2), new Vec2(6.5, 2) });
            var hole3 = Polygon2D.FromPoints(new[] { new Vec2(2, 6), new Vec2(3, 6), new Vec2(3, 7), new Vec2(2, 7) });
            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole1).AddHole(hole2).AddHole(hole3);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.75)
                .WithTargetEdgeLengthZ(1.0)
                .WithMinCapQuadQuality(0.95)
                .WithRejectedCapTriangles(true)
                .Build()
                .UnwrapForTests();
            var result = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(result);
            indexed.VertexCount.Should().BeGreaterThan(50);
            indexed.QuadCount.Should().BeGreaterThan(30);
            indexed.TriangleCount.Should().BeGreaterThan(0);
        }
    }
}
