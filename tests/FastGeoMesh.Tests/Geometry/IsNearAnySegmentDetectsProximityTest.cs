using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class IsNearAnySegmentDetectsProximityTest {
        [Fact]
        public void Test() {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 8), new Vec2(0, 8) });
            var st = new PrismStructureDefinition(outer, 0, 3);
            st.Geometry.AddSegment(new Segment3D(new Vec3(0, 0, 0), new Vec3(8, 8, 0)));
            st.Geometry.AddSegment(new Segment3D(new Vec3(0, 8, 0), new Vec3(8, 0, 0)));
            MeshStructureHelper.IsNearAnySegment(st, 4.1, 4.0, 0.3).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(st, 4.1, 4.0, 0.01).Should().BeFalse();
        }
    }
}
