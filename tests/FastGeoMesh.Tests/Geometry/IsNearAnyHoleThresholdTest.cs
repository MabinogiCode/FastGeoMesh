using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class IsNearAnyHoleThresholdTest.
    /// </summary>
    public sealed class IsNearAnyHoleThresholdTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Theory]
        [InlineData(4, 4, 0.5, true)]
        [InlineData(5, 4, 0.1, true)]
        [InlineData(5, 5, 1.2, true)]
        [InlineData(3.9, 5, 0.05, false)]
        [InlineData(3.9, 5, 0.2, true)]
        public void Test(double x, double y, double band, bool expected)
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var st = new PrismStructureDefinition(outer, 0, 5).AddHole(hole);

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<FastGeoMesh.Domain.Services.IGeometryService>();

            MeshStructureHelper.IsNearAnyHole(st, x, y, band, geo).Should().Be(expected);
        }
    }
}
