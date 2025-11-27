using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Utilities;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Utilities;
/// <summary>
/// Tests for class GeometryUtilitiesTests.
/// </summary>
public sealed class GeometryUtilitiesTests
{
    /// <summary>
    /// Runs test DistancePointToSegmentReturnsCorrectDistanceForPointOnSegment.
    /// </summary>
    [Fact]
    public void DistancePointToSegmentReturnsCorrectDistanceForPointOnSegment()
    {
        // Arrange
        var p = new Vec2(0.5, 0.5);
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);

        // Act
        var distance = GeometryUtilities.DistancePointToSegment(p, a, b);

        // Assert
        distance.Should().BeApproximately(0.0, 1e-9);
    }

    /// <summary>
    /// Runs test DistancePointToSegmentReturnsCorrectDistanceForPointBeyondEnd.
    /// </summary>
    [Fact]
    public void DistancePointToSegmentReturnsCorrectDistanceForPointBeyondEnd()
    {
        // Arrange
        var p = new Vec2(2, 2);
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);

        // Act
        var distance = GeometryUtilities.DistancePointToSegment(p, a, b);

        // Assert - distance to endpoint b
        var expected = Math.Sqrt(2); // sqrt((2-1)^2 + (2-1)^2)
        distance.Should().BeApproximately(expected, 1e-9);
    }

    /// <summary>
    /// Runs test DistancePointToSegmentReturnsCorrectDistanceForPointBeforeStart.
    /// </summary>
    [Fact]
    public void DistancePointToSegmentReturnsCorrectDistanceForPointBeforeStart()
    {
        // Arrange
        var p = new Vec2(-1, -1);
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);

        // Act
        var distance = GeometryUtilities.DistancePointToSegment(p, a, b);

        // Assert - distance to endpoint a
        var expected = Math.Sqrt(2);
        distance.Should().BeApproximately(expected, 1e-9);
    }

    /// <summary>
    /// Runs test DistancePointToSegmentReturnsCorrectDistanceForPerpendicularPoint.
    /// </summary>
    [Fact]
    public void DistancePointToSegmentReturnsCorrectDistanceForPerpendicularPoint()
    {
        // Arrange
        var p = new Vec2(1, 0);
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);

        // Act
        var distance = GeometryUtilities.DistancePointToSegment(p, a, b);

        // Assert
        distance.Should().BeApproximately(0.0, 1e-9);
    }

    /// <summary>
    /// Runs test DistancePointToSegmentHandlesDegenerateSegment.
    /// </summary>
    [Fact]
    public void DistancePointToSegmentHandlesDegenerateSegment()
    {
        // Arrange - segment with same start and end
        var p = new Vec2(3, 4);
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 0);

        // Act
        var distance = GeometryUtilities.DistancePointToSegment(p, a, b);

        // Assert - should be distance to the point
        distance.Should().BeApproximately(5.0, 1e-9);
    }

    /// <summary>
    /// Runs test TriangleSignedAreaReturnsPositiveForCCW.
    /// </summary>
    [Fact]
    public void TriangleSignedAreaReturnsPositiveForCCW()
    {
        // Arrange
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, 1);

        // Act
        var area = GeometryUtilities.TriangleSignedArea(a, b, c);

        // Assert
        area.Should().BeApproximately(1.0, 1e-9);
    }

    /// <summary>
    /// Runs test TriangleSignedAreaReturnsNegativeForCW.
    /// </summary>
    [Fact]
    public void TriangleSignedAreaReturnsNegativeForCW()
    {
        // Arrange
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 0);

        // Act
        var area = GeometryUtilities.TriangleSignedArea(a, b, c);

        // Assert
        area.Should().BeApproximately(-1.0, 1e-9);
    }

    /// <summary>
    /// Runs test TriangleAreaReturnsAbsoluteValue.
    /// </summary>
    [Fact]
    public void TriangleAreaReturnsAbsoluteValue()
    {
        // Arrange
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 0);

        // Act
        var area = GeometryUtilities.TriangleArea(a, b, c);

        // Assert
        area.Should().BeApproximately(1.0, 1e-9);
    }

    /// <summary>
    /// Runs test QuadAreaCalculatesCorrectly.
    /// </summary>
    [Fact]
    public void QuadAreaCalculatesCorrectly()
    {
        // Arrange - unit square
        var quad = (
            new Vec2(0, 0),
            new Vec2(1, 0),
            new Vec2(1, 1),
            new Vec2(0, 1)
        );

        // Act
        var area = GeometryUtilities.QuadArea(quad);

        // Assert
        area.Should().BeApproximately(1.0, 1e-9);
    }

    /// <summary>
    /// Runs test LerpVec2InterpolatesCorrectly.
    /// </summary>
    [Fact]
    public void LerpVec2InterpolatesCorrectly()
    {
        // Arrange
        var a = new Vec2(0, 0);
        var b = new Vec2(10, 10);

        // Act
        var result = GeometryUtilities.Lerp(a, b, 0.5);

        // Assert
        result.X.Should().BeApproximately(5.0, 1e-9);
        result.Y.Should().BeApproximately(5.0, 1e-9);
    }

    /// <summary>
    /// Runs test LerpVec3InterpolatesCorrectly.
    /// </summary>
    [Fact]
    public void LerpVec3InterpolatesCorrectly()
    {
        // Arrange
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 20, 30);

        // Act
        var result = GeometryUtilities.Lerp(a, b, 0.5);

        // Assert
        result.X.Should().BeApproximately(5.0, 1e-9);
        result.Y.Should().BeApproximately(10.0, 1e-9);
        result.Z.Should().BeApproximately(15.0, 1e-9);
    }

    /// <summary>
    /// Runs test LerpScalarInterpolatesCorrectly.
    /// </summary>
    [Fact]
    public void LerpScalarInterpolatesCorrectly()
    {
        // Arrange
        double a = 0;
        double b = 100;

        // Act
        var result = GeometryUtilities.LerpScalar(a, b, 0.25);

        // Assert
        result.Should().BeApproximately(25.0, 1e-9);
    }
}
