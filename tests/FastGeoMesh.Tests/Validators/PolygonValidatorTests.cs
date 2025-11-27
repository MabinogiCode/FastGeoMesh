using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Validators;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validators;
/// <summary>
/// Tests for class PolygonValidatorTests.
/// </summary>
public sealed class PolygonValidatorTests
{
    /// <summary>
    /// Runs test SignedAreaReturnsPositiveForCCWPolygon.
    /// </summary>
    [Fact]
    public void SignedAreaReturnsPositiveForCCWPolygon()
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
        var area = PolygonValidator.SignedArea(vertices);

        // Assert
        area.Should().BeApproximately(1.0, 1e-9);
    }

    /// <summary>
    /// Runs test SignedAreaReturnsNegativeForCWPolygon.
    /// </summary>
    [Fact]
    public void SignedAreaReturnsNegativeForCWPolygon()
    {
        // Arrange
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(0, 1),
            new Vec2(1, 1),
            new Vec2(1, 0)
        };

        // Act
        var area = PolygonValidator.SignedArea(vertices);

        // Assert
        area.Should().BeApproximately(-1.0, 1e-9);
    }

    /// <summary>
    /// Runs test ValidateReturnsTrueForValidPolygon.
    /// </summary>
    [Fact]
    public void ValidateReturnsTrueForValidPolygon()
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
        var isValid = PolygonValidator.Validate(vertices, out var error);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    /// <summary>
    /// Runs test ValidateReturnsFalseForTooFewVertices.
    /// </summary>
    [Fact]
    public void ValidateReturnsFalseForTooFewVertices()
    {
        // Arrange
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0)
        };

        // Act
        var isValid = PolygonValidator.Validate(vertices, out var error);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("Less than 3 vertices");
    }

    /// <summary>
    /// Runs test ValidateReturnsFalseForDegeneratePolygon.
    /// </summary>
    [Fact]
    public void ValidateReturnsFalseForDegeneratePolygon()
    {
        // Arrange - collinear points
        var vertices = new List<Vec2>
        {
            new Vec2(0, 0),
            new Vec2(1, 0),
            new Vec2(2, 0)
        };

        // Act
        var isValid = PolygonValidator.Validate(vertices, out var error);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("Degenerate");
    }

    /// <summary>
    /// Runs test OrientReturnsCorrectOrientation.
    /// </summary>
    [Fact]
    public void OrientReturnsCorrectOrientation()
    {
        // Arrange
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c1 = new Vec2(1, 1); // Counter-clockwise
        var c2 = new Vec2(2, 0); // Collinear
        var c3 = new Vec2(1, -1); // Clockwise

        // Act
        var orient1 = PolygonValidator.Orient(a, b, c1, 1e-9);
        var orient2 = PolygonValidator.Orient(a, b, c2, 1e-9);
        var orient3 = PolygonValidator.Orient(a, b, c3, 1e-9);

        // Assert
        orient1.Should().Be(1); // CCW
        orient2.Should().Be(0); // Collinear
        orient3.Should().Be(-1); // CW
    }
}
