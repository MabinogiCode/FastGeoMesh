using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Ensures good quads (square) have higher scores than degenerate thin quads.
    /// </summary>
    public sealed class ScoreQuadReturnsHighScoreForGoodQuads
    {
        [Fact]
        public void Test()
        {
            // Arrange - Perfect square
            var perfectSquare = (
                new Vec2(0, 0), new Vec2(TestGeometries.UnitSquareSide, 0),
                new Vec2(TestGeometries.UnitSquareSide, TestGeometries.UnitSquareSide), new Vec2(0, TestGeometries.UnitSquareSide)
            );

            // Degenerate quad (very thin)
            var degenerateQuad = (
                new Vec2(0, 0), new Vec2(TestGeometries.StandardSquareSide, 0),
                new Vec2(TestGeometries.StandardSquareSide, 0.1), new Vec2(0, 0.1)
            );

            // Act
            var goodScore = QuadQualityHelper.ScoreQuad(perfectSquare);
            var badScore = QuadQualityHelper.ScoreQuad(degenerateQuad);

            // Assert - Perfect square must have high quality
            goodScore.Should().BeGreaterThanOrEqualTo(TestQualityThresholds.PerfectSquareMinQuality, "Perfect square must have high quality >= 0.8");
            badScore.Should().BeLessThan(TestQualityThresholds.MediumQualityThreshold, "Degenerate quad should have moderate quality due to orthogonality");
            goodScore.Should().BeGreaterThan(badScore, "Good quad should score higher than bad quad");
        }
    }
}
