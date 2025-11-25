using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class ComplexExcavationWithMultipleSlabsAndHolesTest
    {
        [Fact]
        public void Test()
        {
            var excavation = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4) });
            var upperSlab = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4) });
            var upperHole = Polygon2D.FromPoints(new[] { new Vec2(1, 1), new Vec2(2, 1), new Vec2(2, 2), new Vec2(1, 2) });
            var lowerSlab = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4) });
            var lowerHole = Polygon2D.FromPoints(new[] { new Vec2(4, 2), new Vec2(5, 2), new Vec2(5, 3), new Vec2(4, 3) });
            var structure = new PrismStructureDefinition(excavation, -4, 0)
                .AddInternalSurface(upperSlab, -1, upperHole)
                .AddInternalSurface(lowerSlab, -3, lowerHole);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(0.8),
                TargetEdgeLengthZ = EdgeLength.From(0.5),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.1
            };
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var upperSlabQuads = mesh.Quads.Where(q => ComplexExcavationWithMultipleSlabsAndHolesTestHelpers.IsQuadAtZ(q, -1)).ToList();
            var lowerSlabQuads = mesh.Quads.Where(q => ComplexExcavationWithMultipleSlabsAndHolesTestHelpers.IsQuadAtZ(q, -3)).ToList();
            upperSlabQuads.Should().NotBeEmpty();
            lowerSlabQuads.Should().NotBeEmpty();
            foreach (var quad in upperSlabQuads)
            {
                double cx = (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) * 0.25;
                double cy = (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) * 0.25;
                bool insideUpperHole = cx > 1.0 && cx < 2.0 && cy > 1.0 && cy < 2.0;
                insideUpperHole.Should().BeFalse();
            }
            foreach (var quad in lowerSlabQuads)
            {
                double cx = (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) * 0.25;
                double cy = (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) * 0.25;
                bool insideLowerHole = cx > 4.0 && cx < 5.0 && cy > 2.0 && cy < 3.0;
                insideLowerHole.Should().BeFalse();
            }
            var allZLevels = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).Distinct().OrderBy(z => z).ToList();
            allZLevels.Should().Contain(new[] { -4.0, -3.0, -1.0, 0.0 });
        }
    }

    internal static class ComplexExcavationWithMultipleSlabsAndHolesTestHelpers
    {
        public static bool IsQuadAtZ(Quad q, double expectedZ)
        {
            const double Epsilon = 1e-9;
            return Math.Abs(q.V0.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V1.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V2.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V3.Z - expectedZ) < Epsilon;
        }
    }
}
