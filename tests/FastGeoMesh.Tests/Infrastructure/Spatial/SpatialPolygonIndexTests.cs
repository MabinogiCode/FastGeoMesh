using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Infrastructure.Spatial
{
    public class SpatialPolygonIndexTests
    {
        private readonly GeometryService _geometryService = new GeometryService();
        private readonly IReadOnlyList<Vec2> _square;

        public SpatialPolygonIndexTests()
        {
            _square = new List<Vec2> { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        }

        [Fact]
        public void ConstructorWithNullVerticesThrowsArgumentNullException()
        {
            Action act = () => _ = new SpatialPolygonIndex((IReadOnlyList<Vec2>)null!, _geometryService);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConstructorWithNullHelperThrowsArgumentNullException()
        {
            Action act = () => _ = new SpatialPolygonIndex(_square, (GeometryService)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConstructorWithEmptyVerticesThrowsArgumentException()
        {
            Action act = () => _ = new SpatialPolygonIndex(new List<Vec2>(), _geometryService);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConstructorWithInvalidGridResolutionThrowsArgumentOutOfRangeException()
        {
            Action act = () => _ = new SpatialPolygonIndex(_square, _geometryService, 0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void IsInsideWithPointOutsideBoundsReturnsFalse()
        {
            var index = new SpatialPolygonIndex(_square, _geometryService);
            index.IsInside(2, 2).Should().BeFalse();
        }

        [Fact]
        public void IsInsideWithPointOnEdgeReturnsTrue()
        {
            var index = new SpatialPolygonIndex(_square, _geometryService);
            index.IsInside(0.5, 0).Should().BeTrue();
        }

        [Fact]
        public void IsInsideWithPointInsideReturnsTrue()
        {
            var index = new SpatialPolygonIndex(_square, _geometryService);
            index.IsInside(0.5, 0.5).Should().BeTrue();
        }

        [Fact]
        public void BuildIndexWithBoundaryCellMarksAsBoundary()
        {
            // This test is tricky as it depends on private implementation details.
            // We can test its effects indirectly.
            var vertices = new List<Vec2> { new(0.5, 0.5), new(1.5, 0.5), new(1.5, 1.5), new(0.5, 1.5) };
            var index = new SpatialPolygonIndex(vertices, _geometryService, 2);

            // A point known to be in a cell that should be marked as boundary
            index.IsInside(0.6, 0.6).Should().BeTrue();
        }

        [Fact]
        public void PointInPolygonRayCastingWithArrayWorks()
        {
            var vertices = new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
            var index = new SpatialPolygonIndex(vertices, _geometryService);
            index.IsInside(0.5, 0.5).Should().BeTrue();
        }
    }
}
