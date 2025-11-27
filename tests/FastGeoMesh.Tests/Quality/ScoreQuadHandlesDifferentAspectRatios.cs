using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Tests for class ScoreQuadHandlesDifferentAspectRatios.
    /// </summary>
    public sealed class ScoreQuadHandlesDifferentAspectRatios
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Theory]
        [InlineData(1.0, 1.0)]   // Perfect square
        [InlineData(2.0, 1.0)]   // Rectangle
        [InlineData(0.1, 1.0)]   // Very thin
        [InlineData(1.0, 0.1)]   // Very tall
        public void Test(double width, double height)
        {
            // Arrange
            var quad = (
                new Vec2(0, 0), new Vec2(width, 0),
                new Vec2(width, height), new Vec2(0, height)
            );

            // Act
            var score = QuadQualityHelper.ScoreQuad(quad);

            // Assert
            score.Should().BeInRange(0.0, 1.0, "Score should be in valid range");

            // Perfect square should have highest score among the test cases
            if (Math.Abs(width - height) < TestTolerances.Epsilon && Math.Abs(width - TestGeometries.UnitSquareSide) < TestTolerances.Epsilon)
            {
                score.Should().BeGreaterThan(TestQualityThresholds.MediumQualityThreshold, "Perfect unit square should have good score");
            }
        }
    }
}
