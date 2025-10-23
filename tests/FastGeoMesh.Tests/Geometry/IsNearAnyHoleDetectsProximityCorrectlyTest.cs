using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class IsNearAnyHoleDetectsProximityCorrectlyTest {
        [Fact]
        public void Test() {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);

            MeshStructureHelper.IsNearAnyHole(structure, 5, 5, 0.5).Should().BeFalse();
            MeshStructureHelper.IsNearAnyHole(structure, 3.5, 5, 0.6).Should().BeTrue();
            MeshStructureHelper.IsNearAnyHole(structure, 1, 1, 0.5).Should().BeFalse();
        }
    }
}
