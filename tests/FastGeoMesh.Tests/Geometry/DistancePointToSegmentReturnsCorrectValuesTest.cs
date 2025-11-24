using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class DistancePointToSegmentReturnsCorrectValuesTest
    {
        [Fact]
        public void Test()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var a = new Vec2(0, 0);
            var b = new Vec2(10, 0);

            var pointOnSegment = new Vec2(5, 0);
            helper.DistancePointToSegment(pointOnSegment, a, b).Should().BeApproximately(0.0, 1e-9);

            var pointAbove = new Vec2(5, 3);
            helper.DistancePointToSegment(pointAbove, a, b).Should().BeApproximately(3.0, 1e-9);

            var pointBefore = new Vec2(-2, 4);
            var expectedBefore = System.Math.Sqrt(4 + 16);
            helper.DistancePointToSegment(pointBefore, a, b).Should().BeApproximately(expectedBefore, 1e-9);

            var pointAfter = new Vec2(12, 5);
            var expectedAfter = System.Math.Sqrt(4 + 25);
            helper.DistancePointToSegment(pointAfter, a, b).Should().BeApproximately(expectedAfter, 1e-9);
        }
    }
}
