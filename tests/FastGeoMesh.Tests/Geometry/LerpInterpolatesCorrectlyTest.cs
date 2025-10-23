using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class LerpInterpolatesCorrectlyTest {
        [Fact]
        public void Test() {
            var a = new Vec2(0, 0);
            var b = new Vec2(10, 20);
            var start = GeometryHelper.Lerp(a, b, 0.0);
            start.Should().Be(a);
            var end = GeometryHelper.Lerp(a, b, 1.0);
            end.Should().Be(b);
            var middle = GeometryHelper.Lerp(a, b, 0.5);
            middle.Should().Be(new Vec2(5, 10));
            var quarter = GeometryHelper.Lerp(a, b, 0.25);
            quarter.Should().Be(new Vec2(2.5, 5));
        }
    }
}
