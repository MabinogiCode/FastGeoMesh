using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    public sealed class V2ResultPatternMeshingWorks
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .Build();

            optionsResult.IsSuccess.Should().BeTrue();

            var mesher = TestServiceProvider.CreatePrismMesher();
            var meshResult = mesher.Mesh(structure, optionsResult.Value);

            meshResult.IsSuccess.Should().BeTrue();
            meshResult.Value.Should().NotBeNull();
            meshResult.Value.QuadCount.Should().BeGreaterThan(0);

            var indexed = IndexedMesh.FromMesh(meshResult.Value);
            indexed.VertexCount.Should().BeGreaterThan(0);
            indexed.QuadCount.Should().BeGreaterThan(0);
        }
    }
}
