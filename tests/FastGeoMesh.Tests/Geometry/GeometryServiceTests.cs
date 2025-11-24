using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class GeometryServiceTests
    {
        [Fact]
        public void DistancePointToSegment_HandlesDegenerateSegment()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var a = new Vec2(1, 1);
            var b = new Vec2(1, 1); // degenerate
            var p = new Vec2(4, 5);

            var d = helper.DistancePointToSegment(p, a, b);
            d.Should().BeApproximately(Math.Sqrt(3 * 3 + 4 * 4), 1e-9);
        }

        [Fact]
        public void PointInPolygon_TreatsBoundaryAsInside()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var square = new Vec2[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };

            // corner
            helper.PointInPolygon(square, 0, 0).Should().BeTrue();
            // edge
            helper.PointInPolygon(square, 5, 0).Should().BeTrue();
        }

        [Fact]
        public void BatchPointInPolygon_ThrowsOnMismatchedResultsLength()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var triangle = new Vec2[] { new(0, 0), new(6, 0), new(3, 6) };
            var points = new Vec2[] { new(1, 1), new(7, 1) };
            var results = new bool[1]; // wrong length

            Assert.Throws<ArgumentException>(() => helper.BatchPointInPolygon(triangle, points, results));
        }

        [Fact]
        public void PolygonArea_ComputesExpectedValues()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var square = new Vec2[] { new(0,0), new(4,0), new(4,4), new(0,4) };
            helper.PolygonArea(square).Should().BeApproximately(16.0, 1e-9);

            var triangle = new Vec2[] { new(0,0), new(10,0), new(0,10) };
            helper.PolygonArea(triangle).Should().BeApproximately(50.0, 1e-9);
        }
    }
}
