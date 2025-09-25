using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class ExcavationShapesTests
{
    [Fact]
    public void LShapeWithoutGeometryMeshesCapsAndSidesManifold()
    {
        var outer = Polygon2D.FromPoints(new[]
        {
            new Vec2(0,0), new Vec2(8,0), new Vec2(8,3), new Vec2(3,3), new Vec2(3,8), new Vec2(0,8)
        });
        var structure = new PrismStructureDefinition(outer, 0, 4);

        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 0.75,
            TargetEdgeLengthZ = 0.5,
            GenerateTopAndBottomCaps = true
        };

        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
        var adj = im.BuildAdjacency();

        int top = mesh.Quads.Count(q => q.V0.Z == 4 && q.V1.Z == 4 && q.V2.Z == 4 && q.V3.Z == 4);
        int bot = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
        top.Should().BeGreaterThan(0);
        bot.Should().Be(top);
        adj.NonManifoldEdges.Should().BeEmpty();
    }

    [Fact]
    public void LShapeWithGeometryIntegratesConstraintsAndSegments()
    {
        var outer = Polygon2D.FromPoints(new[]
        {
            new Vec2(0,0), new Vec2(8,0), new Vec2(8,3), new Vec2(3,3), new Vec2(3,8), new Vec2(0,8)
        });
        var structure = new PrismStructureDefinition(outer, 0, 6);

        // Lierne: constraint segment at Z=2.0 across lower arm
        structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 1.0), new Vec2(7.5, 1.0)), 2.0);

        // Poutre: horizontal beam segment
        var pA = new Vec3(0.5, 2.5, 1.0);
        var pB = new Vec3(2.5, 2.5, 3.0);
        structure.Geometry.AddPoint(pA).AddPoint(pB).AddSegment(new Segment3D(pA, pB));

        // Buton: strut across the corner
        var sA = new Vec3(3.0, 3.0, 1.5);
        var sB = new Vec3(0.5, 6.5, 1.5);
        structure.Geometry.AddSegment(new Segment3D(sA, sB));

        // Tirant d'ancrage: diagonal tie-back
        var tA = new Vec3(7.5, 0.5, 2.5);
        var tB = new Vec3(5.0, 2.5, 2.5);
        structure.Geometry.AddSegment(new Segment3D(tA, tB));

        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 0.75,
            TargetEdgeLengthZ = 0.5,
            GenerateTopAndBottomCaps = true
        };

        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
        var adj = im.BuildAdjacency();

        // Geometry integrated
        mesh.Points.Should().Contain(pA);
        mesh.Points.Should().Contain(pB);
        mesh.InternalSegments.Should().HaveCount(3);

        // Z-levels include constraint/tie z
        var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
        zset.Should().Contain(2.0);
        zset.Should().Contain(1.0);
        zset.Should().Contain(3.0);
        zset.Should().Contain(1.5);
        zset.Should().Contain(2.5);

        // Caps symmetry and manifold
        int top = mesh.Quads.Count(q => q.V0.Z == 6 && q.V1.Z == 6 && q.V2.Z == 6 && q.V3.Z == 6);
        int bot = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
        top.Should().BeGreaterThan(0);
        bot.Should().Be(top);
        adj.NonManifoldEdges.Should().BeEmpty();
    }

    [Fact]
    public void TShapeWithoutGeometryMeshesCapsAndSidesManifold()
    {
        // Simple orthogonal T shape (8 vertices)
        var outer = Polygon2D.FromPoints(new[]
        {
            new Vec2(0,0), new Vec2(7,0), new Vec2(7,2), new Vec2(5,2), new Vec2(5,5), new Vec2(2,5), new Vec2(2,2), new Vec2(0,2)
        });
        var structure = new PrismStructureDefinition(outer, -3, 0);

        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 1.0,
            TargetEdgeLengthZ = 0.5,
            GenerateTopAndBottomCaps = true
        };

        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
        var adj = im.BuildAdjacency();

        int top = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
        int bot = mesh.Quads.Count(q => q.V0.Z == -3 && q.V1.Z == -3 && q.V2.Z == -3 && q.V3.Z == -3);
        top.Should().BeGreaterThan(0);
        bot.Should().Be(top);
        adj.NonManifoldEdges.Should().BeEmpty();
    }

    [Fact]
    public void TShapeWithGeometryIntegratesConstraintsBeamsStrutsAnchors()
    {
        var outer = Polygon2D.FromPoints(new[]
        {
            new Vec2(0,0), new Vec2(7,0), new Vec2(7,2), new Vec2(5,2), new Vec2(5,5), new Vec2(2,5), new Vec2(2,2), new Vec2(0,2)
        });
        var structure = new PrismStructureDefinition(outer, -4, 0);

        // Lierne across the head of T at Z = -2.0
        structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 0.5), new Vec2(6.5, 0.5)), -2.0);

        // Poutre vertical stem beam
        var b1 = new Vec3(2.5, 3.5, -1.0);
        var b2 = new Vec3(4.5, 3.5, -1.0);
        structure.Geometry.AddSegment(new Segment3D(b1, b2));

        // Buton across stem
        var st1 = new Vec3(2.0, 2.0, -0.5);
        var st2 = new Vec3(5.0, 2.0, -0.5);
        structure.Geometry.AddSegment(new Segment3D(st1, st2));

        // Tirant d'ancrage diagonal
        var an1 = new Vec3(6.8, 0.2, -3.0);
        var an2 = new Vec3(4.0, 2.0, -3.0);
        structure.Geometry.AddSegment(new Segment3D(an1, an2));

        // A few reference points
        var refP = new Vec3(1.0, 1.0, -1.5);
        structure.Geometry.AddPoint(refP);

        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 0.8,
            TargetEdgeLengthZ = 0.4,
            GenerateTopAndBottomCaps = true
        };

        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
        var adj = im.BuildAdjacency();

        // Geometry integrated
        mesh.Points.Should().Contain(refP);
        mesh.InternalSegments.Should().HaveCount(3);

        // Z-levels include constraints and segment Zs
        var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
        zset.Should().Contain(-2.0);
        zset.Should().Contain(-1.5);
        zset.Should().Contain(-1.0);
        zset.Should().Contain(-0.5);
        zset.Should().Contain(-3.0);

        // Caps symmetry and manifold
        int top = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
        int bot = mesh.Quads.Count(q => q.V0.Z == -4 && q.V1.Z == -4 && q.V2.Z == -4 && q.V3.Z == -4);
        top.Should().BeGreaterThan(0);
        bot.Should().Be(top);
        adj.NonManifoldEdges.Should().BeEmpty();
    }
}
