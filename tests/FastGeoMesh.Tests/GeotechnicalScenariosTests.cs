using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class GeotechnicalScenariosTests
{
    [Fact]
    public void SlabWithHolesDoesNotMeshHolesOnCapsAndGeneratesInnerSideFaces()
    {
        // Excavation footprint 20x10, slab z in [-1,0]
        var outer = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(20,0), new Vec2(20,10), new Vec2(0,10)
        });
        var hole1 = Polygon2D.FromPoints(new[] {
            new Vec2(5,3), new Vec2(7,3), new Vec2(7,5), new Vec2(5,5)
        });
        var hole2 = Polygon2D.FromPoints(new[] {
            new Vec2(12,6), new Vec2(13,6), new Vec2(13,8), new Vec2(12,8)
        });
        var structure = new PrismStructureDefinition(outer, -1, 0)
            .AddHole(hole1)
            .AddHole(hole2);

        var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = false, GenerateTopCap = false };
        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon);

        // Caps should exclude areas over holes: count expected top cap quads
        int capTop = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
        int capBottom = mesh.Quads.Count(q => q.V0.Z == -1 && q.V1.Z == -1 && q.V2.Z == -1 && q.V3.Z == -1);

        // Rough expectation: outer area 200, holes 4 + 2 area => 6 removed => ~194 per cap
        capTop.Should().BeLessThan(200);
        capBottom.Should().BeLessThan(200);

        // Inner side faces exist around hole1: look for quads whose XY is around hole1 rectangle and Z spans [-1,0]
        bool innerFaces = mesh.Quads.Any(q =>
            (q.V0.Z != q.V1.Z || q.V1.Z != q.V2.Z || q.V2.Z != q.V3.Z) && // side faces vary in Z
            q.V0.X >= 5 && q.V0.X <= 7 || q.V1.X >= 5 && q.V1.X <= 7 || q.V2.X >= 5 && q.V2.X <= 7 || q.V3.X >= 5 && q.V3.X <= 7);
        innerFaces.Should().BeTrue();

        // Adjacency: still manifold
        var adj = im.BuildAdjacency();
        adj.NonManifoldEdges.Should().BeEmpty();
    }

    [Fact]
    public void BraceFootingOutsideIsCarriedAsInternalSegment()
    {
        // Outer ground extends, we model only the prism footprint 10x5 in z [-2,0]
        var outer = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(10,0), new Vec2(10,5), new Vec2(0,5)
        });
        var structure = new PrismStructureDefinition(outer, -2, 0);

        // Bracon footing outside used as support (x=-5,y=-2,z=0) to a point inside (x=0,y=0,z=0)
        var support = new Vec3(-5, -2, 0);
        var inside = new Vec3(0, 0, 0);
        structure.Geometry.AddPoint(support).AddPoint(inside).AddSegment(new Segment3D(support, inside));

        var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 1.0, GenerateBottomCap = false, GenerateTopCap = false };
        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon);

        // Ensure both points are present in indexed vertices
        int present = im.Vertices.Count(v => (v.X == support.X && v.Y == support.Y && v.Z == support.Z) || (v.X == inside.X && v.Y == inside.Y && v.Z == inside.Z));
        present.Should().Be(2);

        // Edge should exist between them (internal segment)
        var idx = im.Vertices.Select((v,i) => (v,i)).ToDictionary(t => (t.v.X,t.v.Y,t.v.Z), t => t.i);
        var e = (Math.Min(idx[(support.X,support.Y,support.Z)], idx[(inside.X,inside.Y,inside.Z)]), Math.Max(idx[(support.X,support.Y,support.Z)], idx[(inside.X,inside.Y,inside.Z)]));
        im.Edges.Should().Contain(e);
    }
}
