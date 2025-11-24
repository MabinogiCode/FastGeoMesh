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
    /// Verifies MakeQuadFromTrianglePair succeeds with two triangles forming a quad.
    /// </summary>
    public sealed class MakeQuadFromTrianglePairWorksWithValidTriangles
    {
        [Fact]
        public void Test()
        {
            // Arrange - Two triangles sharing an edge
            var vertices = new ContourVertex[4];
            vertices[0].Position = new LibTessDotNet.Vec3(0, 0, 0);
            vertices[1].Position = new LibTessDotNet.Vec3((float)TestGeometries.UnitSquareSide, 0, 0);
            vertices[2].Position = new LibTessDotNet.Vec3((float)TestGeometries.UnitSquareSide, (float)TestGeometries.UnitSquareSide, 0);
            vertices[3].Position = new LibTessDotNet.Vec3(0, (float)TestGeometries.UnitSquareSide, 0);

            var triangle1 = (0, 1, 2); // Bottom-right triangle
            var triangle2 = (0, 2, 3); // Top-left triangle

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<IGeometryService>();

            // Act
            var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangle1, triangle2, vertices, geo);

            // Assert
            quad.Should().NotBeNull("Valid triangle pair should create quad");
            quad!.Value.v0.Should().Be(new Vec2(0, 0));
            quad.Value.v2.Should().Be(new Vec2(TestGeometries.UnitSquareSide, TestGeometries.UnitSquareSide));
        }
    }
}
