using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Infrastructure.Services
{
    public class GeometryServiceTests
    {
        private readonly GeometryService _service = new GeometryService();

        [Fact]
        public void ConstructorWithNullConfigUsesDefault()
        {
            var service = new GeometryService(null);
            service.Should().NotBeNull();
        }

        [Theory]
        [InlineData(0, 0, 0, 1, 1, 1, 1)] // Point below horizontal segment at y=1
        [InlineData(0.5, 0.5, 0, 0, 1, 1, 0)] // Point on segment
        [InlineData(-1, 0, 0, 0, 1, 0, 1)] // Point to the left
        [InlineData(2, 0, 0, 0, 1, 0, 1)] // Point to the right
        [InlineData(0.5, 1, 0, 0, 1, 0, 1)] // Point above
        public void DistancePointToSegmentCalculatesCorrectly(double px, double py, double ax, double ay, double bx, double by, double expected)
        {
            var p = new Vec2(px, py);
            var a = new Vec2(ax, ay);
            var b = new Vec2(bx, by);
            _service.DistancePointToSegment(p, a, b).Should().BeApproximately(expected, 1e-9);
        }

        [Fact]
        public void DistancePointToSegmentWithZeroLengthSegmentReturnsDistanceToPoint()
        {
            var p = new Vec2(1, 1);
            var a = new Vec2(0, 0);
            _service.DistancePointToSegment(p, a, a).Should().BeApproximately(Math.Sqrt(2), 1e-9);
        }

        [Fact]
        public void IsConvexWithConvexQuadReturnsTrue()
        {
            var quad = (new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1));
            _service.IsConvex(quad).Should().BeTrue();
        }

        [Fact]
        public void IsConvexWithConcaveQuadReturnsFalse()
        {
            // A clearly concave quad with one interior angle > 180°
            var quad = (new Vec2(0, 0), new Vec2(2, 0), new Vec2(1, -1), new Vec2(0, 2));
            _service.IsConvex(quad).Should().BeFalse();
        }

        [Fact]
        public void PointInPolygonWithPointInsideReturnsTrue()
        {
            var vertices = new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
            _service.PointInPolygon(vertices, new Vec2(0.5, 0.5)).Should().BeTrue();
        }

        [Fact]
        public void PointInPolygonWithPointOnEdgeReturnsTrue()
        {
            var vertices = new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
            _service.PointInPolygon(vertices, new Vec2(0.5, 0)).Should().BeTrue();
        }

        [Fact]
        public void PointInPolygonWithLessThan3VerticesReturnsFalse()
        {
            var vertices = new Vec2[] { new(0, 0), new(1, 1) };
            _service.PointInPolygon(vertices, new Vec2(0.5, 0.5)).Should().BeFalse();
        }

        [Fact]
        public void BatchPointInPolygonWithMismatchedLengthsThrowsArgumentException()
        {
            var vertices = new Vec2[] { new(0, 0), new(1, 0), new(1, 1) };
            var points = new Vec2[] { new(0.5, 0.5) };
            var results = new bool[2];
            Action act = () => _service.BatchPointInPolygon(vertices, points, results);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void PolygonAreaWithLessThan3VerticesReturnsZero()
        {
            var vertices = new Vec2[] { new(0, 0), new(1, 1) };
            _service.PolygonArea(vertices).Should().Be(0);
        }

        [Fact]
        public void CentroidWithEmptyListReturnsZero()
        {
            _service.Centroid(Array.Empty<Vec2>()).Should().Be(Vec2.Zero);
        }

        [Fact]
        public void NormalizeWithZeroVectorReturnsZero()
        {
            _service.Normalize(Vec2.Zero).Should().Be(Vec2.Zero);
        }

        [Fact]
        public void ClampWithValueInRangeReturnsValue()
        {
            _service.Clamp(5, 0, 10).Should().Be(5);
        }

        [Fact]
        public void ClampWithValueBelowMinReturnsMin()
        {
            _service.Clamp(-5, 0, 10).Should().Be(0);
        }

        [Fact]
        public void ClampWithValueAboveMaxReturnsMax()
        {
            _service.Clamp(15, 0, 10).Should().Be(10);
        }

        [Fact]
        public void LerpVec3InterpolatesCorrectly()
        {
            var a = new Vec3(0, 0, 0);
            var b = new Vec3(1, 1, 1);
            _service.Lerp(a, b, 0.5).Should().Be(new Vec3(0.5, 0.5, 0.5));
        }
    }
}
