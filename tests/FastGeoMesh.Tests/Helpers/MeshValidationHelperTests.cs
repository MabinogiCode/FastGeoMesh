using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using Xunit;

namespace FastGeoMesh.Tests.Helpers
{
    public class MeshValidationHelperTests
    {
        [Fact]
        public void ValidatePolygon_ReturnsTrue_ForValidSquare()
        {
            // Arrange
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(10, 10),
                new Vec2(0, 10)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsTrue_ForValidTriangle()
        {
            // Arrange
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(5, 10)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsFalse_ForNullPolygon()
        {
            // Arrange
            Polygon2D? polygon = null;

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsFalse_ForLessThan3Vertices()
        {
            // Arrange
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsFalse_ForDuplicateConsecutiveVertices()
        {
            // Arrange
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(10, 0), // Duplicate
                new Vec2(10, 10),
                new Vec2(0, 10)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsFalse_ForSelfIntersectingPolygon()
        {
            // Arrange - Bowtie/figure-eight shape
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 10),
                new Vec2(10, 0),
                new Vec2(0, 10) // Creates intersection
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsTrue_ForComplexValidPolygon()
        {
            // Arrange - L-shape
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(10, 5),
                new Vec2(5, 5),
                new Vec2(5, 10),
                new Vec2(0, 10)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsTrue_ForConvexHexagon()
        {
            // Arrange - Regular hexagon
            var polygon = new Polygon2D(new[]
            {
                new Vec2(1, 0),
                new Vec2(2, 0),
                new Vec2(3, 1),
                new Vec2(2, 2),
                new Vec2(1, 2),
                new Vec2(0, 1)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidatePolygon_ReturnsFalse_ForNearlyDuplicateVertices()
        {
            // Arrange - Vertices very close to each other (within tolerance)
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(10, 1e-11), // Essentially same as previous vertex
                new Vec2(10, 10),
                new Vec2(0, 10)
            });

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon);

            // Assert
            Assert.False(result);
        }
    }
}
