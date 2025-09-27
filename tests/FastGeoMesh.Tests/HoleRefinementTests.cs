using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class HoleRefinementTests
    {
        [Fact]
        public void CapsAreRefinedNearHoles()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(20,0), new Vec2(20,10), new Vec2(0,10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(10,4), new Vec2(12,4), new Vec2(12,6), new Vec2(10,6) });
            var structure = new PrismStructureDefinition(outer, -1, 0).AddHole(hole);
            var options = new MesherOptions { TargetEdgeLengthXY = 2.0, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true, TargetEdgeLengthXYNearHoles = 0.5, HoleRefineBand = 1.0 };
            var mesh = new PrismMesher().Mesh(structure, options);
            var topQuads = mesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
            topQuads.Should().NotBeEmpty();
            double AvgEdge(Vec3 a, Vec3 b, Vec3 c, Vec3 d)
            {
                double e0 = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
                double e1 = Math.Sqrt((c.X - b.X) * (c.X - b.X) + (c.Y - b.Y) * (c.Y - b.Y));
                return 0.5 * (e0 + e1);
            }
            var sizes = topQuads.Select(q => AvgEdge(q.V0, q.V1, q.V2, q.V3)).ToList();
            sizes.Min().Should().BeLessThan(1.5);
            sizes.Max().Should().BeGreaterThan(1.5);
        }
    }
}
