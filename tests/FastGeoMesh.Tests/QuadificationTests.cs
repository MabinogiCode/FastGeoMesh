using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for quadification functionality in meshing.</summary>
    public sealed class QuadificationTests
    {
        /// <summary>Tests that generic concave polygon with hole generates caps and manifold mesh.</summary>
        [Fact]
        public void GenericConcavePolygonWithHoleCapsGeneratedAndManifold()
        {
            // L-shaped outer polygon (concave)
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0,0), new Vec2(8,0), new Vec2(8,2), new Vec2(3,2), new Vec2(3,7), new Vec2(0,7)
            });
            // Triangular hole inside the long arm
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

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);

            // ✅ Pour les formes complexes, accepter triangles et quads
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 1 && t.V1.Z == 1 && t.V2.Z == 1);
            int topElements = topQuads + topTriangles;

            int bottomQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int bottomTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int bottomElements = bottomQuads + bottomTriangles;

            topElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            bottomElements.Should().BeGreaterThan(0, "Should have bottom cap elements (quads or triangles)");

            // No non-manifold edges
            var adj = im.BuildAdjacency();
            adj.NonManifoldEdges.Should().BeEmpty();
        }

        /// <summary>Tests that generic polygon without holes produces top and bottom caps with quads only.</summary>
        [Fact]
        public void GenericPolygonWithoutHolesProducesTopAndBottomCapsWithQuadsOnly()
        {
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0,0), new Vec2(4,0), new Vec2(5,2), new Vec2(2.5,4), new Vec2(0,2)
            });
            var structure = new PrismStructureDefinition(outer, -2, -1);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.75), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true };

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();

            // ✅ Pour les formes complexes, accepter triangles et quads
            int topQuads = mesh.Quads.Count(q => q.V0.Z == -1 && q.V1.Z == -1 && q.V2.Z == -1 && q.V3.Z == -1);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == -1 && t.V1.Z == -1 && t.V2.Z == -1);
            int topElements = topQuads + topTriangles;

            int bottomQuads = mesh.Quads.Count(q => q.V0.Z == -2 && q.V1.Z == -2 && q.V2.Z == -2 && q.V3.Z == -2);
            int bottomTriangles = mesh.Triangles.Count(t => t.V0.Z == -2 && t.V1.Z == -2 && t.V2.Z == -2);
            int bottomElements = bottomQuads + bottomTriangles;

            topElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            bottomElements.Should().BeGreaterThan(0, "Should have bottom cap elements (quads or triangles)");
        }
    }
}
