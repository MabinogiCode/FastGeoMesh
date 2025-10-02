using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
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
                TargetEdgeLengthXY = 0.5,
                TargetEdgeLengthZ = 0.5,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);

            // Count caps
            int top = mesh.Quads.Count(q => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1);
            int bottom = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);

            top.Should().BeGreaterThan(0);
            bottom.Should().Be(top);

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
            var options = new MesherOptions { TargetEdgeLengthXY = 0.75, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true };

            var mesh = new PrismMesher().Mesh(structure, options);

            int top = mesh.Quads.Count(q => q.V0.Z == -1 && q.V1.Z == -1 && q.V2.Z == -1 && q.V3.Z == -1);
            int bottom = mesh.Quads.Count(q => q.V0.Z == -2 && q.V1.Z == -2 && q.V2.Z == -2 && q.V3.Z == -2);

            top.Should().BeGreaterThan(0);
            bottom.Should().Be(top);
        }
    }
}
