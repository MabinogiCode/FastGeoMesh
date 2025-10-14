using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Ensures cap quads have quality scores in valid [0,1] range.
    /// </summary>
    public sealed class CapsQuadsExposeQualityScoresBetween0And1
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 3), new Vec2(3, 3), new Vec2(3, 6), new Vec2(0, 6) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(1, 1), new Vec2(2, 1), new Vec2(2, 2), new Vec2(1, 2) });
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.75), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();

            var capQuads = mesh.Quads.Where(q => Math.Abs(q.V0.Z - q.V1.Z) < 1e-12 && Math.Abs(q.V1.Z - q.V2.Z) < 1e-12).ToList();
            var capTriangles = mesh.Triangles.Where(t => Math.Abs(t.V0.Z - t.V1.Z) < 1e-12 && Math.Abs(t.V1.Z - t.V2.Z) < 1e-12).ToList();
            var totalCapElements = capQuads.Count + capTriangles.Count;

            totalCapElements.Should().BeGreaterThan(0, "Should have cap elements (quads or triangles)");

            if (capQuads.Count > 0)
            {
                foreach (var q in capQuads)
                {
                    q.QualityScore.Should().NotBeNull("Cap quads should have quality scores");
                    q.QualityScore!.Value.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1);
                }
            }
        }
    }
}
