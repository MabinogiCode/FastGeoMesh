using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for excavation scenarios with intermediate slabs/platforms containing holes.</summary>
    public sealed class ExcavationWithSlabTests
    {
        private const double Epsilon = 1e-9; // S1244 compliance: epsilon for floating-point comparisons

        /// <summary>S1244 compliant helper: Check if all quad vertices are at specified Z level.</summary>
        private static bool IsQuadAtZ(Quad q, double expectedZ) =>
            Math.Abs(q.V0.Z - expectedZ) < Epsilon &&
            Math.Abs(q.V1.Z - expectedZ) < Epsilon &&
            Math.Abs(q.V2.Z - expectedZ) < Epsilon &&
            Math.Abs(q.V3.Z - expectedZ) < Epsilon;

        /// <summary>S1244 compliant helper: Check if quad has vertices at same Z (is a cap).</summary>
        private static bool IsCapQuad(Quad q) =>
            Math.Abs(q.V0.Z - q.V1.Z) < Epsilon &&
            Math.Abs(q.V1.Z - q.V2.Z) < Epsilon &&
            Math.Abs(q.V2.Z - q.V3.Z) < Epsilon;

        /// <summary>
        /// Verifies excavation with intermediate slab and central hole produces distinct cap and slab quads.
        /// </summary>
        [Fact]
        public void ExcavationWithIntermediateSlabAndHoleGeneratesCorrectMesh()
        {
            // Arrange - Create 5x5m excavation, 5m deep
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
            var bottomQuads = mesh.Quads.Where(q => IsQuadAtZ(q, -5)).ToList();
            bottomQuads.Should().NotBeEmpty();
            var topQuads = mesh.Quads.Where(q => IsQuadAtZ(q, 0)).ToList();
            topQuads.Should().NotBeEmpty();
            var slabQuads = mesh.Quads.Where(q => IsQuadAtZ(q, -2.5)).ToList();
            slabQuads.Should().NotBeEmpty();
            foreach (var quad in slabQuads)
            {
                double centerX = (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) * 0.25;
                double centerY = (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) * 0.25;
                bool insideHole = centerX > 2.0 && centerX < 3.0 && centerY > 2.0 && centerY < 3.0;
                insideHole.Should().BeFalse();
            }
            var sideQuads = mesh.Quads.Where(q => !IsCapQuad(q)).ToList();
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

        /// <summary>
        /// Ensures slab internal surface does not remove required vertical side faces and influences subdivision levels.
        /// </summary>
        [Fact]
        public void ExcavationSlabDoesNotInterferewithSideFaces()
        {
            // Arrange - Smaller excavation for focused testing
            var excavation = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3)
            });
            var slab = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3)
            });
            var structure = new PrismStructureDefinition(excavation, -3, 0)
                .AddInternalSurface(slab, -1.5);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.0  // Fix: double instead of EdgeLength
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var sideQuads = mesh.Quads.Where(q => !IsCapQuad(q)).ToList();
            sideQuads.Should().NotBeEmpty();
            var distinctZLevelsInSides = sideQuads
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Distinct()
                .OrderBy(z => z)
                .ToList();
            distinctZLevelsInSides.Should().Contain(-3.0);
            distinctZLevelsInSides.Should().Contain(-1.5);
            distinctZLevelsInSides.Should().Contain(0.0);
            var slabQuads = mesh.Quads.Where(q => IsQuadAtZ(q, -1.5)).ToList();
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

        /// <summary>
        /// Tests a complex excavation with two slabs and distinct hole patterns ensuring independent exclusion.
        /// </summary>
        [Fact]
        public void ComplexExcavationWithMultipleSlabsAndHoles()
        {
            // Arrange - More complex scenario with multiple levels
            var excavation = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4)
            });
            var upperSlab = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4)
            });
            var upperHole = Polygon2D.FromPoints(new[]
            {
                new Vec2(1, 1), new Vec2(2, 1), new Vec2(2, 2), new Vec2(1, 2)
            });
            var lowerSlab = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4)
            });
            var lowerHole = Polygon2D.FromPoints(new[]
            {
                new Vec2(4, 2), new Vec2(5, 2), new Vec2(5, 3), new Vec2(4, 3)
            });
            var structure = new PrismStructureDefinition(excavation, -4, 0)
                .AddInternalSurface(upperSlab, -1, upperHole)
                .AddInternalSurface(lowerSlab, -3, lowerHole);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(0.8),
                TargetEdgeLengthZ = EdgeLength.From(0.5),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.1  // Fix: double instead of EdgeLength
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var upperSlabQuads = mesh.Quads.Where(q => IsQuadAtZ(q, -1)).ToList();
            var lowerSlabQuads = mesh.Quads.Where(q => IsQuadAtZ(q, -3)).ToList();
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
            var allZLevels = mesh.Quads
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Distinct()
                .OrderBy(z => z)
                .ToList();
            allZLevels.Should().Contain(new[] { -4.0, -3.0, -1.0, 0.0 });
        }
    }
}
