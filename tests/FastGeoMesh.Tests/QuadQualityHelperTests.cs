using System;
using LibTessDotNet;
using FastGeoMesh.Geometry;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for <see cref="QuadQualityHelper"/> scoring and tessellation functions.</summary>
    public sealed class QuadQualityHelperTests
    {
        /// <summary>
        /// Ensures good quads (square) have higher scores than degenerate thin quads.
        /// </summary>
        [Fact]
        public void ScoreQuadReturnsHighScoreForGoodQuads()
        {
            // Arrange - Perfect square
            var perfectSquare = (
                new Vec2(0, 0), new Vec2(1, 0),
                new Vec2(1, 1), new Vec2(0, 1)
            );

            // Degenerate quad (very thin)
            var degenerateQuad = (
                new Vec2(0, 0), new Vec2(10, 0),
                new Vec2(10, 0.1), new Vec2(0, 0.1)
            );

            // Act
            var goodScore = QuadQualityHelper.ScoreQuad(perfectSquare);
            var badScore = QuadQualityHelper.ScoreQuad(degenerateQuad);

            // Assert - Un carré parfait DOIT avoir un score >= 0.8
            goodScore.Should().BeGreaterThanOrEqualTo(0.8, "Perfect square must have high quality >= 0.8");
            badScore.Should().BeLessThan(0.6, "Degenerate quad should have moderate quality due to orthogonality");
            goodScore.Should().BeGreaterThan(badScore, "Good quad should score higher than bad quad");
        }

        /// <summary>
        /// Validates that zero-area quads receive a low score.
        /// </summary>
        [Fact]
        public void ScoreQuadHandlesZeroAreaQuads()
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

        /// <summary>
        /// Verifies MakeQuadFromTrianglePair succeeds with two triangles forming a quad.
        /// </summary>
        [Fact]
        public void MakeQuadFromTrianglePairWorksWithValidTriangles()
        {
            // Arrange - Two triangles sharing an edge
            var vertices = new ContourVertex[4];
            vertices[0].Position = new LibTessDotNet.Vec3(0, 0, 0);
            vertices[1].Position = new LibTessDotNet.Vec3(1, 0, 0);
            vertices[2].Position = new LibTessDotNet.Vec3(1, 1, 0);
            vertices[3].Position = new LibTessDotNet.Vec3(0, 1, 0);

            var triangle1 = (0, 1, 2); // Bottom-right triangle
            var triangle2 = (0, 2, 3); // Top-left triangle

            // Act
            var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangle1, triangle2, vertices);

            // Assert
            quad.Should().NotBeNull("Valid triangle pair should create quad");
            quad!.Value.v0.Should().Be(new Vec2(0, 0));
            quad.Value.v2.Should().Be(new Vec2(1, 1));
        }

        /// <summary>
        /// Ensures MakeQuadFromTrianglePair returns null when triangles don't share an edge.
        /// </summary>
        [Fact]
        public void MakeQuadFromTrianglePairReturnsNullForInvalidTriangles()
        {
            // Arrange - Two triangles not sharing exactly two vertices
            var vertices = new ContourVertex[6];
            for (int i = 0; i < 6; i++)
            {
                vertices[i].Position = new LibTessDotNet.Vec3(i, 0, 0);
            }

            var triangle1 = (0, 1, 2);
            var triangle2 = (3, 4, 5); // No shared vertices

            // Act
            var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangle1, triangle2, vertices);

            // Assert
            quad.Should().BeNull("Triangles with no shared edge should not create quad");
        }

        /// <summary>
        /// Validates MakeQuadFromTrianglePair rejects non-convex results or produces convex quads only.
        /// </summary>
        [Fact]
        public void MakeQuadFromTrianglePairRejectsNonConvexResults()
        {
            // Arrange - Triangles that would create non-convex quad
            var vertices = new ContourVertex[4];
            vertices[0].Position = new LibTessDotNet.Vec3(0, 0, 0);
            vertices[1].Position = new LibTessDotNet.Vec3(2, 0, 0);
            vertices[2].Position = new LibTessDotNet.Vec3(1, 2, 0);  // Creates concave shape
            vertices[3].Position = new LibTessDotNet.Vec3(1, -1, 0);

            var triangle1 = (0, 1, 2);
            var triangle2 = (0, 2, 3);

            // Act
            var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangle1, triangle2, vertices);

            // Assert - Should either be null or create a valid convex quad
            if (quad.HasValue)
            {
                GeometryHelper.IsConvex(quad.Value).Should().BeTrue("Created quad should be convex");
            }
        }

        /// <summary>
        /// Tests scoring stability across a variety of aspect ratios including perfect square and thin quads.
        /// </summary>
        [Theory]
        [InlineData(1.0, 1.0)]   // Perfect square
        [InlineData(2.0, 1.0)]   // Rectangle
        [InlineData(0.1, 1.0)]   // Very thin
        [InlineData(1.0, 0.1)]   // Very tall
        public void ScoreQuadHandlesDifferentAspectRatios(double width, double height)
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
            if (Math.Abs(width - height) < 1e-9 && Math.Abs(width - 1.0) < 1e-9)
            {
                score.Should().BeGreaterThan(0.6, "Perfect unit square should have good score");
            }
        }
    }
}
