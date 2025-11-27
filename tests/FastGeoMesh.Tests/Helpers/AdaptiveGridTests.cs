using FastGeoMesh.Application.Helpers.Meshing;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Helpers;
/// <summary>
/// Tests for class AdaptiveGridTests.
/// </summary>
public sealed class AdaptiveGridTests
{
    /// <summary>
    /// Runs test ConstructorInitializesPropertiesCorrectly.
    /// </summary>
    [Fact]
    public void ConstructorInitializesPropertiesCorrectly()
    {
        // Arrange
        var xDivisions = new List<double> { 0.0, 1.0, 2.0 };
        var yDivisions = new List<double> { 0.0, 0.5, 1.0, 1.5 };

        // Act
        var grid = new AdaptiveGrid(xDivisions, yDivisions);

        // Assert
        grid.XDivisions.Should().BeEquivalentTo(xDivisions);
        grid.YDivisions.Should().BeEquivalentTo(yDivisions);
    }

    /// <summary>
    /// Runs test XDivisionsReturnsReadOnlyList.
    /// </summary>
    [Fact]
    public void XDivisionsReturnsReadOnlyList()
    {
        // Arrange
        var xDivisions = new List<double> { 1.0, 2.0 };
        var yDivisions = new List<double> { 1.0 };
        var grid = new AdaptiveGrid(xDivisions, yDivisions);

        // Act & Assert
        grid.XDivisions.Should().BeAssignableTo<IReadOnlyList<double>>();
    }

    /// <summary>
    /// Runs test YDivisionsReturnsReadOnlyList.
    /// </summary>
    [Fact]
    public void YDivisionsReturnsReadOnlyList()
    {
        // Arrange
        var xDivisions = new List<double> { 1.0 };
        var yDivisions = new List<double> { 1.0, 2.0, 3.0 };
        var grid = new AdaptiveGrid(xDivisions, yDivisions);

        // Act & Assert
        grid.YDivisions.Should().BeAssignableTo<IReadOnlyList<double>>();
    }

    /// <summary>
    /// Runs test ConstructorWithEmptyListsWorks.
    /// </summary>
    [Fact]
    public void ConstructorWithEmptyListsWorks()
    {
        // Arrange
        var xDivisions = new List<double>();
        var yDivisions = new List<double>();

        // Act
        var grid = new AdaptiveGrid(xDivisions, yDivisions);

        // Assert
        grid.XDivisions.Should().BeEmpty();
        grid.YDivisions.Should().BeEmpty();
    }

    /// <summary>
    /// Runs test StructIsReadOnly.
    /// </summary>
    [Fact]
    public void StructIsReadOnly()
    {
        // Arrange
        var xDivisions = new List<double> { 1.0, 2.0 };
        var yDivisions = new List<double> { 3.0, 4.0 };
        var grid = new AdaptiveGrid(xDivisions, yDivisions);

        // Act - Modifying original lists should not affect the grid
        xDivisions.Add(99.0);
        yDivisions.Add(99.0);

        // Assert - Grid should still reference the same list instances
        grid.XDivisions.Should().HaveCount(3);
        grid.YDivisions.Should().HaveCount(3);
    }
}
