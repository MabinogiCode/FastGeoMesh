using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class ShapeVariationTests
    {
        [Fact]
        public void LShapeWithoutExtraGeometryMeshesCapsAndSidesManifold()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(8,0), new Vec2(8,3), new Vec2(3,3), new Vec2(3,8), new Vec2(0,8) });
            var structure = new PrismStructureDefinition(outer, 0, 4);
            var options = new MesherOptions { TargetEdgeLengthXY = 0.75, TargetEdgeLengthZ = 0.5 };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            int top = mesh.Quads.Count(q => q.V0.Z == 4 && q.V1.Z == 4 && q.V2.Z == 4 && q.V3.Z == 4);
            int bot = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            _ = top.Should().BeGreaterThan(0);
            _ = bot.Should().Be(top);
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }

        [Fact]
        public void LShapeWithExtraGeometryIntegratesConstraintAndSegments()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(8,0), new Vec2(8,3), new Vec2(3,3), new Vec2(3,8), new Vec2(0,8) });
            var structure = new PrismStructureDefinition(outer, 0, 6);
            _ = structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 1.0), new Vec2(7.5, 1.0)), 2.0);
            var pA = new Vec3(0.5, 2.5, 1.0);
            var pB = new Vec3(2.5, 2.5, 3.0);
            _ = structure.Geometry.AddPoint(pA).AddPoint(pB).AddSegment(new Segment3D(pA, pB));
            var sA = new Vec3(3.0, 3.0, 1.5);
            var sB = new Vec3(0.5, 6.5, 1.5);
            _ = structure.Geometry.AddSegment(new Segment3D(sA, sB));
            var tA = new Vec3(7.5, 0.5, 2.5);
            var tB = new Vec3(5.0, 2.5, 2.5);
            _ = structure.Geometry.AddSegment(new Segment3D(tA, tB));
            var options = new MesherOptions { TargetEdgeLengthXY = 0.75, TargetEdgeLengthZ = 0.5 };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            _ = mesh.Points.Should().Contain(pA);
            _ = mesh.Points.Should().Contain(pB);
            _ = mesh.InternalSegments.Should().HaveCount(3);
            var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
            _ = zset.Should().Contain(2.0).And.Contain(1.0).And.Contain(3.0).And.Contain(1.5).And.Contain(2.5);
            int top = mesh.Quads.Count(q => q.V0.Z == 6 && q.V1.Z == 6 && q.V2.Z == 6 && q.V3.Z == 6);
            int bot = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            _ = top.Should().BeGreaterThan(0);
            _ = bot.Should().Be(top);
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }

        [Fact]
        public void TShapeWithoutExtraGeometryMeshesCapsAndSidesManifold()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(7,0), new Vec2(7,2), new Vec2(5,2), new Vec2(5,5), new Vec2(2,5), new Vec2(2,2), new Vec2(0,2) });
            var structure = new PrismStructureDefinition(outer, -3, 0);
            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5 };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            int top = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int bot = mesh.Quads.Count(q => q.V0.Z == -3 && q.V1.Z == -3 && q.V2.Z == -3 && q.V3.Z == -3);
            _ = top.Should().BeGreaterThan(0);
            _ = bot.Should().Be(top);
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }

        [Fact]
        public void TShapeWithExtraGeometryIntegratesConstraints()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(7,0), new Vec2(7,2), new Vec2(5,2), new Vec2(5,5), new Vec2(2,5), new Vec2(2,2), new Vec2(0,2) });
            var structure = new PrismStructureDefinition(outer, -4, 0);
            _ = structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 0.5), new Vec2(6.5, 0.5)), -2.0);
            var b1 = new Vec3(2.5, 3.5, -1.0);
            var b2 = new Vec3(4.5, 3.5, -1.0);
            _ = structure.Geometry.AddSegment(new Segment3D(b1, b2));
            var st1 = new Vec3(2.0, 2.0, -0.5);
            var st2 = new Vec3(5.0, 2.0, -0.5);
            _ = structure.Geometry.AddSegment(new Segment3D(st1, st2));
            var an1 = new Vec3(6.8, 0.2, -3.0);
            var an2 = new Vec3(4.0, 2.0, -3.0);
            _ = structure.Geometry.AddSegment(new Segment3D(an1, an2));
            var refP = new Vec3(1.0, 1.0, -1.5);
            _ = structure.Geometry.AddPoint(refP);
            var options = new MesherOptions { TargetEdgeLengthXY = 0.8, TargetEdgeLengthZ = 0.4 };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            _ = mesh.Points.Should().Contain(refP);
            _ = mesh.InternalSegments.Should().HaveCount(3);
            var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
            _ = zset.Should().Contain(-2.0).And.Contain(-1.5).And.Contain(-1.0).And.Contain(-0.5).And.Contain(-3.0);
            int top = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int bot = mesh.Quads.Count(q => q.V0.Z == -4 && q.V1.Z == -4 && q.V2.Z == -4 && q.V3.Z == -4);
            _ = top.Should().BeGreaterThan(0);
            _ = bot.Should().Be(top);
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
