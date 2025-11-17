using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class LShapeWithoutExtraGeometryMeshesCapsAndSidesManifoldTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(3, 3), new Vec2(3, 8), new Vec2(0, 8) });
            var structure = new PrismStructureDefinition(outer, 0, 4);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.75)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();

            int topQuads = mesh.Quads.Count(q => q.V0.Z == 4 && q.V1.Z == 4 && q.V2.Z == 4 && q.V3.Z == 4);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 4 && t.V1.Z == 4 && t.V2.Z == 4);
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
