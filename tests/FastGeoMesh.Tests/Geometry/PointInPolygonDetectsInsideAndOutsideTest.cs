using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class PointInPolygonDetectsInsideAndOutsideTest {
        [Fact]
        public void Test() {
            var square = new Vec2[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
            GeometryHelper.PointInPolygon(square, 5, 5).Should().BeTrue();
            GeometryHelper.PointInPolygon(square, 0, 0).Should().BeTrue();
            GeometryHelper.PointInPolygon(square, 5, 0).Should().BeTrue();
            GeometryHelper.PointInPolygon(square, -1, 5).Should().BeFalse();
            GeometryHelper.PointInPolygon(square, 11, 5).Should().BeFalse();
            GeometryHelper.PointInPolygon(square, 5, -1).Should().BeFalse();
            GeometryHelper.PointInPolygon(square, 5, 11).Should().BeFalse();
        }
    }
}
