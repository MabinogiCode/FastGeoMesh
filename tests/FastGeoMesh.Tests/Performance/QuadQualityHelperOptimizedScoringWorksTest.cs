using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Tests for class QuadQualityHelperOptimizedScoringWorksTest.
    /// </summary>
    public sealed class QuadQualityHelperOptimizedScoringWorksTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var perfectSquare = (
                new Vec2(0, 0), new Vec2(TestGeometries.UnitSquareSide, 0),
                new Vec2(TestGeometries.UnitSquareSide, TestGeometries.UnitSquareSide), new Vec2(0, TestGeometries.UnitSquareSide)
            );

            var degenerateQuad = (
                new Vec2(0, 0), new Vec2(TestGeometries.StandardSquareSide, 0),
                new Vec2(TestGeometries.StandardSquareSide, 0.1), new Vec2(0, 0.1)
            );

            var goodScore = QuadQualityHelper.ScoreQuad(perfectSquare);
            var badScore = QuadQualityHelper.ScoreQuad(degenerateQuad);

            goodScore.Should().BeGreaterThanOrEqualTo(TestQualityThresholds.PerfectSquareMinQuality);
            badScore.Should().BeLessThan(TestQualityThresholds.MediumQualityThreshold);
            goodScore.Should().BeGreaterThan(badScore);
        }
    }
}
