using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using Xunit;

namespace FastGeoMesh.Tests.Helpers
{
    public class MeshGeometryHelperTests
    {
        [Fact]
        public void ComputeAreaReturnsCorrectAreaForSquare()
        {
            // Arrange - 10x10 square
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(10, 10),
                new Vec2(0, 10)
            });

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            Assert.Equal(100.0, area, precision: 6);
        }

        [Fact]
        public void ComputeAreaReturnsCorrectAreaForRectangle()
        {
            // Arrange - 5x20 rectangle
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(20, 0),
                new Vec2(20, 5),
                new Vec2(0, 5)
            });

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            Assert.Equal(100.0, area, precision: 6);
        }

        [Fact]
        public void ComputeAreaReturnsCorrectAreaForTriangle()
        {
            // Arrange - Right triangle with base=10, height=10
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(0, 10)
            });

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            Assert.Equal(50.0, area, precision: 6);
        }

        [Fact]
        public void ComputeAreaReturnsZeroForNullPolygon()
        {
            // Arrange
            Polygon2D? polygon = null;

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon!);

            // Assert
            Assert.Equal(0.0, area);
        }

        [Fact]
        public void ComputeAreaReturnsZeroForLessThan3Vertices()
        {
            // Arrange
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0)
            });

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            Assert.Equal(0.0, area);
        }

        [Fact]
        public void ComputeAreaReturnsCorrectAreaForLShape()
        {
            // Arrange - L-shape: 10x10 square with 5x5 square removed
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
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            // L-shape area = 10*10 - 5*5 = 75
            Assert.Equal(75.0, area, precision: 6);
        }

        [Fact]
        public void ComputeAreaIsIndependentOfWindingOrder()
        {
            // Arrange - Same square, different winding orders
            var polygonCW = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10, 0),
                new Vec2(10, 10),
                new Vec2(0, 10)
            });

            var polygonCCW = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(0, 10),
                new Vec2(10, 10),
                new Vec2(10, 0)
            });

            // Act
            var areaCW = MeshGeometryHelper.ComputeArea(polygonCW);
            var areaCCW = MeshGeometryHelper.ComputeArea(polygonCCW);

            // Assert - Both should give same positive area
            Assert.Equal(100.0, areaCW, precision: 6);
            Assert.Equal(100.0, areaCCW, precision: 6);
            Assert.Equal(areaCW, areaCCW, precision: 6);
        }

        [Fact]
        public void ComputeAreaReturnsCorrectAreaForPentagon()
        {
            // Arrange - Regular pentagon inscribed in unit circle
            // Using approximation for testing
            var vertices = new[]
            {
                new Vec2(0, 1),
                new Vec2(0.951, 0.309),
                new Vec2(0.588, -0.809),
                new Vec2(-0.588, -0.809),
                new Vec2(-0.951, 0.309)
            };
            var polygon = new Polygon2D(vertices);

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert - Regular pentagon area ≈ 2.378 for unit circle
            Assert.True(area > 2.0 && area < 3.0);
        }

        [Fact]
        public void ComputeAreaHandlesVerySmallPolygons()
        {
            // Arrange - Tiny triangle
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(0.001, 0),
                new Vec2(0, 0.001)
            });

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            Assert.Equal(0.0000005, area, precision: 10);
        }

        [Fact]
        public void ComputeAreaHandlesLargePolygons()
        {
            // Arrange - Very large square
            var polygon = new Polygon2D(new[]
            {
                new Vec2(0, 0),
                new Vec2(10000, 0),
                new Vec2(10000, 10000),
                new Vec2(0, 10000)
            });

            // Act
            var area = MeshGeometryHelper.ComputeArea(polygon);

            // Assert
            Assert.Equal(100000000.0, area, precision: 2);
        }
    }
}
