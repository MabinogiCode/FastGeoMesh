using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class GenericConcavePolygonWithHoleCapsGeneratedAndManifoldTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0,0), new Vec2(8,0), new Vec2(8,2), new Vec2(3,2), new Vec2(3,7), new Vec2(0,7)
            });
            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(5,0.5), new Vec2(6.5,0.5), new Vec2(5.75,1.5)
            });

            var structure = new PrismStructureDefinition(outer, 0, 1)
                .AddHole(hole);

            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(0.5),
                TargetEdgeLengthZ = EdgeLength.From(0.5),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);

            int topQuads = mesh.Quads.Count(q => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 1 && t.V1.Z == 1 && t.V2.Z == 1);
            int topElements = topQuads + topTriangles;

            int bottomQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int bottomTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int bottomElements = bottomQuads + bottomTriangles;

            topElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            bottomElements.Should().BeGreaterThan(0, "Should have bottom cap elements (quads or triangles)");

            var adj = im.BuildAdjacency();
            adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
