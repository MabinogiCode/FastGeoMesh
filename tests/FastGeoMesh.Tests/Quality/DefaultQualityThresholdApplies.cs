using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Tests that default quality threshold applies correctly.
    /// </summary>
    public sealed class DefaultQualityThresholdApplies
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 1), new Vec2(2.6, 1), new Vec2(2.4, 3), new Vec2(5, 3), new Vec2(5, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var withDefault = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.5), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true };
            var loose = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.5), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true, MinCapQuadQuality = 0.0 };
            var meshDef = new PrismMesher().Mesh(structure, withDefault).UnwrapForTests();
            var meshLoose = new PrismMesher().Mesh(structure, loose).UnwrapForTests();

            bool IsTop(Quad q) => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1;
            bool IsTopTriangle(Triangle t) => t.V0.Z == 1 && t.V1.Z == 1 && t.V2.Z == 1;

            var topDefQuads = meshDef.Quads.Where(IsTop).ToList();
            var topDefTriangles = meshDef.Triangles.Where(IsTopTriangle).ToList();
            var topDefElements = topDefQuads.Count + topDefTriangles.Count;

            var topLooseQuads = meshLoose.Quads.Where(IsTop).ToList();
            var topLooseTriangles = meshLoose.Triangles.Where(IsTopTriangle).ToList();
            var topLooseElements = topLooseQuads.Count + topLooseTriangles.Count;

            topDefElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            topLooseElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");

            double thresh = 0.3;
            int badDef = topDefQuads.Count(q => q.QualityScore is { } s && s < thresh);
            int badLoose = topLooseQuads.Count(q => q.QualityScore is { } s && s < thresh);

            if (topDefQuads.Count > 0 && topLooseQuads.Count > 0)
            {
                badDef.Should().BeLessThanOrEqualTo(badLoose + 1);
            }
        }
    }
}
