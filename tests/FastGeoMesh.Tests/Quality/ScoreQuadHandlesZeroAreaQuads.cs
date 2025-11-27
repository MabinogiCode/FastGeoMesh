using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Tests for class ScoreQuadHandlesZeroAreaQuads.
    /// </summary>
    public sealed class ScoreQuadHandlesZeroAreaQuads
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            // Arrange - Zero area quad (all points collinear)
            var zeroAreaQuad = (
                new Vec2(0, 0), new Vec2(1, 0),
                new Vec2(2, 0), new Vec2(3, 0)
            );

            // Act
            var score = QuadQualityHelper.ScoreQuad(zeroAreaQuad);

            // Assert - Zero area quad still has some orthogonality but poor aspect ratio
            score.Should().BeLessThanOrEqualTo(0.2, "Zero area quad should have low score");
        }
    }
}
