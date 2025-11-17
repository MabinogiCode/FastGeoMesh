using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Domain
{
    public sealed class AreaInvariantRectangleCapsApproximateFootprintAreaMinusHolesTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 6), new Vec2(0, 6) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            double footprintArea = System.Math.Abs(Polygon2D.SignedArea(outer.Vertices)) - System.Math.Abs(Polygon2D.SignedArea(hole.Vertices));
            var topQuads = mesh.Quads.Where(q => System.Math.Abs(q.V0.Z - 2) < 1e-9 && q.QualityScore.HasValue).ToList();
            double SumArea() => topQuads.Sum(q => System.Math.Abs(Polygon2D.SignedArea(new[] { new Vec2(q.V0.X, q.V0.Y), new Vec2(q.V1.X, q.V1.Y), new Vec2(q.V2.X, q.V2.Y), new Vec2(q.V3.X, q.V3.Y) })));
            double capArea = SumArea();
            capArea.Should().BeGreaterThan(0);
            capArea.Should().BeInRange(footprintArea * 0.75, footprintArea * 1.25);
        }
    }
}
