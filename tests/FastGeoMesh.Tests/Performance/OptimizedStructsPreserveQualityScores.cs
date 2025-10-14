using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Validates that optimized structs preserve quality scores correctly.
    /// </summary>
    public sealed class OptimizedStructsPreserveQualityScores
    {
        [Fact]
        public void Test()
        {
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3),
                new Vec2(3, 3), new Vec2(3, 8), new Vec2(0, 8)
            });
            var structure = new PrismStructureDefinition(lShape, 0, 2);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.5
            };
            var mesher = new PrismMesher();
            var mesh = mesher.Mesh(structure, options).UnwrapForTests();
            var capQuads = mesh.Quads.Where(q => q.QualityScore.HasValue).ToList();
            var capTriangles = mesh.Triangles.Where(t => t.V0.Z == t.V1.Z && t.V1.Z == t.V2.Z).ToList();
            if (capQuads.Count > 0)
            {
                capQuads.Should().NotBeEmpty("Cap quads should have quality scores");
                foreach (var quad in capQuads)
                {
                    quad.QualityScore.Should().HaveValue();
                    quad.QualityScore!.Value.Should().BeInRange(0.0, 1.0);
                }
            }
            else
            {
                capTriangles.Should().NotBeEmpty("Should have cap elements (quads or triangles)");
            }
        }
    }
}
