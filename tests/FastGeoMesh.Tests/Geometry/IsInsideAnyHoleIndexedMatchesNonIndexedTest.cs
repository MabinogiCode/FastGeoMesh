using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class IsInsideAnyHoleIndexedMatchesNonIndexedTest {
        [Fact]
        public void Test() {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var st = new PrismStructureDefinition(outer, 0, 5).AddHole(hole);
            var holeIdx = st.Holes.Select(h => new SpatialPolygonIndex(h.Vertices)).ToArray();
            for (double x = 0; x <= 10; x += 0.5) {
                for (double y = 0; y <= 10; y += 0.5) {
                    bool a = MeshStructureHelper.IsInsideAnyHole(st, x, y);
                    bool b = MeshStructureHelper.IsInsideAnyHole(holeIdx, x, y);
                    a.Should().Be(b);
                }
            }
        }
    }
}
