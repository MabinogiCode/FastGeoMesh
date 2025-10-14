using FastGeoMesh.Application.Helpers.Meshing;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class GenericCapsProduceQualityScoresWithinRangeTest
    {
        [Fact]
        public void Test()
        {
            var concave = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 2), new Vec2(2, 2), new Vec2(2, 6), new Vec2(0, 6) });
            var structure = new PrismStructureDefinition(concave, -1, 0);
            var opt = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.75), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new ImmutableMesh();
            var resultMesh = CapMeshingHelper.GenerateCaps(mesh, structure, opt, -1, 0);
            var capQuads = resultMesh.Quads.Where(q => q.V0.Z == -1 || q.V0.Z == 0).ToList();
            var capTriangles = resultMesh.Triangles.Where(t => t.V0.Z == -1 || t.V0.Z == 0).ToList();
            (capQuads.Count + capTriangles.Count).Should().BeGreaterThan(0);
            foreach (var q in capQuads.Where(q => q.QualityScore.HasValue))
            {
                q.QualityScore!.Value.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1);
            }
        }
    }
}
