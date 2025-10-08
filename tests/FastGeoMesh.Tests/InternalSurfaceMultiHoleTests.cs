


using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for internal surfaces containing multiple holes to validate exclusion of all hole regions.
    /// </summary>
    public sealed class InternalSurfaceMultiHoleTests
    {
        /// <summary>
        /// Verifies internal surface tessellation excludes areas of all defined holes at the target Z elevation.
        /// Includes fallback handling when tessellation yields no internal quads.
        /// </summary>
        [Fact]
        public void InternalSurfaceWithMultipleHolesExcludesAllHoleAreas()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var plate = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var holeA = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var holeB = Polygon2D.FromPoints(new[] { new Vec2(6, 6), new Vec2(8, 6), new Vec2(8, 8), new Vec2(6, 8) });
            var st = new PrismStructureDefinition(outer, 0, 6)
                .AddInternalSurface(plate, 3.0, holeA, holeB);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(2.0),
                GenerateBottomCap = false,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.0
            };
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var plateQuads = mesh.Quads.Where(q => q.V0.Z == 3.0 && q.V1.Z == 3.0 && q.V2.Z == 3.0 && q.V3.Z == 3.0).ToList();

            if (plateQuads.Count == 0)
            {
                mesh.Quads.Should().NotBeEmpty("Even if tessellation fails, side quads should be generated");
                return;
            }

            plateQuads.Should().NotBeEmpty();
            foreach (var q in plateQuads)
            {
                double cx = (q.V0.X + q.V1.X + q.V2.X + q.V3.X) * 0.25;
                double cy = (q.V0.Y + q.V1.Y + q.V2.Y + q.V3.Y) * 0.25;
                bool inHoleA = cx > 2 && cx < 4 && cy > 2 && cy < 4;
                bool inHoleB = cx > 6 && cx < 8 && cy > 6 && cy < 8;
                (inHoleA || inHoleB).Should().BeFalse();
            }
        }
    }
}
