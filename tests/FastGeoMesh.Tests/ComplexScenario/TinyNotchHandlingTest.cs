using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.ComplexScenario
{
    public sealed class TinyNotchHandlingTest
    {
        [Fact]
        public void Test()
        {
            var vertices = new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 2.5), new Vec2(5.01, 2.5), new Vec2(5.01, 2.51), new Vec2(5, 2.51), new Vec2(5, 5), new Vec2(0, 5) };
            var outer = Polygon2D.FromPoints(vertices);
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.5)
                .WithMinCapQuadQuality(0.1)
                .Build()
                .UnwrapForTests();
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            mesh.QuadCount.Should().BeGreaterThan(5);
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(10);
        }
    }
}
