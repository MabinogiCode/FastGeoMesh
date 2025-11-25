using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using Xunit;

namespace FastGeoMesh.Tests.Helpers
{
    public class MeshValidationHelperTests
    {
        [Fact]
        public void ValidatePolygonReturnsTrueForValidSquare()
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
        public void ValidatePolygonReturnsTrueForValidTriangle()
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
        public void ValidatePolygonReturnsFalseForNullPolygon()
        {
            // Arrange
            Polygon2D? polygon = null;

            // Act
            var result = MeshValidationHelper.ValidatePolygon(polygon!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePolygonReturnsFalseForLessThan3Vertices()
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
        public void ValidatePolygonReturnsFalseForDuplicateConsecutiveVertices()
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
        public void ValidatePolygonReturnsFalseForSelfIntersectingPolygon()
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
        public void ValidatePolygonReturnsTrueForComplexValidPolygon()
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
        public void ValidatePolygonReturnsTrueForConvexHexagon()
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
        public void ValidatePolygonReturnsFalseForNearlyDuplicateVertices()
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
