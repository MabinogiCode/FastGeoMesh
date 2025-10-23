using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing {
    public sealed class TShapeWithoutExtraGeometryMeshesCapsAndSidesManifoldTest {
        [Fact]
        public void Test() {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(7, 0), new Vec2(7, 2), new Vec2(5, 2), new Vec2(5, 5), new Vec2(2, 5), new Vec2(2, 2), new Vec2(0, 2) });
            var structure = new PrismStructureDefinition(outer, -3, 0);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int topElements = topQuads + topTriangles;
            int botQuads = mesh.Quads.Count(q => q.V0.Z == -3 && q.V1.Z == -3 && q.V2.Z == -3 && q.V3.Z == -3);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == -3 && t.V1.Z == -3 && t.V2.Z == -3);
            int botElements = botQuads + botTriangles;
            topElements.Should().BeGreaterThan(0);
            botElements.Should().BeGreaterThan(0);
            adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
