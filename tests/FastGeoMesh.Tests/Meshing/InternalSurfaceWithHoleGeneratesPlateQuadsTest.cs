using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class InternalSurfaceWithHoleGeneratesPlateQuadsTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var plateOuter = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(3, 2), new Vec2(3, 3), new Vec2(2, 3) });
            var structure = new PrismStructureDefinition(outer, 0, 5).AddInternalSurface(plateOuter, 2.5, hole);
            var opt = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = true, GenerateTopCap = true };
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var mesh = mesher.Mesh(structure, opt).UnwrapForTests();
            var plateQuads = mesh.Quads.Where(q => q.V0.Z == 2.5 && q.V1.Z == 2.5 && q.V2.Z == 2.5 && q.V3.Z == 2.5).ToList();
            plateQuads.Should().NotBeEmpty();
            foreach (var q in plateQuads)
            {
                double cx = (q.V0.X + q.V1.X + q.V2.X + q.V3.X) * 0.25;
                double cy = (q.V0.Y + q.V1.Y + q.V2.Y + q.V3.Y) * 0.25;
                bool inHole = cx > 2 && cx < 3 && cy > 2 && cy < 3;
                inHole.Should().BeFalse();
            }
        }
    }
}
