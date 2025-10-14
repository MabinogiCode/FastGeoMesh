using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.ComplexScenario
{
    public sealed class StarShapedPolygonWithHoleTest
    {
        [Fact]
        public void Test()
        {
            var starVertices = new Vec2[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = 2 * Math.PI * i / 10;
                double r = (i % 2 == 0) ? 3.0 : 1.5;
                starVertices[i] = new Vec2(r * Math.Cos(angle), r * Math.Sin(angle));
            }
            var outer = Polygon2D.FromPoints(starVertices);
            var holeVertices = new Vec2[8];
            for (int i = 0; i < 8; i++)
            {
                double angle = 2 * Math.PI * i / 8;
                holeVertices[i] = new Vec2(0.3 * Math.Cos(angle), 0.3 * Math.Sin(angle));
            }
            var hole = Polygon2D.FromPoints(holeVertices);
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.3)
                .WithTargetEdgeLengthZ(0.5)
                .WithMinCapQuadQuality(0.1)
                .WithRejectedCapTriangles(true)
                .Build()
                .UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(20);
            (indexed.QuadCount + indexed.TriangleCount).Should().BeGreaterThan(15);
        }
    }
}
