using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class PointInPolygonHandlesEdgeCasesTheoryTest
    {
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
            var square = new Vec2[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
            GeometryHelper.PointInPolygon(square, x, y).Should().Be(expected);
        }
    }
}
