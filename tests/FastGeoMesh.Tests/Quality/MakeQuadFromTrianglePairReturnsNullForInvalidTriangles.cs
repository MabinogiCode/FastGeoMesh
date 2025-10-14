using FastGeoMesh.Application.Helpers.Quality;
using FluentAssertions;
using LibTessDotNet;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Ensures MakeQuadFromTrianglePair returns null when triangles don't share an edge.
    /// </summary>
    public sealed class MakeQuadFromTrianglePairReturnsNullForInvalidTriangles
    {
        [Fact]
        public void Test()
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
    }
}
