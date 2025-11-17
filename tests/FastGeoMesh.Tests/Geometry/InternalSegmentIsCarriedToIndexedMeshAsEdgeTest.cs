using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class InternalSegmentIsCarriedToIndexedMeshAsEdgeTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            var a = new Vec3(0, 4, 2);
            var b = new Vec3(20, 4, 4);
            _ = structure.Geometry.AddPoint(a).AddPoint(b).AddSegment(new Segment3D(a, b));
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            int ia = im.Vertices.Select((v, i) => (v, i)).First(t => MathUtil.NearlyEqual(t.v.X, a.X, options.Epsilon) && MathUtil.NearlyEqual(t.v.Y, a.Y, options.Epsilon) && MathUtil.NearlyEqual(t.v.Z, a.Z, options.Epsilon)).i;
            int ib = im.Vertices.Select((v, i) => (v, i)).First(t => MathUtil.NearlyEqual(t.v.X, b.X, options.Epsilon) && MathUtil.NearlyEqual(t.v.Y, b.Y, options.Epsilon) && MathUtil.NearlyEqual(t.v.Z, b.Z, options.Epsilon)).i;
            var edge = ia < ib ? (ia, ib) : (ib, ia);
            _ = im.Edges.Should().Contain(edge);
        }
    }
}
