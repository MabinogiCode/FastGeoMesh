using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Helpers;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class CapMeshingHelperTests
    {
        [Fact]
        public void RectangleCapsMatchExpectedCounts()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(10,0), new Vec2(10,4), new Vec2(0,4) });
            var structure = new PrismStructureDefinition(rect, 0, 2);
            var opt = new MesherOptions { TargetEdgeLengthXY = 2.0, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new Mesh();
            CapMeshingHelper.GenerateCaps(mesh, structure, opt, 0, 2);
            int bottom = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int top = mesh.Quads.Count(q => q.V0.Z == 2 && q.V1.Z == 2 && q.V2.Z == 2 && q.V3.Z == 2);
            bottom.Should().BeGreaterThan(0);
            top.Should().Be(bottom);
        }

        [Fact]
        public void GenericCapsProduceQualityScoresWithinRange()
        {
            var concave = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(6,0), new Vec2(6,2), new Vec2(2,2), new Vec2(2,6), new Vec2(0,6) });
            var structure = new PrismStructureDefinition(concave, -1, 0);
            var opt = new MesherOptions { TargetEdgeLengthXY = 0.75, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new Mesh();
            CapMeshingHelper.GenerateCaps(mesh, structure, opt, -1, 0);
            var capQuads = mesh.Quads.Where(q => q.V0.Z == -1 || q.V0.Z == 0).ToList();
            capQuads.Should().NotBeEmpty();
            foreach (var q in capQuads)
            {
                q.QualityScore.Should().NotBeNull();
                q.QualityScore!.Value.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1);
            }
        }
    }
}
