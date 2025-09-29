using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Additional tests for degenerate scenarios & area invariants.</summary>
    public sealed class DegenerateAndInvariantsTests
    {
        [Fact]
        public void AreaInvariant_RectangleCapsApproximateFootprintAreaMinusHoles()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(10,0), new Vec2(10,6), new Vec2(0,6) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2,2), new Vec2(4,2), new Vec2(4,4), new Vec2(2,4) });
            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new PrismMesher().Mesh(structure, options);
            double footprintArea = Math.Abs(Polygon2D.SignedArea(outer.Vertices)) - Math.Abs(Polygon2D.SignedArea(hole.Vertices));
            var topQuads = mesh.Quads.Where(q => Math.Abs(q.V0.Z - 2) < 1e-9 && q.QualityScore.HasValue).ToList();
            // Approximate area by summing quad areas (shoelace)
            double SumArea() => topQuads.Sum(q => Math.Abs(Polygon2D.SignedArea(new[] { new Vec2(q.V0.X,q.V0.Y), new Vec2(q.V1.X,q.V1.Y), new Vec2(q.V2.X,q.V2.Y), new Vec2(q.V3.X,q.V3.Y) })));
            double capArea = SumArea();
            capArea.Should().BeGreaterThan(0);
            // Loose tolerance because discretisation: within 25%
            capArea.Should().BeInRange(footprintArea * 0.75, footprintArea * 1.25);
        }

        [Fact]
        public void DegenerateVerySmallHeightStillProducesSideFaces()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(3,0), new Vec2(3,3), new Vec2(0,3) });
            var structure = new PrismStructureDefinition(outer, 0, 0.0001);
            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 1.0, GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = new PrismMesher().Mesh(structure, options);
            mesh.Quads.Should().NotBeEmpty();
        }

        [Fact]
        public void ImmutableStructure_AddHoleReturnsNewInstance()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(5,0), new Vec2(5,5), new Vec2(0,5) });
            var baseStruct = new PrismStructureDefinition(outer, 0, 5);
            var hole = Polygon2D.FromPoints(new[] { new Vec2(1,1), new Vec2(2,1), new Vec2(2,2), new Vec2(1,2) });
            var next = baseStruct.AddHole(hole);
            next.Should().NotBeSameAs(baseStruct);
            baseStruct.Holes.Should().BeEmpty();
            next.Holes.Should().HaveCount(1);
        }

        [Fact]
        public void ImmutableStructure_AddConstraintReturnsNewInstance()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(5,0), new Vec2(5,5), new Vec2(0,5) });
            var baseStruct = new PrismStructureDefinition(outer, 0, 5);
            var seg = new Segment2D(new Vec2(1,1), new Vec2(4,1));
            var next = baseStruct.AddConstraintSegment(seg, 2.5);
            next.Should().NotBeSameAs(baseStruct);
            baseStruct.ConstraintSegments.Should().BeEmpty();
            next.ConstraintSegments.Should().HaveCount(1);
        }
    }
}
