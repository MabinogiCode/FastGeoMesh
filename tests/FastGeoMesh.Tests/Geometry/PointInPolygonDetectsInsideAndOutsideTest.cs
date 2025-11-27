using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class PointInPolygonDetectsInsideAndOutsideTest.
    /// </summary>
    public sealed class PointInPolygonDetectsInsideAndOutsideTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var square = new Vec2[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
            helper.PointInPolygon(square, 5, 5).Should().BeTrue();
            helper.PointInPolygon(square, 0, 0).Should().BeTrue();
            helper.PointInPolygon(square, 5, 0).Should().BeTrue();
            helper.PointInPolygon(square, -1, 5).Should().BeFalse();
            helper.PointInPolygon(square, 11, 5).Should().BeFalse();
            helper.PointInPolygon(square, 5, -1).Should().BeFalse();
            helper.PointInPolygon(square, 5, 11).Should().BeFalse();
        }
    }
}
