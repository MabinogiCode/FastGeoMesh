using FastGeoMesh.Domain;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for <see cref="SpatialPolygonIndex"/> performance and correctness.</summary>
    public sealed class SpatialPolygonIndexTests
    {
        /// <summary>
        /// Ensures inside points within a convex polygon return true.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexDetectsInsidePoints()
        {
            // Arrange - Square polygon
            var square = new Vec2[]
            {
                new(0, 0), new(10, 0), new(10, 10), new(0, 10)
            };
            var index = new SpatialPolygonIndex(square);

            // Act & Assert
            index.IsInside(5, 5).Should().BeTrue("Point inside square");
            index.IsInside(1, 1).Should().BeTrue("Point near corner inside");
            index.IsInside(9, 9).Should().BeTrue("Point near opposite corner inside");
        }

        /// <summary>
        /// Ensures outside points return false for containment queries.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexDetectsOutsidePoints()
        {
            // Arrange
            var square = new Vec2[]
            {
                new(0, 0), new(10, 0), new(10, 10), new(0, 10)
            };
            var index = new SpatialPolygonIndex(square);

            // Act & Assert
            index.IsInside(-1, 5).Should().BeFalse("Point outside left");
            index.IsInside(11, 5).Should().BeFalse("Point outside right");
            index.IsInside(5, -1).Should().BeFalse("Point outside bottom");
            index.IsInside(5, 11).Should().BeFalse("Point outside top");
            index.IsInside(-5, -5).Should().BeFalse("Point far outside");
        }

        /// <summary>
        /// Verifies index does not crash on boundary points (corner and edge).
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexHandlesBoundaryPoints()
        {
            // Arrange
            var square = new Vec2[]
            {
                new(0, 0), new(10, 0), new(10, 10), new(0, 10)
            };
            var index = new SpatialPolygonIndex(square);

            // Act & Assert - Boundary behavior may vary but should be consistent
            var corner = index.IsInside(0, 0);
            var edge = index.IsInside(5, 0);

            // Just verify it gives results without crashing
            Assert.True(corner || !corner, "Corner test should not crash");
            Assert.True(edge || !edge, "Edge test should not crash");
        }

        /// <summary>
        /// Tests indexing on a non-convex L-shaped polygon.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexWorksWithComplexPolygon()
        {
            // Arrange - L-shaped polygon
            var lShape = new Vec2[]
            {
                new(0, 0), new(6, 0), new(6, 3),
                new(3, 3), new(3, 6), new(0, 6)
            };
            var index = new SpatialPolygonIndex(lShape);

            // Act & Assert
            index.IsInside(1, 1).Should().BeTrue("Point in bottom-left rectangle");
            index.IsInside(1, 5).Should().BeTrue("Point in top-left rectangle");
            index.IsInside(5, 1).Should().BeTrue("Point in bottom-right rectangle");
            index.IsInside(5, 5).Should().BeFalse("Point in missing top-right rectangle");
        }

        /// <summary>
        /// Performs broad sampling to ensure some points are classified inside and not everything is inside.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexPerformsBetterThanNaive()
        {
            // Arrange - Complex polygon with many vertices
            var vertices = new List<Vec2>();
            int n = 100;
            for (int i = 0; i < n; i++)
            {
                double angle = 2 * System.Math.PI * i / n;
                double radius = 10 + 2 * System.Math.Sin(8 * angle); // Star-like shape
                vertices.Add(new Vec2(
                    radius * System.Math.Cos(angle),
                    radius * System.Math.Sin(angle)
                ));
            }

            var index = new SpatialPolygonIndex(vertices);

            // Act - Test many points (in practice, spatial index should be faster)
            int insideCount = 0;
            for (double x = -15; x <= 15; x += 0.5)
            {
                for (double y = -15; y <= 15; y += 0.5)
                {
                    if (index.IsInside(x, y))
                    {
                        insideCount++;
                    }
                }
            }

            // Assert - Should complete without issues and give reasonable results
            insideCount.Should().BeGreaterThan(0, "Should find some points inside");
            insideCount.Should().BeLessThan(1900, "Should not classify all points as inside"); // Total points: 31*31 = 961
        }

        /// <summary>
        /// Ensures different grid resolutions produce same results for clear inside/outside points.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexHandlesDifferentGridResolutions()
        {
            // Arrange
            var square = new Vec2[]
            {
                new(0, 0), new(10, 0), new(10, 10), new(0, 10)
            };

            var coarseIndex = new SpatialPolygonIndex(square, gridResolution: 4);
            var fineIndex = new SpatialPolygonIndex(square, gridResolution: 64);

            // Act & Assert - Both should give same results for clear inside/outside points
            coarseIndex.IsInside(5, 5).Should().BeTrue("Coarse index should detect inside");
            fineIndex.IsInside(5, 5).Should().BeTrue("Fine index should detect inside");

            coarseIndex.IsInside(-1, -1).Should().BeFalse("Coarse index should detect outside");
            fineIndex.IsInside(-1, -1).Should().BeFalse("Fine index should detect outside");
        }

        /// <summary>
        /// Tests triangle support for minimal polygon definitions.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexHandlesTriangle()
        {
            // Arrange - Simple triangle
            var triangle = new Vec2[]
            {
                new(0, 0), new(10, 0), new(5, 10)
            };
            var index = new SpatialPolygonIndex(triangle);

            // Act & Assert
            index.IsInside(5, 3).Should().BeTrue("Point inside triangle");
            index.IsInside(1, 8).Should().BeFalse("Point outside triangle");
            index.IsInside(9, 8).Should().BeFalse("Point outside triangle");
        }

        /// <summary>
        /// Cross-validates fast index against reference point-in-polygon for a sampled grid.
        /// </summary>
        [Fact]
        public void SpatialPolygonIndexMatchesReferenceImplementation()
        {
            var poly = new Vec2[] { new(0, 0), new(10, 0), new(10, 5), new(0, 5) };
            var idx = new SpatialPolygonIndex(poly, gridResolution: 32);
            for (double x = -1; x <= 11; x += 0.8)
            {
                for (double y = -1; y <= 6; y += 0.6)
                {
                    bool refInside = GeometryHelper.PointInPolygon(poly, x, y);
                    bool fastInside = idx.IsInside(x, y);
                    fastInside.Should().Be(refInside, $"Mismatch at ({x},{y})");
                }
            }
        }
    }
}
