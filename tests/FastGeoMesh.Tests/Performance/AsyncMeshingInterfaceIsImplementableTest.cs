using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance {
    public sealed class AsyncMeshingInterfaceIsImplementableTest {
        [Fact]
        public async Task Test() {
            var mesher = new TestAsyncMesher();
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(TestGeometries.SmallSquareSide, 0), new Vec2(TestGeometries.SmallSquareSide, TestGeometries.SmallSquareSide), new Vec2(0, TestGeometries.SmallSquareSide)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
            var mesh = await mesher.MeshAsync(structure, options, CancellationToken.None);
            mesh.Should().NotBeNull();
            mesh.Value.Quads.Should().NotBeEmpty();
        }
    }
}
