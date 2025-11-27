using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using FluentAssertions;
using LibTessDotNet;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Quality
{
    /// <summary>
    /// Tests for class MakeQuadFromTrianglePairRejectsNonConvexResults.
    /// </summary>
    public sealed class MakeQuadFromTrianglePairRejectsNonConvexResults
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            // Arrange - Triangles that would create non-convex quad
            var vertices = new ContourVertex[4];
            vertices[0].Position = new LibTessDotNet.Vec3(0, 0, 0);
            vertices[1].Position = new LibTessDotNet.Vec3(2, 0, 0);
            vertices[2].Position = new LibTessDotNet.Vec3(1, 2, 0);  // Creates concave shape
            vertices[3].Position = new LibTessDotNet.Vec3(1, -1, 0);

            var triangle1 = (0, 1, 2);
            var triangle2 = (0, 2, 3);

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<IGeometryService>();

            // Act
            var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangle1, triangle2, vertices, geo);

            // Assert - Should either be null or create a valid convex quad
            if (quad.HasValue)
            {
                var helper = provider.GetRequiredService<IGeometryHelper>();
                helper.IsConvex(quad.Value).Should().BeTrue("Created quad should be convex");
            }
        }
    }
}
