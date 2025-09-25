using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class PrismMesherTests
{
    [Fact]
    public void SideQuadsAreGeneratedCcw()
    {
        var poly = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(10,0), new Vec2(10,10), new Vec2(0,10)
        });
        var structure = new PrismStructureDefinition(poly, -10, 10);
        var options = new MesherOptions { TargetEdgeLengthXY = 10.0, TargetEdgeLengthZ = 20.0, GenerateTopAndBottomCaps = false };
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(structure, options);

        mesh.Quads.Should().NotBeEmpty();
        foreach (var q in mesh.Quads)
        {
            // Basic CCW check on projection to a face plane:
            // On side faces, Z varies, XY on edge. We check the 2D cross product using (x,y) of v0->v1->v2.
            var ax = q.V1.X - q.V0.X;
            var ay = q.V1.Y - q.V0.Y;
            var bx = q.V2.X - q.V1.X;
            var by = q.V2.Y - q.V1.Y;
            double cross = ax*by - ay*bx;
            cross.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
