using FastGeoMesh.Application.Helpers.Meshing;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class RectangleCapsMatchExpectedCountsTest
    {
        [Fact]
        public void Test()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 4), new Vec2(0, 4) });
            var structure = new PrismStructureDefinition(rect, 0, 2);
            var opt = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(2.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new ImmutableMesh();
            var resultMesh = CapMeshingHelper.GenerateCaps(mesh, structure, opt, 0, 2);
            int bottom = resultMesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int top = resultMesh.Quads.Count(q => q.V0.Z == 2 && q.V1.Z == 2 && q.V2.Z == 2 && q.V3.Z == 2);
            bottom.Should().BeGreaterThan(0);
            top.Should().Be(bottom);
        }
    }
}
