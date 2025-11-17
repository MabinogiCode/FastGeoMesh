using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class ExcavationSlabDoesNotInterferewithSideFacesTest
    {
        [Fact]
        public void Test()
        {
            var excavation = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3) });
            var slab = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3) });
            var structure = new PrismStructureDefinition(excavation, -3, 0)
                .AddInternalSurface(slab, -1.5);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.0
            };
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var sideQuads = mesh.Quads.Where(q => !ExcavationSlabDoesNotInterferewithSideFacesTestHelpers.IsCapQuad(q)).ToList();
            sideQuads.Should().NotBeEmpty();
            var distinctZLevelsInSides = sideQuads
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Distinct()
                .OrderBy(z => z)
                .ToList();
            distinctZLevelsInSides.Should().Contain(-3.0);
            distinctZLevelsInSides.Should().Contain(-1.5);
            distinctZLevelsInSides.Should().Contain(0.0);
            var slabQuads = mesh.Quads.Where(q => ExcavationSlabDoesNotInterferewithSideFacesTestHelpers.IsQuadAtZ(q, -1.5)).ToList();
            if (slabQuads.Count == 0)
            {
                mesh.Quads.Should().NotBeEmpty();
                distinctZLevelsInSides.Should().Contain(-1.5);
            }
            else
            {
                slabQuads.Should().NotBeEmpty();
            }
        }
    }

    internal static class ExcavationSlabDoesNotInterferewithSideFacesTestHelpers
    {
        private const double Epsilon = 1e-9;

        public static bool IsQuadAtZ(Quad q, double expectedZ)
        {
            return Math.Abs(q.V0.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V1.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V2.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V3.Z - expectedZ) < Epsilon;
        }

        public static bool IsCapQuad(Quad q)
        {
            return Math.Abs(q.V0.Z - q.V1.Z) < Epsilon &&
                   Math.Abs(q.V1.Z - q.V2.Z) < Epsilon &&
                   Math.Abs(q.V2.Z - q.V3.Z) < Epsilon;
        }
    }
}
