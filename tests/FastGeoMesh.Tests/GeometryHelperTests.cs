using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for <see cref="GeometryHelper"/> utility functions.</summary>
    public sealed class GeometryHelperTests
    {
        /// <summary>
        /// Verifies distance from a point to a segment for points on the segment, perpendicular, and beyond endpoints.
        /// </summary>
        [Fact]
        public void DistancePointToSegmentReturnsCorrectValues()
        {
            // Arrange
            var a = new Vec2(0, 0);
            var b = new Vec2(10, 0);

            // Act & Assert - Point on segment
            var pointOnSegment = new Vec2(5, 0);
            GeometryHelper.DistancePointToSegment(pointOnSegment, a, b).Should().BeApproximately(0.0, 1e-9);

            // Point perpendicular to segment
            var pointAbove = new Vec2(5, 3);
            GeometryHelper.DistancePointToSegment(pointAbove, a, b).Should().BeApproximately(3.0, 1e-9);

            // Point before segment start
            var pointBefore = new Vec2(-2, 4);
            var expectedBefore = Math.Sqrt(4 + 16); // distance to (0,0)
            GeometryHelper.DistancePointToSegment(pointBefore, a, b).Should().BeApproximately(expectedBefore, 1e-9);

            // Point after segment end
            var pointAfter = new Vec2(12, 5);
            var expectedAfter = Math.Sqrt(4 + 25); // distance to (10,0)
            GeometryHelper.DistancePointToSegment(pointAfter, a, b).Should().BeApproximately(expectedAfter, 1e-9);
        }

        /// <summary>
        /// Ensures point-in-polygon detects interior, edge, corner, and outside points for a convex square.
        /// </summary>
        [Fact]
        public void PointInPolygonDetectsInsideAndOutside()
        {
            // Arrange - Square
            var square = new Vec2[]
            {
                new(0, 0), new(10, 0), new(10, 10), new(0, 10)
            };

            // Act & Assert
            GeometryHelper.PointInPolygon(square, 5, 5).Should().BeTrue("Point inside square");
            GeometryHelper.PointInPolygon(square, 0, 0).Should().BeTrue("Point on corner");
            GeometryHelper.PointInPolygon(square, 5, 0).Should().BeTrue("Point on edge");
            GeometryHelper.PointInPolygon(square, -1, 5).Should().BeFalse("Point outside left");
            GeometryHelper.PointInPolygon(square, 11, 5).Should().BeFalse("Point outside right");
            GeometryHelper.PointInPolygon(square, 5, -1).Should().BeFalse("Point outside bottom");
            GeometryHelper.PointInPolygon(square, 5, 11).Should().BeFalse("Point outside top");
        }

        /// <summary>
        /// Validates convexity detection for convex and concave (bow-tie) quads.
        /// </summary>
        [Fact]
        public void IsConvexDetectsConvexQuads()
        {
            // Arrange - Convex square
            var convexSquare = (
                new Vec2(0, 0), new Vec2(10, 0),
                new Vec2(10, 10), new Vec2(0, 10)
            );

            // Concave (bow-tie) quad
            var concaveQuad = (
                new Vec2(0, 0), new Vec2(10, 10),
                new Vec2(10, 0), new Vec2(0, 10)
            );

            // Act & Assert
            GeometryHelper.IsConvex(convexSquare).Should().BeTrue("Square should be convex");
            GeometryHelper.IsConvex(concaveQuad).Should().BeFalse("Bow-tie should not be convex");
        }

        /// <summary>
        /// Tests linear interpolation between two 2D points at multiple factors.
        /// </summary>
        [Fact]
        public void LerpInterpolatesCorrectly()
        {
            // Arrange
            var a = new Vec2(0, 0);
            var b = new Vec2(10, 20);

            // Act & Assert
            var start = GeometryHelper.Lerp(a, b, 0.0);
            start.Should().Be(a);

            var end = GeometryHelper.Lerp(a, b, 1.0);
            end.Should().Be(b);

            var middle = GeometryHelper.Lerp(a, b, 0.5);
            middle.Should().Be(new Vec2(5, 10));

            var quarter = GeometryHelper.Lerp(a, b, 0.25);
            quarter.Should().Be(new Vec2(2.5, 5));
        }

        /// <summary>
        /// Tests scalar interpolation for several interpolation factors including extremes.
        /// </summary>
        [Fact]
        public void LerpScalarInterpolatesCorrectly()
        {
            // Act & Assert
            GeometryHelper.LerpScalar(0, 10, 0.0).Should().Be(0);
            GeometryHelper.LerpScalar(0, 10, 1.0).Should().Be(10);
            GeometryHelper.LerpScalar(0, 10, 0.5).Should().Be(5);
            GeometryHelper.LerpScalar(5, 15, 0.25).Should().Be(7.5);
            GeometryHelper.LerpScalar(-10, 10, 0.75).Should().Be(5);
        }

        /// <summary>
        /// Edge-case driven point-in-polygon tests ensure boundary and outside conditions behave correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0, true)]    // Corner
        [InlineData(5, 0, true)]    // Edge
        [InlineData(5, 5, true)]    // Inside
        [InlineData(-1, 5, false)]  // Outside left
        [InlineData(11, 5, false)]  // Outside right
        [InlineData(5, -1, false)]  // Outside bottom
        [InlineData(5, 11, false)]  // Outside top
        public void PointInPolygonHandlesEdgeCases(double x, double y, bool expected)
        {
            // Arrange
            var square = new Vec2[]
            {
                new(0, 0), new(10, 0), new(10, 10), new(0, 10)
            };

            // Act & Assert
            GeometryHelper.PointInPolygon(square, x, y).Should().Be(expected);
        }
    }
}
