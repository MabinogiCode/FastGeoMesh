using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class LShapeWithExtraGeometryIntegratesConstraintAndSegmentsTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(3, 3), new Vec2(3, 8), new Vec2(0, 8) });
            var structure = new PrismStructureDefinition(outer, 0, 6);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 1.0), new Vec2(7.5, 1.0)), 2.0);
            var pA = new Vec3(0.5, 2.5, 1.0);
            var pB = new Vec3(2.5, 2.5, 3.0);
            _ = structure.Geometry.AddPoint(pA).AddPoint(pB).AddSegment(new Segment3D(pA, pB));
            var sA = new Vec3(3.0, 3.0, 1.5);
            var sB = new Vec3(0.5, 6.5, 1.5);
            _ = structure.Geometry.AddSegment(new Segment3D(sA, sB));
            var tA = new Vec3(7.5, 0.5, 2.5);
            var tB = new Vec3(5.0, 2.5, 2.5);
            _ = structure.Geometry.AddSegment(new Segment3D(tA, tB));
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.75)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            mesh.Points.Should().Contain(pA);
            mesh.InternalSegments.Should().HaveCount(3);
            var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
            zset.Should().Contain(2.0).And.Contain(1.0).And.Contain(3.0).And.Contain(1.5).And.Contain(2.5);
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 6 && q.V1.Z == 6 && q.V2.Z == 6 && q.V3.Z == 6);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 6 && t.V1.Z == 6 && t.V2.Z == 6);
            int topElements = topQuads + topTriangles;
            int botQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int botElements = botQuads + botTriangles;
            topElements.Should().BeGreaterThan(0);
            botElements.Should().BeGreaterThan(0);
            adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
