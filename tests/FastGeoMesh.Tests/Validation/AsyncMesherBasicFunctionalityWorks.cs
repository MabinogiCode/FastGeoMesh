using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Tests for class AsyncMesherBasicFunctionalityWorks.
    /// </summary>
    public sealed class AsyncMesherBasicFunctionalityWorks
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public async Task Test()
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
            var asyncMesher = (IAsyncMesher)mesher;
            var mesh = await asyncMesher.MeshAsync(structure, options);
            mesh.Value.Should().NotBeNull();
            mesh.Value.QuadCount.Should().BeGreaterThan(0);
            mesh.Value.Points.Should().NotBeEmpty();
        }
    }
}
