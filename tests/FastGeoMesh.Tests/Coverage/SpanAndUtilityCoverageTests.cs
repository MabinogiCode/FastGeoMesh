using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for span extensions and advanced geometric operations to improve coverage.
    /// Focuses on AdvancedSpanExtensions, SpanExtensions, and other utility classes.
    /// </summary>
    public sealed class SpanAndUtilityCoverageTests
    {
        /// <summary>Tests AdvancedSpanExtensions.ComputeSignedArea method.</summary>
        [Fact]
        public void AdvancedSpanExtensionsComputeSignedAreaWorksCorrectly()
        {
            // Test with empty span
            ReadOnlySpan<Vec2> empty = Array.Empty<Vec2>();
            var emptyArea = empty.ComputeSignedArea();
            emptyArea.Should().Be(0.0);

            // Test with insufficient vertices
            var twoPoints = new Vec2[] { new Vec2(0, 0), new Vec2(1, 0) };
            ReadOnlySpan<Vec2> twoPointsSpan = twoPoints;
            var twoPointArea = twoPointsSpan.ComputeSignedArea();
            twoPointArea.Should().Be(0.0);

            // Test with square (CCW order - positive area)
            var squareCCW = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(1, 0),
                new Vec2(1, 1), new Vec2(0, 1)
            };
            ReadOnlySpan<Vec2> squareCCWSpan = squareCCW;
            var squareAreaCCW = squareCCWSpan.ComputeSignedArea();
            squareAreaCCW.Should().Be(1.0);

            // Test with square (CW order - negative area)
            var squareCW = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(0, 1),
                new Vec2(1, 1), new Vec2(1, 0)
            };
            ReadOnlySpan<Vec2> squareCWSpan = squareCW;
            var squareAreaCW = squareCWSpan.ComputeSignedArea();
            squareAreaCW.Should().Be(-1.0);

            // Test with triangle
            var triangle = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(2, 0), new Vec2(1, 2)
            };
            ReadOnlySpan<Vec2> triangleSpan = triangle;
            var triangleArea = triangleSpan.ComputeSignedArea();
            triangleArea.Should().Be(2.0); // Area = 0.5 * base * height = 0.5 * 2 * 2 = 2.0

            // Test with odd number of vertices (pentagon)
            var pentagon = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(2, 0), new Vec2(3, 1),
                new Vec2(1, 3), new Vec2(-1, 1)
            };
            ReadOnlySpan<Vec2> pentagonSpan = pentagon;
            var pentagonArea = pentagonSpan.ComputeSignedArea();
            pentagonArea.Should().BeGreaterThan(0); // Should be positive for CCW
        }

        /// <summary>Tests AdvancedSpanExtensions.ContainsPoint method.</summary>
        [Fact]
        public void AdvancedSpanExtensionsContainsPointWorksCorrectly()
        {
            // Test with empty polygon
            ReadOnlySpan<Vec2> empty = Array.Empty<Vec2>();
            empty.ContainsPoint(new Vec2(1, 1)).Should().BeFalse();

            // Test with insufficient vertices
            var line = new Vec2[] { new Vec2(0, 0), new Vec2(1, 0) };
            ReadOnlySpan<Vec2> lineSpan = line;
            lineSpan.ContainsPoint(new Vec2(0.5, 0)).Should().BeFalse();

            // Test with square
            var square = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(2, 0),
                new Vec2(2, 2), new Vec2(0, 2)
            };
            ReadOnlySpan<Vec2> squareSpan = square;

            // Points inside
            squareSpan.ContainsPoint(new Vec2(1, 1)).Should().BeTrue();
            squareSpan.ContainsPoint(new Vec2(0.5, 0.5)).Should().BeTrue();
            squareSpan.ContainsPoint(new Vec2(1.5, 1.5)).Should().BeTrue();

            // Points outside
            squareSpan.ContainsPoint(new Vec2(-1, 1)).Should().BeFalse();
            squareSpan.ContainsPoint(new Vec2(3, 1)).Should().BeFalse();
            squareSpan.ContainsPoint(new Vec2(1, -1)).Should().BeFalse();
            squareSpan.ContainsPoint(new Vec2(1, 3)).Should().BeFalse();

            // Test with triangle
            var triangle = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(4, 0), new Vec2(2, 3)
            };
            ReadOnlySpan<Vec2> triangleSpan = triangle;

            triangleSpan.ContainsPoint(new Vec2(2, 1)).Should().BeTrue();
            triangleSpan.ContainsPoint(new Vec2(1, 1)).Should().BeTrue();
            triangleSpan.ContainsPoint(new Vec2(3, 1)).Should().BeTrue();
            triangleSpan.ContainsPoint(new Vec2(2, 2.5)).Should().BeTrue();

            triangleSpan.ContainsPoint(new Vec2(-1, 1)).Should().BeFalse();
            triangleSpan.ContainsPoint(new Vec2(5, 1)).Should().BeFalse();
            triangleSpan.ContainsPoint(new Vec2(2, 4)).Should().BeFalse();
        }

        /// <summary>Tests AdvancedSpanExtensions.ContainsPoints batch method.</summary>
        [Fact]
        public void AdvancedSpanExtensionsContainsPointsBatchWorksCorrectly()
        {
            var square = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(2, 0),
                new Vec2(2, 2), new Vec2(0, 2)
            };
            ReadOnlySpan<Vec2> squareSpan = square;

            var testPoints = new Vec2[]
            {
                new Vec2(1, 1),     // inside
                new Vec2(0.5, 0.5), // inside
                new Vec2(-1, 1),    // outside
                new Vec2(3, 1),     // outside
                new Vec2(1, 3)      // outside
            };
            ReadOnlySpan<Vec2> testPointsSpan = testPoints;

            var results = new bool[testPoints.Length];

            // Test batch operation
            squareSpan.ContainsPoints(testPointsSpan, results);

            results[0].Should().BeTrue();  // inside
            results[1].Should().BeTrue();  // inside
            results[2].Should().BeFalse(); // outside
            results[3].Should().BeFalse(); // outside
            results[4].Should().BeFalse(); // outside

            // Test with empty polygon
            ReadOnlySpan<Vec2> empty = Array.Empty<Vec2>();
            var emptyResults = new bool[testPoints.Length];
            empty.ContainsPoints(testPointsSpan, emptyResults);

            foreach (var result in emptyResults)
            {
                result.Should().BeFalse();
            }

            // Test error case: results span too small - avoid using spans in lambda
            var smallResults = new bool[testPoints.Length - 1];
            try
            {
                squareSpan.ContainsPoints(testPointsSpan, smallResults);
                Assert.Fail("Expected ArgumentException was not thrown");
            }
            catch (ArgumentException)
            {
                // Expected exception
            }
        }

        /// <summary>Tests AdvancedSpanExtensions.ComputePaddedBounds method.</summary>
        [Fact]
        public void AdvancedSpanExtensionsComputePaddedBoundsWorksCorrectly()
        {
            // Test with empty span
            ReadOnlySpan<Vec2> empty = Array.Empty<Vec2>();
            var (emptyMin, emptyMax) = empty.ComputePaddedBounds();
            emptyMin.Should().Be(Vec2.Zero);
            emptyMax.Should().Be(Vec2.Zero);

            // Test with single point
            var singlePoint = new Vec2[] { new Vec2(5, 3) };
            ReadOnlySpan<Vec2> singlePointSpan = singlePoint;
            var (singleMin, singleMax) = singlePointSpan.ComputePaddedBounds();
            singleMin.Should().Be(new Vec2(5, 3));
            singleMax.Should().Be(new Vec2(5, 3));

            // Test with multiple points
            var points = new Vec2[]
            {
                new Vec2(1, 2), new Vec2(5, 1), new Vec2(3, 6),
                new Vec2(0, 4), new Vec2(7, 3), new Vec2(2, 0)
            };
            ReadOnlySpan<Vec2> pointsSpan = points;

            var (min, max) = pointsSpan.ComputePaddedBounds();
            min.Should().Be(new Vec2(0, 0)); // minimum X=0, Y=0
            max.Should().Be(new Vec2(7, 6)); // maximum X=7, Y=6

            // Test with padding
            var (paddedMin, paddedMax) = pointsSpan.ComputePaddedBounds(1.0);
            paddedMin.Should().Be(new Vec2(-1, -1)); // min - padding
            paddedMax.Should().Be(new Vec2(8, 7));   // max + padding

            // Test unrolled loop path with exactly 4 points
            var fourPoints = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(2, 1),
                new Vec2(1, 3), new Vec2(3, 2)
            };
            ReadOnlySpan<Vec2> fourPointsSpan = fourPoints;
            var (fourMin, fourMax) = fourPointsSpan.ComputePaddedBounds();
            fourMin.Should().Be(new Vec2(0, 0));
            fourMax.Should().Be(new Vec2(3, 3));

            // Test with many points to exercise unrolled loop
            var manyPoints = new Vec2[10];
            for (int i = 0; i < 10; i++)
            {
                manyPoints[i] = new Vec2(i, i * 0.5);
            }
            ReadOnlySpan<Vec2> manyPointsSpan = manyPoints;
            var (manyMin, manyMax) = manyPointsSpan.ComputePaddedBounds();
            manyMin.Should().Be(new Vec2(0, 0));
            manyMax.Should().Be(new Vec2(9, 4.5));
        }

        /// <summary>Tests AdvancedSpanExtensions.DistanceToSegment method.</summary>
        [Fact]
        public void AdvancedSpanExtensionsDistanceToSegmentWorksCorrectly()
        {
            // Test distance to horizontal segment
            var start = new Vec2(0, 0);
            var end = new Vec2(4, 0);

            // Point directly above middle of segment
            var distance1 = AdvancedSpanExtensions.DistanceToSegment(new Vec2(2, 3), start, end);
            distance1.Should().Be(3.0);

            // Point to the left of segment start
            var distance2 = AdvancedSpanExtensions.DistanceToSegment(new Vec2(-2, 0), start, end);
            distance2.Should().Be(2.0);

            // Point to the right of segment end
            var distance3 = AdvancedSpanExtensions.DistanceToSegment(new Vec2(6, 0), start, end);
            distance3.Should().Be(2.0);

            // Point on the segment
            var distance4 = AdvancedSpanExtensions.DistanceToSegment(new Vec2(2, 0), start, end);
            distance4.Should().BeApproximately(0.0, 1e-10);

            // Test with degenerate segment (point)
            var point = new Vec2(5, 5);
            var distanceToPoint = AdvancedSpanExtensions.DistanceToSegment(new Vec2(8, 9), point, point);
            distanceToPoint.Should().Be(5.0); // distance from (8,9) to (5,5) = sqrt(3^2 + 4^2) = 5

            // Test with vertical segment
            var vertStart = new Vec2(0, 0);
            var vertEnd = new Vec2(0, 4);
            var distanceVert = AdvancedSpanExtensions.DistanceToSegment(new Vec2(3, 2), vertStart, vertEnd);
            distanceVert.Should().Be(3.0);
        }

        /// <summary>Tests SpanExtensions.ComputeCentroid methods.</summary>
        [Fact]
        public void SpanExtensionsComputeCentroidWorksCorrectly()
        {
            // Test 2D centroid with empty span
            ReadOnlySpan<Vec2> empty2D = Array.Empty<Vec2>();
            var emptyCentroid2D = empty2D.ComputeCentroid();
            emptyCentroid2D.Should().Be(Vec2.Zero);

            // Test 2D centroid with square
            var square2D = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(2, 0),
                new Vec2(2, 2), new Vec2(0, 2)
            };
            ReadOnlySpan<Vec2> square2DSpan = square2D;
            var squareCentroid2D = square2DSpan.ComputeCentroid();
            squareCentroid2D.Should().Be(new Vec2(1, 1));

            // Test 2D centroid with triangle
            var triangle2D = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(3, 0), new Vec2(1.5, 3)
            };
            ReadOnlySpan<Vec2> triangle2DSpan = triangle2D;
            var triangleCentroid2D = triangle2DSpan.ComputeCentroid();
            triangleCentroid2D.Should().Be(new Vec2(1.5, 1.0));

            // Test 3D centroid with empty span
            ReadOnlySpan<Vec3> empty3D = Array.Empty<Vec3>();
            var emptyCentroid3D = empty3D.ComputeCentroid();
            emptyCentroid3D.Should().Be(Vec3.Zero);

            // Test 3D centroid with cube corners
            var cube3D = new Vec3[]
            {
                new Vec3(0, 0, 0), new Vec3(2, 0, 0),
                new Vec3(2, 2, 0), new Vec3(0, 2, 0),
                new Vec3(0, 0, 2), new Vec3(2, 0, 2),
                new Vec3(2, 2, 2), new Vec3(0, 2, 2)
            };
            ReadOnlySpan<Vec3> cube3DSpan = cube3D;
            var cubeCentroid3D = cube3DSpan.ComputeCentroid();
            cubeCentroid3D.Should().Be(new Vec3(1, 1, 1));
        }

        /// <summary>Tests SpanExtensions.TransformTo3D method.</summary>
        [Fact]
        public void SpanExtensionsTransformTo3DWorksCorrectly()
        {
            var source2D = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(1, 2), new Vec2(3, 4)
            };
            ReadOnlySpan<Vec2> source2DSpan = source2D;

            var destination3D = new Vec3[source2D.Length];
            const double z = 5.0;

            // Test transformation
            source2DSpan.TransformTo3D(destination3D, z);

            destination3D[0].Should().Be(new Vec3(0, 0, 5));
            destination3D[1].Should().Be(new Vec3(1, 2, 5));
            destination3D[2].Should().Be(new Vec3(3, 4, 5));

            // Test error case: destination too small - avoid using spans in lambda
            var smallDestination = new Vec3[source2D.Length - 1];
            try
            {
                source2DSpan.TransformTo3D(smallDestination, z);
                Assert.Fail("Expected ArgumentException was not thrown");
            }
            catch (ArgumentException)
            {
                // Expected exception
            }

            // Test with empty spans
            ReadOnlySpan<Vec2> emptySource = Array.Empty<Vec2>();
            var emptyDestination = new Vec3[0];
            emptySource.TransformTo3D(emptyDestination, 1.0); // Should not throw
        }

        /// <summary>Tests SpanExtensions.ComputeBounds method.</summary>
        [Fact]
        public void SpanExtensionsComputeBoundsWorksCorrectly()
        {
            // Test with empty span
            ReadOnlySpan<Vec2> empty = Array.Empty<Vec2>();
            var (emptyMin, emptyMax) = empty.ComputeBounds();
            emptyMin.Should().Be(Vec2.Zero);
            emptyMax.Should().Be(Vec2.Zero);

            // Test with single point
            var singlePoint = new Vec2[] { new Vec2(3, 7) };
            ReadOnlySpan<Vec2> singlePointSpan = singlePoint;
            var (singleMin, singleMax) = singlePointSpan.ComputeBounds();
            singleMin.Should().Be(new Vec2(3, 7));
            singleMax.Should().Be(new Vec2(3, 7));

            // Test with multiple points
            var points = new Vec2[]
            {
                new Vec2(2, 1), new Vec2(-1, 5), new Vec2(4, -2),
                new Vec2(0, 3), new Vec2(6, 0)
            };
            ReadOnlySpan<Vec2> pointsSpan = points;

            var (min, max) = pointsSpan.ComputeBounds();
            min.Should().Be(new Vec2(-1, -2)); // minimum X=-1, Y=-2
            max.Should().Be(new Vec2(6, 5));   // maximum X=6, Y=5

            // Test with identical points
            var identical = new Vec2[]
            {
                new Vec2(2, 3), new Vec2(2, 3), new Vec2(2, 3)
            };
            ReadOnlySpan<Vec2> identicalSpan = identical;
            var (identicalMin, identicalMax) = identicalSpan.ComputeBounds();
            identicalMin.Should().Be(new Vec2(2, 3));
            identicalMax.Should().Be(new Vec2(2, 3));
        }

        /// <summary>Tests integration between different span extension methods.</summary>
        [Fact]
        public void SpanExtensionsIntegrationWorksCorrectly()
        {
            // Create a simple convex polygon (rectangle) to ensure centroid is inside
            var polygon = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(4, 0),
                new Vec2(4, 3), new Vec2(0, 3)
            };
            ReadOnlySpan<Vec2> polygonSpan = polygon;

            // Test multiple operations on the same data
            var area = polygonSpan.ComputeSignedArea();
            var centroid = polygonSpan.ComputeCentroid();
            var (min, max) = polygonSpan.ComputeBounds();
            var containsCenter = polygonSpan.ContainsPoint(centroid);

            // Verify results are reasonable
            area.Should().BeGreaterThan(0); // CCW polygon should have positive area
            centroid.X.Should().BeInRange(min.X, max.X);
            centroid.Y.Should().BeInRange(min.Y, max.Y);
            containsCenter.Should().BeTrue(); // Centroid should be inside for convex polygons

            // Test batch operations
            var testPoints = new Vec2[]
            {
                centroid, // Should be inside
                new Vec2(min.X - 1, min.Y - 1), // Should be outside
                new Vec2(max.X + 1, max.Y + 1), // Should be outside
                new Vec2((min.X + max.X) / 2, (min.Y + max.Y) / 2) // Should be inside (center of rectangle)
            };
            ReadOnlySpan<Vec2> testPointsSpan = testPoints;

            var results = new bool[testPoints.Length];
            polygonSpan.ContainsPoints(testPointsSpan, results);

            results[0].Should().BeTrue();  // centroid
            results[1].Should().BeFalse(); // outside min
            results[2].Should().BeFalse(); // outside max
            results[3].Should().BeTrue();  // center of rectangle should be inside
        }

        /// <summary>Tests edge cases and performance characteristics.</summary>
        [Fact]
        public void SpanExtensionsHandleEdgeCasesCorrectly()
        {
            // Test with very large coordinates
            var largeCoords = new Vec2[]
            {
                new Vec2(1e10, 1e10), new Vec2(1e10 + 1, 1e10),
                new Vec2(1e10 + 1, 1e10 + 1), new Vec2(1e10, 1e10 + 1)
            };
            ReadOnlySpan<Vec2> largeCoordsSpan = largeCoords;

            var largeCentroid = largeCoordsSpan.ComputeCentroid();
            largeCentroid.X.Should().BeApproximately(1e10 + 0.5, 1e6);
            largeCentroid.Y.Should().BeApproximately(1e10 + 0.5, 1e6);

            // Test with very small coordinates
            var smallCoords = new Vec2[]
            {
                new Vec2(1e-10, 1e-10), new Vec2(2e-10, 1e-10),
                new Vec2(2e-10, 2e-10), new Vec2(1e-10, 2e-10)
            };
            ReadOnlySpan<Vec2> smallCoordsSpan = smallCoords;

            var smallArea = smallCoordsSpan.ComputeSignedArea();
            smallArea.Should().BeApproximately(1e-20, 1e-25);

            // Test with many vertices to ensure performance
            var manyVertices = new Vec2[1000];
            for (int i = 0; i < manyVertices.Length; i++)
            {
                double angle = 2 * Math.PI * i / manyVertices.Length;
                manyVertices[i] = new Vec2(Math.Cos(angle), Math.Sin(angle));
            }
            ReadOnlySpan<Vec2> manyVerticesSpan = manyVertices;

            var circleArea = manyVerticesSpan.ComputeSignedArea();
            circleArea.Should().BeApproximately(Math.PI, 0.1); // Approximate circle area

            var circleCentroid = manyVerticesSpan.ComputeCentroid();
            circleCentroid.X.Should().BeApproximately(0, 0.01); // Should be near origin
            circleCentroid.Y.Should().BeApproximately(0, 0.01); // Should be near origin
        }
    }
}
