using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class TriangleFallbackTests
    {
        [Fact]
        public void HighThresholdWithoutTriangleOutputProducesDegenerateQuads()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(4,0), new Vec2(4,1), new Vec2(3,1), new Vec2(3,2), new Vec2(4,2), new Vec2(4,4), new Vec2(0,4) });
            var st = new PrismStructureDefinition(outer, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = 0.6,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.99, // Very high threshold -> many rejects
                OutputRejectedCapTriangles = false
            };
            var mesh = new PrismMesher().Mesh(st, opt);
            
            // S1244 fix: Use epsilon-based comparison instead of direct double comparison
            const double epsilon = 1e-9;
            var bottomQuads = mesh.Quads.Where(q => 
                Math.Abs(q.V0.Z - 0) < epsilon && Math.Abs(q.V1.Z - 0) < epsilon && 
                Math.Abs(q.V2.Z - 0) < epsilon && Math.Abs(q.V3.Z - 0) < epsilon).ToList();
            bottomQuads.Should().NotBeEmpty();
            
            // S1244 fix: Use epsilon-based comparison for vertex equality
            bool anyDegenerate = bottomQuads.Any(q => 
                Math.Abs(q.V2.X - q.V3.X) < epsilon && 
                Math.Abs(q.V2.Y - q.V3.Y) < epsilon && 
                Math.Abs(q.V2.Z - q.V3.Z) < epsilon);
            anyDegenerate.Should().BeTrue("Triangle fallback should produce degenerate quads when triangles disabled");
        }

        [Fact]
        public void HighThresholdWithTriangleOutputProducesTriangles()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(4,0), new Vec2(4,1), new Vec2(3,1), new Vec2(3,2), new Vec2(4,2), new Vec2(4,4), new Vec2(0,4) });
            var st = new PrismStructureDefinition(outer, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = 0.6,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.99,
                OutputRejectedCapTriangles = true
            };
            var mesh = new PrismMesher().Mesh(st, opt);
            mesh.Triangles.Should().NotBeEmpty("Rejected quads should be emitted as triangles");
        }
    }
}
