using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class InternalSurfaceWithoutOuterCapStillGeneratesOnlyPlateTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var plateOuter = outer;
            var structure = new PrismStructureDefinition(outer, 0, 5).AddInternalSurface(plateOuter, 2.5);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(2.0),
                TargetEdgeLengthZ = EdgeLength.From(2.0),
                GenerateBottomCap = false,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.0
            };
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, opt).UnwrapForTests();
            mesh.Quads.Should().NotBeEmpty();
            mesh.Quads.All(q => (q.V0.Z == 2.5 && q.V1.Z == 2.5 && q.V2.Z == 2.5 && q.V3.Z == 2.5) || (q.V0.Z != q.V1.Z || q.V1.Z != q.V2.Z || q.V2.Z != q.V3.Z)).Should().BeTrue();
        }
    }
}
