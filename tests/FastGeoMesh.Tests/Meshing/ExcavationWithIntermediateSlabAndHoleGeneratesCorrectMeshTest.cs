using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers; // Required for UnwrapForTests extension
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class ExcavationWithIntermediateSlabAndHoleGeneratesCorrectMeshTest
    {
        [Fact]
        public void Test()
        {
            var excavationFootprint = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
            });
            var slabOutline = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
            });
            var slabHole = Polygon2D.FromPoints(new[]
            {
                new Vec2(2, 2), new Vec2(3, 2), new Vec2(3, 3), new Vec2(2, 3)
            });
            var structure = new PrismStructureDefinition(excavationFootprint, -5, 0)
                .AddInternalSurface(slabOutline, -2.5, slabHole);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(0.5),
                TargetEdgeLengthZ = EdgeLength.From(0.5),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.1
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            mesh.Quads.Should().NotBeEmpty();
            var bottomQuads = mesh.Quads.Where(q => ExcavationWithIntermediateSlabAndHoleHelpers.IsQuadAtZ(q, -5)).ToList();
            bottomQuads.Should().NotBeEmpty();
            var topQuads = mesh.Quads.Where(q => ExcavationWithIntermediateSlabAndHoleHelpers.IsQuadAtZ(q, 0)).ToList();
            topQuads.Should().NotBeEmpty();
            var slabQuads = mesh.Quads.Where(q => ExcavationWithIntermediateSlabAndHoleHelpers.IsQuadAtZ(q, -2.5)).ToList();
            slabQuads.Should().NotBeEmpty();
            foreach (var quad in slabQuads)
            {
                double centerX = (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) * 0.25;
                double centerY = (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) * 0.25;
                bool insideHole = centerX > 2.0 && centerX < 3.0 && centerY > 2.0 && centerY < 3.0;
                insideHole.Should().BeFalse();
            }
            var sideQuads = mesh.Quads.Where(q => !ExcavationWithIntermediateSlabAndHoleHelpers.IsCapQuad(q)).ToList();
            sideQuads.Should().NotBeEmpty();
            var allZValues = mesh.Quads
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Distinct()
                .OrderBy(z => z)
                .ToList();
            allZValues.Should().Contain(-5.0);
            allZValues.Should().Contain(-2.5);
            allZValues.Should().Contain(0.0);
        }
    }

    internal static class ExcavationWithIntermediateSlabAndHoleHelpers
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
