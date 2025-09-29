using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class InternalSurfaceTests
    {
        [Fact]
        public void InternalSurfaceWithHoleGeneratesPlateQuads()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var plateOuter = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(3, 2), new Vec2(3, 3), new Vec2(2, 3) });
            var structure = new PrismStructureDefinition(outer, 0, 5)
                .AddInternalSurface(plateOuter, 2.5, hole);
            var opt = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new PrismMesher().Mesh(structure, opt);
            // Plate quads at z=2.5
            var plateQuads = mesh.Quads.Where(q => q.V0.Z == 2.5 && q.V1.Z == 2.5 && q.V2.Z == 2.5 && q.V3.Z == 2.5).ToList();
            plateQuads.Should().NotBeEmpty();
            // Ensure hole area is excluded: no quad center inside hole
            foreach (var q in plateQuads)
            {
                double cx = (q.V0.X + q.V1.X + q.V2.X + q.V3.X) * 0.25;
                double cy = (q.V0.Y + q.V1.Y + q.V2.Y + q.V3.Y) * 0.25;
                bool inHole = cx > 2 && cx < 3 && cy > 2 && cy < 3;
                inHole.Should().BeFalse();
            }
        }

        [Fact]
        public void InternalSurfaceWithoutOuterCapStillGeneratesOnlyPlate()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var plateOuter = outer;
            var structure = new PrismStructureDefinition(outer, 0, 5)
                .AddInternalSurface(plateOuter, 2.5);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 2.0,
                GenerateBottomCap = false,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.0  // Allow all qualities for internal surfaces
            };
            var mesh = new PrismMesher().Mesh(structure, opt);
            mesh.Quads.Should().NotBeEmpty();
            // All quads should be either internal surface quads (all vertices at Z=2.5) or side quads (different Z values)
            mesh.Quads.All(q =>
                (q.V0.Z == 2.5 && q.V1.Z == 2.5 && q.V2.Z == 2.5 && q.V3.Z == 2.5) ||  // Internal surface
                (q.V0.Z != q.V1.Z || q.V1.Z != q.V2.Z || q.V2.Z != q.V3.Z)              // Side quad
            ).Should().BeTrue();
        }
    }
}
