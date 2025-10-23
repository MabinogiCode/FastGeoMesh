using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using LibTessDotNet;
using Xunit;

namespace FastGeoMesh.Tests.Application {
    public sealed class QuadQualityHelperTests {
        [Fact]
        public void ScoreQuadScalarPerfectSquareReturnsHighScore() {
            var quad = (new Vec2(0,0), new Vec2(1,0), new Vec2(1,1), new Vec2(0,1));
            double score = QuadQualityHelper.ScoreQuad(quad);
            score.Should().BeGreaterThan(0.7);
            score.Should().BeLessThanOrEqualTo(1.0);
        }

        [Fact]
        public void MakeQuadFromTrianglePairReturnsNullForInvalidIndices() {
            var vertices = new ContourVertex[3];
            vertices[0].Position = new LibTessDotNet.Vec3(0,0,0);
            vertices[1].Position = new LibTessDotNet.Vec3(1,0,0);
            vertices[2].Position = new LibTessDotNet.Vec3(0,1,0);

            var result = QuadQualityHelper.MakeQuadFromTrianglePair((0,1,2),(0,2,3), vertices);
            result.Should().BeNull();
        }

        [Fact]
        public void MakeQuadFromTrianglePairReturnsQuadWhenPossible() {
            var vertices = new ContourVertex[4];
            vertices[0].Position = new LibTessDotNet.Vec3(0,0,0);
            vertices[1].Position = new LibTessDotNet.Vec3(1,0,0);
            vertices[2].Position = new LibTessDotNet.Vec3(1,1,0);
            vertices[3].Position = new LibTessDotNet.Vec3(0,1,0);

            var maybe = QuadQualityHelper.MakeQuadFromTrianglePair((0,1,2),(0,2,3), vertices);
            Assert.True(maybe.HasValue);
            var quad = maybe.Value; // safe after HasValue assertion
            // Ensure convex using domain helper
            GeometryHelper.IsConvex(quad).Should().BeTrue();
        }
    }
}
