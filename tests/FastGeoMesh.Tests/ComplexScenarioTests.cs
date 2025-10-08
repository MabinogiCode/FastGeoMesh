using FastGeoMesh.Application;
using FastGeoMesh.Domain;
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
        /// <summary>
        /// Tests meshing of complex structures with multiple holes and refinement settings.
        /// Validates that the mesher can handle intricate geometries with multiple exclusion zones.
        /// </summary>
        [Fact]
        public void MultipleHolesWithRefinement()
        {
            // Arrange
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(6, 3),
                new Vec2(6, 5), new Vec2(8, 5), new Vec2(8, 8), new Vec2(0, 8)
            });
            var hole1 = Polygon2D.FromPoints(new[] { new Vec2(1, 1), new Vec2(2, 1), new Vec2(2, 2), new Vec2(1, 2) });
            var hole2 = Polygon2D.FromPoints(new[] { new Vec2(6.5, 1), new Vec2(7.5, 1), new Vec2(7.5, 2), new Vec2(6.5, 2) });
            var hole3 = Polygon2D.FromPoints(new[] { new Vec2(2, 6), new Vec2(3, 6), new Vec2(3, 7), new Vec2(2, 7) });
            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole1).AddHole(hole2).AddHole(hole3);

            // V2.0: Use builder pattern with Result validation
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.75)
                .WithTargetEdgeLengthZ(1.0)
                .WithMinCapQuadQuality(0.95)
                .WithRejectedCapTriangles(true)
                .Build()
                .UnwrapForTests(); // V2.0 compatibility extension

            // Act
            var result = new PrismMesher().Mesh(structure, options).UnwrapForTests(); // V2.0 pattern
            var indexed = IndexedMesh.FromMesh(result);

            // Assert
            indexed.VertexCount.Should().BeGreaterThan(50);
            indexed.QuadCount.Should().BeGreaterThan(30);
            indexed.TriangleCount.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Tests handling of very small notches in geometry that could cause numerical issues.
        /// Validates robustness of the meshing algorithm against edge cases in geometric input.
        /// </summary>
        [Fact]
        public void TinyNotchHandling()
        {
            // Arrange
            var vertices = new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 2.5), new Vec2(5.01, 2.5), new Vec2(5.01, 2.51), new Vec2(5, 2.51), new Vec2(5, 5), new Vec2(0, 5) };
            var outer = Polygon2D.FromPoints(vertices);
            var structure = new PrismStructureDefinition(outer, 0, 1);

            // V2.0: Use builder pattern with Result validation  
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.5)
                .WithMinCapQuadQuality(0.1)
                .Build()
                .UnwrapForTests(); // V2.0 compatibility extension

            // Act
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests(); // V2.0 pattern

            // Assert
            mesh.QuadCount.Should().BeGreaterThan(5);
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(10);
        }

        /// <summary>
        /// Tests high refinement settings as a stress test for the meshing algorithm.
        /// Validates performance and correctness under demanding mesh density requirements.
        /// </summary>
        [Fact]
        public void HighRefinementStressTest()
        {
            // Arrange
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(0.8, 0.8), new Vec2(1.2, 0.8), new Vec2(1.2, 1.2), new Vec2(0.8, 1.2) });
            var structure = new PrismStructureDefinition(outer, 0, 0.5).AddHole(hole).AddConstraintSegment(new Segment2D(new Vec2(0, 1), new Vec2(2, 1)), 0.25);

            // V2.0: Use builder pattern with Result validation
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.05)
                .WithTargetEdgeLengthZ(0.05)
                .WithHoleRefinement(0.02, 0.5)
                .WithSegmentRefinement(0.02, 0.3)
                .WithMinCapQuadQuality(0.2)
                .Build()
                .UnwrapForTests(); // V2.0 compatibility extension

            // Act
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests(); // V2.0 pattern
            var indexed = IndexedMesh.FromMesh(mesh);

            // Assert
            indexed.VertexCount.Should().BeGreaterThan(100);
            indexed.QuadCount.Should().BeGreaterThan(80);
        }

        /// <summary>
        /// Tests L-shaped geometry with internal surfaces to validate complex structural meshing.
        /// Validates handling of non-convex shapes combined with internal horizontal surfaces.
        /// </summary>
        [Fact]
        public void LShapedGeometryWithInternalSurface()
        {
            // Arrange
            var lShape = new[] { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 2), new Vec2(1, 2), new Vec2(1, 4), new Vec2(0, 4) };
            var outer = Polygon2D.FromPoints(lShape);
            var structure = new PrismStructureDefinition(outer, -1, 3);
            var internalOutline = Polygon2D.FromPoints(new[] { new Vec2(0.5, 0.5), new Vec2(2.5, 0.5), new Vec2(2.5, 1.5), new Vec2(0.5, 1.5) });
            structure = structure.AddInternalSurface(internalOutline, 1.0);

            // V2.0: Use builder pattern with Result validation
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.4)
                .WithTargetEdgeLengthZ(0.8)
                .Build()
                .UnwrapForTests(); // V2.0 compatibility extension

            // Act
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests(); // V2.0 pattern
            var indexed = IndexedMesh.FromMesh(mesh);

            // Assert
            indexed.VertexCount.Should().BeGreaterThan(30);
            indexed.QuadCount.Should().BeGreaterThan(25);
            indexed.Vertices.Any(v => Math.Abs(v.Z - 1.0) < 0.001).Should().BeTrue();
        }

        /// <summary>
        /// Tests star-shaped polygon with hole to validate complex concave geometry handling.
        /// Validates meshing of intricate shapes that challenge standard tessellation algorithms.
        /// </summary>
        [Fact]
        public void StarShapedPolygonWithHole()
        {
            // Arrange
            var starVertices = new Vec2[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = 2 * Math.PI * i / 10;
                double r = (i % 2 == 0) ? 3.0 : 1.5;
                starVertices[i] = new Vec2(r * Math.Cos(angle), r * Math.Sin(angle));
            }
            var outer = Polygon2D.FromPoints(starVertices);

            var holeVertices = new Vec2[8];
            for (int i = 0; i < 8; i++)
            {
                double angle = 2 * Math.PI * i / 8;
                holeVertices[i] = new Vec2(0.3 * Math.Cos(angle), 0.3 * Math.Sin(angle));
            }
            var hole = Polygon2D.FromPoints(holeVertices);
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);

            // V2.0: Use builder pattern with Result validation
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.3)
                .WithTargetEdgeLengthZ(0.5)
                .WithMinCapQuadQuality(0.1)
                .WithRejectedCapTriangles(true)
                .Build()
                .UnwrapForTests(); // V2.0 compatibility extension

            // Act
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests(); // V2.0 pattern

            // Assert
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(20);
            (indexed.QuadCount + indexed.TriangleCount).Should().BeGreaterThan(15);
        }
    }
}
