using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    public sealed class OptimizedMeshingProducesCorrectQuadCountTest
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(polygon, -5, 5);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(2.0), TargetEdgeLengthZ = EdgeLength.From(2.0), GenerateBottomCap = true, GenerateTopCap = true };
            var mesher = new PrismMesher();
            var mesh = mesher.Mesh(structure, options).UnwrapForTests();
            mesh.Quads.Should().NotBeEmpty();
            mesh.Quads.Count.Should().BeGreaterThan(200).And.BeLessThan(300);
        }
    }
}
