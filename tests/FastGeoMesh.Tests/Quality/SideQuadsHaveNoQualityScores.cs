using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Ensures side quads have no quality scores assigned (only caps should have scores).
    /// </summary>
    public sealed class SideQuadsHaveNoQualityScores
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var sideQuads = mesh.Quads.Where(q => !(Math.Abs(q.V0.Z - q.V1.Z) < 1e-12 && Math.Abs(q.V1.Z - q.V2.Z) < 1e-12)).ToList();
            sideQuads.Should().NotBeEmpty();
            sideQuads.All(q => q.QualityScore == null).Should().BeTrue();
        }
    }
}
