using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Validates that zero-area quads receive a low score.
    /// </summary>
    public sealed class ScoreQuadHandlesZeroAreaQuads
    {
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
