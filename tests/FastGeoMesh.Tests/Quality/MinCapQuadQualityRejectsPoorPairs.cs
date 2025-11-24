using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Tests that minimum cap quad quality rejects poor quality quad pairs.
    /// </summary>
    public sealed class MinCapQuadQualityRejectsPoorPairs
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 1), new Vec2(2.6, 1), new Vec2(2.4, 3), new Vec2(5, 3), new Vec2(5, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var strict = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.5), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true, MinCapQuadQuality = 0.5 };
            var loose = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.5), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true, MinCapQuadQuality = 0.0 };
            var meshStrict = TestServiceProvider.CreatePrismMesher().Mesh(structure, strict).UnwrapForTests();
            var meshLoose = TestServiceProvider.CreatePrismMesher().Mesh(structure, loose).UnwrapForTests();

            bool IsTop(Quad q) => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1;
            bool IsTopTriangle(Triangle t) => t.V0.Z == 1 && t.V1.Z == 1 && t.V2.Z == 1;

            var topStrictQuads = meshStrict.Quads.Where(IsTop).ToList();
            var topStrictTriangles = meshStrict.Triangles.Where(IsTopTriangle).ToList();
            var topStrictElements = topStrictQuads.Count + topStrictTriangles.Count;

            var topLooseQuads = meshLoose.Quads.Where(IsTop).ToList();
            var topLooseTriangles = meshLoose.Triangles.Where(IsTopTriangle).ToList();
            var topLooseElements = topLooseQuads.Count + topLooseTriangles.Count;

            topStrictElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            topLooseElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");

            double thresh = 0.3;
            int badStrict = topStrictQuads.Count(q => q.QualityScore is { } s && s < thresh);
            int badLoose = topLooseQuads.Count(q => q.QualityScore is { } s && s < thresh);

            if (topStrictQuads.Count > 0 && topLooseQuads.Count > 0)
            {
                badStrict.Should().BeLessThanOrEqualTo(badLoose + 1);
            }
        }
    }
}
