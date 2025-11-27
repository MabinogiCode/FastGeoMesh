using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class PointInPolygonHandlesEdgeCasesTheoryTest.
    /// </summary>
    public sealed class PointInPolygonHandlesEdgeCasesTheoryTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(5, 0, true)]
        [InlineData(5, 5, true)]
        [InlineData(-1, 5, false)]
        [InlineData(11, 5, false)]
        [InlineData(5, -1, false)]
        [InlineData(5, 11, false)]
        public void Test(double x, double y, bool expected)
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var square = new Vec2[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
            helper.PointInPolygon(square, x, y).Should().Be(expected);
        }
    }
}
