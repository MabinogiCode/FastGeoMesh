using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Factories;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Factories;
/// <summary>
/// Tests for class Polygon2DFactoryTests.
/// </summary>
public sealed class Polygon2DFactoryTests
{
    /// <summary>
    /// Runs test CreateValidatedCreatesPolygonWithValidInput.
    /// </summary>
    [Fact]
    public void CreateValidatedCreatesPolygonWithValidInput()
    {
        // Arrange
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(2, 0),
            new Vec2(2, 2),
            new Vec2(0, 2)
        };

        // Act
        var polygon = Polygon2DFactory.CreateValidated(vertices);

        // Assert
        polygon.Should().NotBeNull();
        polygon.Count.Should().Be(4);
    }

    /// <summary>
    /// Runs test CreateValidatedThrowsForInvalidPolygon.
    /// </summary>
    [Fact]
    public void CreateValidatedThrowsForInvalidPolygon()
    {
        // Arrange - too few vertices
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0)
        };

        // Act & Assert
        var act = () => Polygon2DFactory.CreateValidated(vertices);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least 3 vertices*");
    }

    /// <summary>
    /// Runs test FromUnsafeCreatesPolygonWithoutValidation.
    /// </summary>
    [Fact]
    public void FromUnsafeCreatesPolygonWithoutValidation()
    {
        // Arrange - even invalid vertices work
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0),
            new Vec2(2, 0) // Collinear - invalid but FromUnsafe allows it
        };

        // Act
        var polygon = Polygon2DFactory.FromUnsafe(vertices);

        // Assert
        polygon.Should().NotBeNull();
        polygon.Count.Should().Be(3);
    }

    /// <summary>
    /// Runs test TryCreateReturnsTrueForValidPolygon.
    /// </summary>
    [Fact]
    public void TryCreateReturnsTrueForValidPolygon()
    {
        // Arrange
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0),
            new Vec2(1, 1),
            new Vec2(0, 1)
        };

        // Act
        var success = Polygon2DFactory.TryCreate(vertices, out var polygon, out var error);

        // Assert
        success.Should().BeTrue();
        polygon.Should().NotBeNull();
        error.Should().BeNull();
    }

    /// <summary>
    /// Runs test TryCreateReturnsFalseForInvalidPolygon.
    /// </summary>
    [Fact]
    public void TryCreateReturnsFalseForInvalidPolygon()
    {
        // Arrange
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0)
        };

        // Act
        var success = Polygon2DFactory.TryCreate(vertices, out var polygon, out var error);

        // Assert
        success.Should().BeFalse();
        polygon.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Runs test FromPointsIsSameAsCreateValidated.
    /// </summary>
    [Fact]
    public void FromPointsIsSameAsCreateValidated()
    {
        // Arrange
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0),
            new Vec2(1, 1),
            new Vec2(0, 1)
        };

        // Act
        var polygon = Polygon2DFactory.FromPoints(vertices);

        // Assert
        polygon.Should().NotBeNull();
        polygon.Count.Should().Be(4);
    }

    /// <summary>
    /// Runs test CreateValidatedReversesCWToCC W.
    /// </summary>
    [Fact]
    public void CreateValidatedReversesCWToCCW()
    {
        // Arrange - CW order
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(0, 1),
            new Vec2(1, 1),
            new Vec2(1, 0)
        };

        // Act
        var polygon = Polygon2DFactory.CreateValidated(vertices);

        // Assert
        polygon.Should().NotBeNull();
        // Should be reversed to CCW automatically
        polygon.Vertices[0].Should().Be(new Vec2(1, 0));
        polygon.Vertices[1].Should().Be(new Vec2(1, 1));
    }
}
