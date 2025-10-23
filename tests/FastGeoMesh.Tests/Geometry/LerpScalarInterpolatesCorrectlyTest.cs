using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class LerpScalarInterpolatesCorrectlyTest {
        [Fact]
        public void Test() {
            GeometryHelper.LerpScalar(0, 10, 0.0).Should().Be(0);
            GeometryHelper.LerpScalar(0, 10, 1.0).Should().Be(10);
            GeometryHelper.LerpScalar(0, 10, 0.5).Should().Be(5);
            GeometryHelper.LerpScalar(5, 15, 0.25).Should().Be(7.5);
            GeometryHelper.LerpScalar(-10, 10, 0.75).Should().Be(5);
        }
    }
}
