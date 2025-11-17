using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.ComplexScenario
{
    public sealed class LShapedGeometryWithInternalSurfaceTest
    {
        [Fact]
        public void Test()
        {
            var lShape = new[] { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 2), new Vec2(1, 2), new Vec2(1, 4), new Vec2(0, 4) };
            var outer = Polygon2D.FromPoints(lShape);
            var structure = new PrismStructureDefinition(outer, -1, 3);
            var internalOutline = Polygon2D.FromPoints(new[] { new Vec2(0.5, 0.5), new Vec2(2.5, 0.5), new Vec2(2.5, 1.5), new Vec2(0.5, 1.5) });
            structure = structure.AddInternalSurface(internalOutline, 1.0);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.4)
                .WithTargetEdgeLengthZ(0.8)
                .Build()
                .UnwrapForTests();
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(30);
            indexed.QuadCount.Should().BeGreaterThan(25);
            indexed.Vertices.Any(v => Math.Abs(v.Z - 1.0) < 0.001).Should().BeTrue();
        }
    }
}
