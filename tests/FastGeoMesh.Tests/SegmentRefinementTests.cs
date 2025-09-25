using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class SegmentRefinementTests
{
    [Fact]
    public void CapsAreRefinedNearInternalSegments()
    {
        var outer = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(20,0), new Vec2(20,10), new Vec2(0,10)
        });
        var structure = new PrismStructureDefinition(outer, -1, 0);

        // internal segment in XY near (10,5)
        structure.Geometry
            .AddPoint(new Vec3(9, 5, -0.5))
            .AddPoint(new Vec3(11, 5, -0.5))
            .AddSegment(new Segment3D(new Vec3(9, 5, -0.5), new Vec3(11, 5, -0.5)));

        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 2.0,
            TargetEdgeLengthZ = 1.0,
            GenerateTopAndBottomCaps = true,
            TargetEdgeLengthXYNearSegments = 0.5,
            SegmentRefineBand = 1.0
        };

        var mesh = new PrismMesher().Mesh(structure, options);
        var top = mesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
        top.Should().NotBeEmpty();

        // quads near (10,5) should be smaller than far ones
        double Size(Quad q)
        {
            double e0 = Math.Sqrt((q.V1.X-q.V0.X)*(q.V1.X-q.V0.X) + (q.V1.Y-q.V0.Y)*(q.V1.Y-q.V0.Y));
            double e1 = Math.Sqrt((q.V2.X-q.V1.X)*(q.V2.X-q.V1.X) + (q.V2.Y-q.V1.Y)*(q.V2.Y-q.V1.Y));
            return 0.5*(e0+e1);
        }

        var near = top.Where(q => Math.Abs((q.V0.X+q.V1.X+q.V2.X+q.V3.X)/4.0 - 10.0) <= 1.0 && Math.Abs((q.V0.Y+q.V1.Y+q.V2.Y+q.V3.Y)/4.0 - 5.0) <= 1.0).Select(Size).ToList();
        var far = top.Where(q => Math.Abs((q.V0.X+q.V1.X+q.V2.X+q.V3.X)/4.0 - 10.0) > 3.0 || Math.Abs((q.V0.Y+q.V1.Y+q.V2.Y+q.V3.Y)/4.0 - 5.0) > 3.0).Select(Size).ToList();

        near.Should().NotBeEmpty();
        far.Should().NotBeEmpty();
        near.Average().Should().BeLessThan(far.Average());
    }
}
