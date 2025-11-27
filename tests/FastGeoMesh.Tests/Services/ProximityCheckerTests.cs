using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using Xunit;

namespace FastGeoMesh.Tests.Services
{
    /// <summary>
    /// Tests for class ProximityCheckerTests.
    /// </summary>
    public class ProximityCheckerTests
    {
        private readonly ProximityChecker _checker;
        private readonly GeometryService _geometryService;
        /// <summary>
        /// Runs test ProximityCheckerTests.
        /// </summary>
        public ProximityCheckerTests()
        {
            _checker = new ProximityChecker();
            _geometryService = new GeometryService();
        }
        /// <summary>
        /// Runs test IsNearAnyHoleReturnsTrueWhenPointIsNearHoleBoundary.
        /// </summary>
        [Fact]
        public void IsNearAnyHoleReturnsTrueWhenPointIsNearHoleBoundary()
        {
            // Arrange
            var hole = new Polygon2D(new[]
            {
                new Vec2(2, 2),
                new Vec2(4, 2),
                new Vec2(4, 4),
                new Vec2(2, 4)
            });

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            ).AddHole(hole);

            // Act - point near left edge of hole
            var result = _checker.IsNearAnyHole(structure, 1.9, 3.0, band: 0.2, _geometryService);

            // Assert
            Assert.True(result);
        }
        /// <summary>
        /// Runs test IsNearAnyHoleReturnsFalseWhenPointIsFarFromHoles.
        /// </summary>
        [Fact]
        public void IsNearAnyHoleReturnsFalseWhenPointIsFarFromHoles()
        {
            // Arrange
            var hole = new Polygon2D(new[]
            {
                new Vec2(2, 2),
                new Vec2(4, 2),
                new Vec2(4, 4),
                new Vec2(2, 4)
            });

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            ).AddHole(hole);

            // Act - point far from hole
            var result = _checker.IsNearAnyHole(structure, 8.0, 8.0, band: 0.2, _geometryService);

            // Assert
            Assert.False(result);
        }
        /// <summary>
        /// Runs test IsNearAnyHoleReturnsFalseWhenNoHoles.
        /// </summary>
        [Fact]
        public void IsNearAnyHoleReturnsFalseWhenNoHoles()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );

            // Act
            var result = _checker.IsNearAnyHole(structure, 5.0, 5.0, band: 0.2, _geometryService);

            // Assert
            Assert.False(result);
        }
        /// <summary>
        /// Runs test IsNearAnySegmentReturnsTrueWhenPointIsNearSegment.
        /// </summary>
        [Fact]
        public void IsNearAnySegmentReturnsTrueWhenPointIsNearSegment()
        {
            // Arrange
            var segment = new Segment3D(
                new Vec3(2, 2, 0),
                new Vec3(8, 2, 10)
            );

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );
            structure.Geometry.AddSegment(segment);

            // Act - point near the segment
            var result = _checker.IsNearAnySegment(structure, 5.0, 2.1, band: 0.2, _geometryService);

            // Assert
            Assert.True(result);
        }
        /// <summary>
        /// Runs test IsNearAnySegmentReturnsFalseWhenPointIsFarFromSegments.
        /// </summary>
        [Fact]
        public void IsNearAnySegmentReturnsFalseWhenPointIsFarFromSegments()
        {
            // Arrange
            var segment = new Segment3D(
                new Vec3(2, 2, 0),
                new Vec3(8, 2, 10)
            );

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );
            structure.Geometry.AddSegment(segment);

            // Act - point far from segment
            var result = _checker.IsNearAnySegment(structure, 5.0, 8.0, band: 0.2, _geometryService);

            // Assert
            Assert.False(result);
        }
        /// <summary>
        /// Runs test IsInsideAnyHoleReturnsTrueWhenPointIsInsideHole.
        /// </summary>
        [Fact]
        public void IsInsideAnyHoleReturnsTrueWhenPointIsInsideHole()
        {
            // Arrange
            var hole = new Polygon2D(new[]
            {
                new Vec2(2, 2),
                new Vec2(4, 2),
                new Vec2(4, 4),
                new Vec2(2, 4)
            });

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            ).AddHole(hole);

            // Act - point inside hole
            var result = _checker.IsInsideAnyHole(structure, 3.0, 3.0, _geometryService);

            // Assert
            Assert.True(result);
        }
        /// <summary>
        /// Runs test IsInsideAnyHoleReturnsFalseWhenPointIsOutsideAllHoles.
        /// </summary>
        [Fact]
        public void IsInsideAnyHoleReturnsFalseWhenPointIsOutsideAllHoles()
        {
            // Arrange
            var hole = new Polygon2D(new[]
            {
                new Vec2(2, 2),
                new Vec2(4, 2),
                new Vec2(4, 4),
                new Vec2(2, 4)
            });

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            ).AddHole(hole);

            // Act - point outside hole
            var result = _checker.IsInsideAnyHole(structure, 8.0, 8.0, _geometryService);

            // Assert
            Assert.False(result);
        }
        /// <summary>
        /// Runs test IsInsideAnyHoleReturnsFalseWhenNoHoles.
        /// </summary>
        [Fact]
        public void IsInsideAnyHoleReturnsFalseWhenNoHoles()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );

            // Act
            var result = _checker.IsInsideAnyHole(structure, 5.0, 5.0, _geometryService);

            // Assert
            Assert.False(result);
        }
    }
}
