#pragma warning disable IDE0005, CS1591
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Advanced tests for complex scenarios including multi-hole structures,
    /// tiny notches, and high refinement stress cases.
    /// </summary>
    public sealed class ComplexScenarioTests
    {
        [Fact]
        public void MultipleHolesWithRefinement()
        {
            // Arrange - Outer rectangle with 3 holes
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 8), new Vec2(0, 8)
            });

            var hole1 = Polygon2D.FromPoints(new[]
            {
                new Vec2(1, 1), new Vec2(3, 1), new Vec2(3, 3), new Vec2(1, 3)
            });

            var hole2 = Polygon2D.FromPoints(new[]
            {
                new Vec2(5, 1), new Vec2(7, 1), new Vec2(7, 3), new Vec2(5, 3)
            });

            var hole3 = Polygon2D.FromPoints(new[]
            {
                new Vec2(2.5, 5), new Vec2(4.5, 5), new Vec2(4.5, 7), new Vec2(2.5, 7)
            });

            var structure = new PrismStructureDefinition(outer, 0, 2)
                .AddHole(hole1)
                .AddHole(hole2)
                .AddHole(hole3);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.8)
                .WithTargetEdgeLengthZ(1.0)
                .WithHoleRefinement(0.4, 1.2) // Refine within 1.2 units of holes
                .WithCaps(bottom: true, top: true)
                .WithRejectedCapTriangles(true)
                .Build();

            // Act
            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh);

            // Assert
            indexed.VertexCount.Should().BeGreaterThan(50, "Multi-hole structure should generate significant geometry");
            indexed.QuadCount.Should().BeGreaterThan(30, "Should generate substantial side and cap quads");

            // Should have some triangles due to complex hole topology
            var triangleCount = indexed.TriangleCount;
            triangleCount.Should().BeGreaterThan(0, "Complex hole topology should produce some triangles");

            // Verify no degenerate geometry
            foreach (var quad in indexed.Quads)
            {
                var v0 = indexed.Vertices[quad.v0];
                var v1 = indexed.Vertices[quad.v1];
                var v2 = indexed.Vertices[quad.v2];
                var v3 = indexed.Vertices[quad.v3];

                // All vertices should be different
                v0.Should().NotBe(v1);
                v0.Should().NotBe(v2);
                v0.Should().NotBe(v3);
            }
        }

        [Fact]
        public void TinyNotchHandling()
        {
            // Arrange - Rectangle with a very small notch
            var vertices = new[]
            {
                new Vec2(0, 0),
                new Vec2(5, 0),
                new Vec2(5, 2.5),
                new Vec2(5.01, 2.5),  // Tiny notch - 0.01 units wide
                new Vec2(5.01, 2.51), // 0.01 units deep
                new Vec2(5, 2.51),
                new Vec2(5, 5),
                new Vec2(0, 5)
            };

            var outer = Polygon2D.FromPoints(vertices);
            var structure = new PrismStructureDefinition(outer, 0, 1);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .WithMinCapQuadQuality(0.1) // Allow low quality for challenging geometry
                .Build();

            // Act
            var action = () => new PrismMesher().Mesh(structure, options);

            // Assert - Should not crash and should produce reasonable mesh
            action.Should().NotThrow("Tiny notch should be handled gracefully");

            var mesh = action();
            mesh.QuadCount.Should().BeGreaterThan(5, "Should still generate geometry despite tiny notch");

            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(10, "Should generate vertices for tiny notch handling");
        }

        [Fact]
        public void HighRefinementStressTest()
        {
            // Arrange - Simple geometry with very high refinement requirements
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2)
            });

            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(0.8, 0.8), new Vec2(1.2, 0.8), new Vec2(1.2, 1.2), new Vec2(0.8, 1.2)
            });

            var structure = new PrismStructureDefinition(outer, 0, 0.5)
                .AddHole(hole)
                .AddConstraintSegment(new Segment2D(new Vec2(0, 1), new Vec2(2, 1)), 0.25);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.05) // Very fine mesh
                .WithTargetEdgeLengthZ(0.05)
                .WithHoleRefinement(0.02, 0.5) // Very fine near holes
                .WithSegmentRefinement(0.02, 0.3) // Very fine near segments
                .WithCaps(bottom: true, top: true)
                .WithMinCapQuadQuality(0.2)
                .Build();

            // Act
            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh);

            // Assert
            indexed.VertexCount.Should().BeGreaterThan(100, "High refinement should generate many vertices");
            indexed.QuadCount.Should().BeGreaterThan(80, "High refinement should generate many quads");

            // Verify mesh quality under stress
            var capQuads = indexed.Quads.Where(q =>
            {
                var v0 = indexed.Vertices[q.v0];
                var v1 = indexed.Vertices[q.v1];
                var v2 = indexed.Vertices[q.v2];
                var v3 = indexed.Vertices[q.v3];
                return Math.Abs(v0.Z - v1.Z) < 0.001 && Math.Abs(v1.Z - v2.Z) < 0.001;
            }).ToList();

            capQuads.Should().NotBeEmpty("Should generate cap quads even under high refinement");
        }

        [Fact]
        public void LShapedGeometryWithInternalSurface()
        {
            // Arrange - L-shaped outer with internal surface
            var lShape = new[]
            {
                new Vec2(0, 0),
                new Vec2(3, 0),
                new Vec2(3, 2),
                new Vec2(1, 2),
                new Vec2(1, 4),
                new Vec2(0, 4)
            };

            var outer = Polygon2D.FromPoints(lShape);
            var structure = new PrismStructureDefinition(outer, -1, 3);

            // Add internal surface at middle level
            var internalOutline = Polygon2D.FromPoints(new[]
            {
                new Vec2(0.5, 0.5), new Vec2(2.5, 0.5), new Vec2(2.5, 1.5), new Vec2(0.5, 1.5)
            });

            structure = structure.AddInternalSurface(internalOutline, 1.0);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.4)
                .WithTargetEdgeLengthZ(0.8)
                .WithCaps(bottom: true, top: true)
                .Build();

            // Act
            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh);

            // Assert
            indexed.VertexCount.Should().BeGreaterThan(30, "L-shape with internal surface should generate complex geometry");
            indexed.QuadCount.Should().BeGreaterThan(25, "Should generate quads for sides, caps, and internal surface");

            // Verify internal surface vertices exist at Z = 1.0
            var internalVertices = indexed.Vertices.Where(v => Math.Abs(v.Z - 1.0) < 0.001).ToList();
            internalVertices.Should().NotBeEmpty("Should have vertices at internal surface level");
        }

        [Fact]
        public void StarShapedPolygonWithHole()
        {
            // Arrange - Star-shaped polygon (non-convex)
            var starVertices = new Vec2[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = 2 * Math.PI * i / 10;
                double radius = (i % 2 == 0) ? 3.0 : 1.5; // Alternating radii for star shape
                starVertices[i] = new Vec2(
                    radius * Math.Cos(angle),
                    radius * Math.Sin(angle)
                );
            }

            var outer = Polygon2D.FromPoints(starVertices);

            // Small circular-ish hole in center
            var holeVertices = new Vec2[8];
            for (int i = 0; i < 8; i++)
            {
                double angle = 2 * Math.PI * i / 8;
                holeVertices[i] = new Vec2(
                    0.3 * Math.Cos(angle),
                    0.3 * Math.Sin(angle)
                );
            }
            var hole = Polygon2D.FromPoints(holeVertices);

            var structure = new PrismStructureDefinition(outer, 0, 1)
                .AddHole(hole);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.3)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .WithMinCapQuadQuality(0.1) // Allow lower quality for complex star shape
                .WithRejectedCapTriangles(true)
                .Build();

            // Act
            var action = () => new PrismMesher().Mesh(structure, options);

            // Assert - Should handle complex non-convex shape
            action.Should().NotThrow("Star shape with hole should be processable");

            var mesh = action();
            var indexed = IndexedMesh.FromMesh(mesh);

            indexed.VertexCount.Should().BeGreaterThan(20, "Star shape should generate substantial geometry");

            // Should have both quads and triangles due to complex topology
            var totalElements = indexed.QuadCount + indexed.TriangleCount;
            totalElements.Should().BeGreaterThan(15, "Complex star shape should generate substantial elements");
        }

        [Fact]
        public void QualityEvaluatorPublicApiWorks()
        {
            // Arrange - Perfect square
            ReadOnlySpan<Vec2> perfectSquare = stackalloc Vec2[]
            {
                new(0, 0), new(1, 0), new(1, 1), new(0, 1)
            };

            // Degenerate quad (triangle)
            ReadOnlySpan<Vec2> degenerateQuad = stackalloc Vec2[]
            {
                new(0, 0), new(1, 0), new(1, 1), new(1, 1)
            };

            // Act & Assert
            var perfectScore = QualityEvaluator.ScoreQuad(perfectSquare);
            perfectScore.Should().BeGreaterThan(0.9, "Perfect square should have high quality score");

            var degenerateScore = QualityEvaluator.ScoreQuad(degenerateQuad);
            degenerateScore.Should().BeLessThan(0.5, "Degenerate quad should have low quality score");

            // Test threshold checking
            QualityEvaluator.MeetsQualityThreshold(perfectSquare, 0.8).Should().BeTrue();
            QualityEvaluator.MeetsQualityThreshold(degenerateQuad, 0.8).Should().BeFalse();

            // Test detailed metrics
            var metrics = QualityEvaluator.EvaluateDetailedQuality(perfectSquare);
            metrics.OverallScore.Should().BeGreaterThan(0.9);
            metrics.AspectRatio.Should().BeGreaterThan(0.9, "Perfect square should have good aspect ratio");
            metrics.Area.Should().BeApproximately(1.0, 0.01, "Unit square should have area 1");
        }
    }
}
