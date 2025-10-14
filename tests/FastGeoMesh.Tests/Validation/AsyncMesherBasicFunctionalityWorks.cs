using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Validates that asynchronous meshing produces a valid mesh with expected properties.
    /// </summary>
    public sealed class AsyncMesherBasicFunctionalityWorks
    {
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
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var mesh = await asyncMesher.MeshAsync(structure, options);
            mesh.Value.Should().NotBeNull();
            mesh.Value.QuadCount.Should().BeGreaterThan(0);
            mesh.Value.Points.Should().NotBeEmpty();
        }
    }
}
