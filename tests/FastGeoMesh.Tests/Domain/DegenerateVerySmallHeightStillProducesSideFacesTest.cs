using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Domain
{
    public sealed class DegenerateVerySmallHeightStillProducesSideFacesTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3) });
            var structure = new PrismStructureDefinition(outer, 0, 0.0001);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            mesh.Quads.Should().NotBeEmpty();
        }
    }
}
