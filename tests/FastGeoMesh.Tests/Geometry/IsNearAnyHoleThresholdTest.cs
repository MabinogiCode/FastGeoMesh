using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class IsNearAnyHoleThresholdTest
    {
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
            MeshStructureHelper.IsNearAnyHole(st, x, y, band, new GeometryService()).Should().Be(expected);
        }
    }
}
