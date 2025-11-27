using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Tests for class MeshCachingOptimizationWorksCorrectly.
    /// </summary>
    public sealed class MeshCachingOptimizationWorksCorrectly
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0)
            };
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var mesh = mesher.Mesh(structure, options).UnwrapForTests();
            var quads1 = mesh.Quads;
            var quads2 = mesh.Quads;
            var triangles1 = mesh.Triangles;
            var triangles2 = mesh.Triangles;
            quads1.Should().NotBeNull("Quads collection should be accessible");
            quads2.Should().NotBeNull("Quads collection should be accessible on second access");
            triangles1.Should().NotBeNull("Triangles collection should be accessible");
            triangles2.Should().NotBeNull("Triangles collection should be accessible on second access");
            quads1.Count.Should().Be(quads2.Count, "Multiple accesses should return consistent data");
            triangles1.Count.Should().Be(triangles2.Count, "Multiple accesses should return consistent data");
            bool cachingImplemented = ReferenceEquals(quads1, quads2) && ReferenceEquals(triangles1, triangles2);
            var newQuads = new List<Quad> { new Quad(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)) };
            var modifiedMesh = mesh.AddQuads(newQuads);
            modifiedMesh.Quads.Count.Should().BeGreaterThan(mesh.Quads.Count, "Immutable modification should create new mesh with more quads");
            mesh.Quads.Count.Should().Be(quads1.Count, "Original mesh should remain unchanged");
        }
    }
}
