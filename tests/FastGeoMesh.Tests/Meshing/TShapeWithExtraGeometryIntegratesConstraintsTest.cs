using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class TShapeWithExtraGeometryIntegratesConstraintsTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(7, 0), new Vec2(7, 2), new Vec2(5, 2), new Vec2(5, 5), new Vec2(2, 5), new Vec2(2, 2), new Vec2(0, 2) });
            var structure = new PrismStructureDefinition(outer, -4, 0);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 0.5), new Vec2(6.5, 0.5)), -2.0);
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
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.8)
                .WithTargetEdgeLengthZ(0.4)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            mesh.Points.Should().Contain(refP);
            mesh.InternalSegments.Should().HaveCount(3);
            var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
            zset.Should().Contain(-2.0).And.Contain(-1.5).And.Contain(-1.0).And.Contain(-0.5).And.Contain(-3.0);
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int topElements = topQuads + topTriangles;
            int botQuads = mesh.Quads.Count(q => q.V0.Z == -4 && q.V1.Z == -4 && q.V2.Z == -4 && q.V3.Z == -4);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == -4 && t.V1.Z == -4 && t.V2.Z == -4);
            int botElements = botQuads + botTriangles;
            topElements.Should().BeGreaterThan(0);
            botElements.Should().BeGreaterThan(0);
            adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
