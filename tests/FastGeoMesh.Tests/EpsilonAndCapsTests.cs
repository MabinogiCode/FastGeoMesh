using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class EpsilonAndCapsTests
    {
        [Fact]
        public void FromMeshUsesExactKeyingWhenEpsilonIsDoubleEpsilon()
        {
            // Two very close points should NOT merge with epsilon = double.Epsilon
            var mesh = new Mesh();
            var p0 = new Vec3(0, 0, 0);
            var p1 = new Vec3(1e-10, 0, 0); // distinct but very close
            mesh.AddPoint(p0);
            mesh.AddPoint(p1);

            var imExact = IndexedMesh.FromMesh(mesh, double.Epsilon);
            _ = imExact.Vertices.Count.Should().Be(2);

            // With larger epsilon, they should merge
            var imMerge = IndexedMesh.FromMesh(mesh, 1e-6);
            _ = imMerge.Vertices.Count.Should().Be(1);
        }

        [Fact]
        public void CapsAreGeneratedWithExpectedQuadCountForAxisAlignedRectangle()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5)
            });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new PrismMesher().Mesh(structure, options);

            int capCount = mesh.Quads.Count(q =>
                (MathUtil.NearlyEqual(q.V0.Z, -10, options.Epsilon) && MathUtil.NearlyEqual(q.V1.Z, -10, options.Epsilon) && MathUtil.NearlyEqual(q.V2.Z, -10, options.Epsilon) && MathUtil.NearlyEqual(q.V3.Z, -10, options.Epsilon)) ||
                (MathUtil.NearlyEqual(q.V0.Z, 10, options.Epsilon) && MathUtil.NearlyEqual(q.V1.Z, 10, options.Epsilon) && MathUtil.NearlyEqual(q.V2.Z, 10, options.Epsilon) && MathUtil.NearlyEqual(q.V3.Z, 10, options.Epsilon)));

            _ = capCount.Should().Be(200);
        }

        [Fact]
        public void InternalSegmentIsCarriedToIndexedMeshAsEdge()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5)
            });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            // Add points and a segment in geometry
            var a = new Vec3(0, 4, 2);
            var b = new Vec3(20, 4, 4);
            _ = structure.Geometry.AddPoint(a).AddPoint(b).AddSegment(new Segment3D(a, b));

            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);

            // find indices for a and b
            int ia = im.Vertices.Select((v, i) => (v, i)).First(t => MathUtil.NearlyEqual(t.v.X, a.X, options.Epsilon) && MathUtil.NearlyEqual(t.v.Y, a.Y, options.Epsilon) && MathUtil.NearlyEqual(t.v.Z, a.Z, options.Epsilon)).i;
            int ib = im.Vertices.Select((v, i) => (v, i)).First(t => MathUtil.NearlyEqual(t.v.X, b.X, options.Epsilon) && MathUtil.NearlyEqual(t.v.Y, b.Y, options.Epsilon) && MathUtil.NearlyEqual(t.v.Z, b.Z, options.Epsilon)).i;
            var edge = ia < ib ? (ia, ib) : (ib, ia);

            _ = im.Edges.Should().Contain(edge);
        }

        [Fact]
        public void ConstraintZLevelIsPresentInSideQuads()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0,0), new Vec2(10,0), new Vec2(10,10), new Vec2(0,10)
            });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            // FIXED: AddConstraintSegment returns new immutable instance - must reassign
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(10, 0)), 2.5);
            var options = new MesherOptions { TargetEdgeLengthXY = 5.0, TargetEdgeLengthZ = 3.0, GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = new PrismMesher().Mesh(structure, options);

            bool hasZ = mesh.Quads.Any(q =>
                MathUtil.NearlyEqual(q.V0.Z, 2.5, options.Epsilon) ||
                MathUtil.NearlyEqual(q.V1.Z, 2.5, options.Epsilon) ||
                MathUtil.NearlyEqual(q.V2.Z, 2.5, options.Epsilon) ||
                MathUtil.NearlyEqual(q.V3.Z, 2.5, options.Epsilon));

            _ = hasZ.Should().BeTrue();
        }
    }
}
