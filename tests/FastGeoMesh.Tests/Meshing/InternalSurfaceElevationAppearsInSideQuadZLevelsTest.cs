using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing {
    public sealed class InternalSurfaceElevationAppearsInSideQuadZLevelsTest {
        [Fact]
        public void Test() {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(outer, 0, 5)
                .AddInternalSurface(outer, 2.5);
            var opt = new MesherOptions {
                TargetEdgeLengthXY = EdgeLength.From(5.0),
                TargetEdgeLengthZ = EdgeLength.From(10.0),
                GenerateBottomCap = false,
                GenerateTopCap = false
            };

            var mesh = new PrismMesher().Mesh(structure, opt).UnwrapForTests();

            var zset = mesh.Quads
                .Where(q => !(q.V0.Z == q.V1.Z && q.V1.Z == q.V2.Z && q.V2.Z == q.V3.Z))
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            zset.Should().Contain(2.5);
            zset.Should().Contain(0);
            zset.Should().Contain(5);
        }
    }
}
