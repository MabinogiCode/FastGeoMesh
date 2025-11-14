using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using Xunit;

namespace FastGeoMesh.Tests.Services
{
    public class ProximityCheckerTests
    {
        private readonly ProximityChecker _checker;
        private readonly GeometryService _geometryService;

        public ProximityCheckerTests()
        {
            _checker = new ProximityChecker();
            _geometryService = new GeometryService();
        }

        [Fact]
        public void IsNearAnyHole_ReturnsTrueWhenPointIsNearHoleBoundary()
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
                topElevation: 10.0,
                holes: new[] { hole }
            );

            // Act - point near left edge of hole
            var result = _checker.IsNearAnyHole(structure, 1.9, 3.0, band: 0.2, _geometryService);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNearAnyHole_ReturnsFalseWhenPointIsFarFromHoles()
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
                topElevation: 10.0,
                holes: new[] { hole }
            );

            // Act - point far from hole
            var result = _checker.IsNearAnyHole(structure, 8.0, 8.0, band: 0.2, _geometryService);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNearAnyHole_ReturnsFalseWhenNoHoles()
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

        [Fact]
        public void IsNearAnySegment_ReturnsTrueWhenPointIsNearSegment()
        {
            // Arrange
            var segment = new Segment3D(
                new Vec3(2, 2, 0),
                new Vec3(8, 2, 10)
            );

            var geometry = new AdditionalGeometry(
                points: Array.Empty<Vec3>(),
                segments: new[] { segment }
            );

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0,
                geometry: geometry
            );

            // Act - point near the segment
            var result = _checker.IsNearAnySegment(structure, 5.0, 2.1, band: 0.2, _geometryService);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNearAnySegment_ReturnsFalseWhenPointIsFarFromSegments()
        {
            // Arrange
            var segment = new Segment3D(
                new Vec3(2, 2, 0),
                new Vec3(8, 2, 10)
            );

            var geometry = new AdditionalGeometry(
                points: Array.Empty<Vec3>(),
                segments: new[] { segment }
            );

            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0,
                geometry: geometry
            );

            // Act - point far from segment
            var result = _checker.IsNearAnySegment(structure, 5.0, 8.0, band: 0.2, _geometryService);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInsideAnyHole_ReturnsTrueWhenPointIsInsideHole()
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
                topElevation: 10.0,
                holes: new[] { hole }
            );

            // Act - point inside hole
            var result = _checker.IsInsideAnyHole(structure, 3.0, 3.0, _geometryService);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInsideAnyHole_ReturnsFalseWhenPointIsOutsideAllHoles()
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
                topElevation: 10.0,
                holes: new[] { hole }
            );

            // Act - point outside hole
            var result = _checker.IsInsideAnyHole(structure, 8.0, 8.0, _geometryService);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInsideAnyHole_ReturnsFalseWhenNoHoles()
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
