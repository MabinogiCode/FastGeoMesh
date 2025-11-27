using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FastGeoMesh.Domain.Interfaces;
namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Tests for class PrismMesherBasicFunctionalityWorks.
    /// </summary>
    public sealed class PrismMesherBasicFunctionalityWorks
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            structure.Geometry.AddPoint(new Vec3(5, 2.5, 1));
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var mesh = mesher.Mesh(structure, options).UnwrapForTests();
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
            mesh.Points.Should().NotBeEmpty();
        }
    }
}

