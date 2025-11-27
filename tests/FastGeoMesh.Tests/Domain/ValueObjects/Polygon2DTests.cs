using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Domain.ValueObjects
{
    public class Polygon2DTests
    {
        [Fact]
        public void ConstructorWithNullEnumerableShouldCreatePolygonWithEmptyVertices()
        {
            var polygon = new Polygon2D((IEnumerable<Vec2>)null!);
            polygon.Vertices.Should().BeEmpty();
        }

        [Fact]
        public void ConstructorWithLessThanThreeVerticesShouldCreatePolygon()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 1) };
            var polygon = new Polygon2D(vertices);
            polygon.Vertices.Should().HaveCount(2);
        }

        [Fact]
        public void ConstructorWithClockwiseVerticesShouldReverseToCCW()
        {
            var vertices = new List<Vec2> { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }; // Clockwise
            var polygon = new Polygon2D(vertices);
            // Implementation reverses the list when clockwise (does not rotate to keep start vertex)
            polygon.Vertices[0].Should().Be(new Vec2(1, 0));
            polygon.Vertices[1].Should().Be(new Vec2(1, 1));
            polygon.Vertices[2].Should().Be(new Vec2(0, 1));
            polygon.Vertices[3].Should().Be(new Vec2(0, 0));
        }

        [Fact]
        public void CreateValidatedWithNullVertsThrowsArgumentNullException()
        {
            Action act = () => Polygon2D.CreateValidated((IEnumerable<Vec2>)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateValidatedWithLessThan3VerticesThrowsArgumentException()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 1) };
            Action act = () => Polygon2D.CreateValidated(vertices);
            act.Should().Throw<ArgumentException>().WithMessage("Polygon must have at least 3 vertices.*");
        }

        [Fact]
        public void CreateValidatedWithCollinearVerticesThrowsArgumentException()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 1), new(2, 2) };
            Action act = () => Polygon2D.CreateValidated(vertices);
            act.Should().Throw<ArgumentException>().WithMessage("Invalid polygon: Degenerate area (collinear vertices)*");
        }

        [Fact]
        public void CreateValidatedWithZeroLengthEdgeThrowsArgumentException()
        {
            // Construct a non-degenerate polygon but with a zero-length edge between consecutive duplicate vertices
            var vertices = new List<Vec2> { new(0, 0), new(1, 0), new(1, 0), new(0, 1) };
            Action act = () => Polygon2D.CreateValidated(vertices);
            act.Should().Throw<ArgumentException>().WithMessage("Invalid polygon:*");
        }

        [Fact]
        public void CreateValidatedWithDuplicateVerticesThrowsArgumentException()
        {
            // Create a non-degenerate polygon with a non-adjacent duplicate to trigger duplicate detection specifically
            var vertices = new List<Vec2> { new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(1, 0) };
            Action act = () => Polygon2D.CreateValidated(vertices);
            act.Should().Throw<ArgumentException>().WithMessage("Invalid polygon: Duplicate/near-coincident vertices*");
        }

        [Fact]
        public void CreateValidatedWithSelfIntersectionThrowsArgumentException()
        {
            var vertices = new List<Vec2> { new(0, 0), new(3, 0), new(1, 2), new(2, -1) };
            Action act = () => Polygon2D.CreateValidated(vertices);
            act.Should().Throw<ArgumentException>().WithMessage("Invalid polygon:*");
        }

        [Fact]
        public void FromUnsafeWithNullVertsThrowsArgumentNullException()
        {
            Action act = () => Polygon2D.FromUnsafe((IEnumerable<Vec2>)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromUnsafeWithSelfIntersectionDoesNotThrow()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 1), new(0, 1), new(1, 0) };
            var polygon = Polygon2D.FromUnsafe(vertices);
            polygon.Should().NotBeNull();
            polygon.Vertices.Should().HaveCount(4);
        }

        [Fact]
        public void TryCreateWithValidVerticesReturnsTrueAndPolygon()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 0), new(1, 1) };
            var result = Polygon2D.TryCreate(vertices, out var polygon, out var error);
            result.Should().BeTrue();
            polygon.Should().NotBeNull();
            error.Should().BeNull();
        }

        [Fact]
        public void TryCreateWithNullVerticesReturnsFalse()
        {
            var result = Polygon2D.TryCreate((IEnumerable<Vec2>)null!, out var polygon, out var error);
            result.Should().BeFalse();
            polygon.Should().BeNull();
            error.Should().Be("Vertices is null");
        }

        [Fact]
        public void TryCreateWithLessThan3VerticesReturnsFalse()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 1) };
            var result = Polygon2D.TryCreate(vertices, out var polygon, out var error);
            result.Should().BeFalse();
            polygon.Should().BeNull();
            error.Should().Be("Less than 3 vertices");
        }

        [Fact]
        public void TryCreateWithInvalidPolygonReturnsFalse()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 1), new(0, 1), new(1, 0) }; // Self-intersecting
            var result = Polygon2D.TryCreate(vertices, out var polygon, out var error);
            result.Should().BeFalse();
            polygon.Should().BeNull();
            error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void FromPointsBehavesLikeCreateValidated()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 0), new(1, 1) };
            var polygon = Polygon2D.FromPoints(vertices);
            polygon.Should().NotBeNull();

            Action act = () => Polygon2D.FromPoints(new List<Vec2> { new(0, 0), new(1, 1) });
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SignedAreaWithNullVertsThrowsArgumentNullException()
        {
            Action act = () => Polygon2D.SignedArea((IReadOnlyList<Vec2>)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PerimeterCalculatesCorrectly()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
            var polygon = new Polygon2D(vertices);
            polygon.Perimeter().Should().BeApproximately(4.0, 1e-9);
        }

        [Fact]
        public void IsRectangleAxisAlignedWithNonRectangleReturnsFalse()
        {
            var vertices = new List<Vec2> { new(0, 0), new(1, 0), new(1, 1) };
            var polygon = new Polygon2D(vertices);
            polygon.IsRectangleAxisAligned(out _, out _).Should().BeFalse();
        }

        [Fact]
        public void IsRectangleAxisAlignedWithRotatedRectangleReturnsFalse()
        {
            var vertices = new List<Vec2> { new(1, 0), new(2, 1), new(1, 2), new(0, 1) };
            var polygon = new Polygon2D(vertices);
            polygon.IsRectangleAxisAligned(out _, out _).Should().BeFalse();
        }

        [Fact]
        public void IsRectangleAxisAlignedWithMisalignedCornersReturnsFalse()
        {
            var vertices = new List<Vec2> { new(0, 0), new(2, 0), new(2, 1), new(0.1, 1) };
            var polygon = new Polygon2D(vertices);
            polygon.IsRectangleAxisAligned(out _, out _).Should().BeFalse();
        }
    }
}
