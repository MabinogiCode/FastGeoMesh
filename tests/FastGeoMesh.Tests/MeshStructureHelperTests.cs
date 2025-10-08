using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for MeshStructureHelper functions.</summary>
    public sealed class MeshStructureHelperTests
    {
        /// <summary>
        /// Tests that BuildZLevels creates the correct number of Z levels based on target edge length.
        /// Validates vertical discretization logic for prism structures.
        /// </summary>
        [Fact]
        public void BuildZLevelsCreatesCorrectNumberOfLevels()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 10);
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(2.0) };

            // Act
            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);

            // Assert
            levels.Should().NotBeEmpty();
            levels[0].Should().Be(0, "First level should be z0");
            levels[^1].Should().Be(10, "Last level should be z1");
            levels.Should().HaveCountGreaterThan(2, "Should have intermediate levels");
            levels.Should().BeInAscendingOrder("Levels should be sorted");
        }

        /// <summary>
        /// Tests that BuildZLevels properly includes constraint segment levels in the Z discretization.
        /// Validates integration of constraint geometry into vertical level generation.
        /// </summary>
        [Fact]
        public void BuildZLevelsIncludesConstraintSegmentLevels()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 10);
            // FIXED: AddConstraintSegment returns new immutable instance - must reassign
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(1, 0)), 3.5);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 1), new Vec2(1, 1)), 7.2);
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(5.0) };

            // Act
            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);

            // Assert
            levels.Should().Contain(3.5, "Should include first constraint level");
            levels.Should().Contain(7.2, "Should include second constraint level");
        }

        /// <summary>
        /// Tests that BuildZLevels properly includes geometry points in the Z discretization.
        /// Validates integration of auxiliary geometry points into vertical level generation.
        /// </summary>
        [Fact]
        public void BuildZLevelsIncludesGeometryPoints()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 10);
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 2.5));
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 8.1));
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(5.0) };

            // Act
            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);

            // Assert
            levels.Should().Contain(2.5, "Should include geometry point Z level");
            levels.Should().Contain(8.1, "Should include second geometry point Z level");
        }

        /// <summary>
        /// Tests that IsNearAnyHole correctly detects proximity to hole boundaries.
        /// Validates spatial proximity detection for mesh refinement near hole edges.
        /// </summary>
        [Fact]
        public void IsNearAnyHoleDetectsProximityCorrectly()
        {
            // Arrange
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);

            // Act & Assert
            MeshStructureHelper.IsNearAnyHole(structure, 5, 5, 0.5).Should().BeFalse("Point inside hole not near boundary");
            MeshStructureHelper.IsNearAnyHole(structure, 3.5, 5, 0.6).Should().BeTrue("Point near hole edge");
            MeshStructureHelper.IsNearAnyHole(structure, 1, 1, 0.5).Should().BeFalse("Point far from hole");
        }

        /// <summary>
        /// Tests that IsNearAnySegment correctly detects proximity to internal geometry segments.
        /// Validates spatial proximity detection for mesh refinement near constraint segments.
        /// </summary>
        [Fact]
        public void IsNearAnySegmentDetectsProximityCorrectly()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(polygon, 0, 5);
            structure.Geometry.AddSegment(new Segment3D(new Vec3(2, 2, 1), new Vec3(8, 2, 1)));

            // Act & Assert
            MeshStructureHelper.IsNearAnySegment(structure, 5, 2, 0.1).Should().BeTrue("Point on segment");
            MeshStructureHelper.IsNearAnySegment(structure, 5, 2.5, 0.6).Should().BeTrue("Point near segment");
            MeshStructureHelper.IsNearAnySegment(structure, 5, 8, 1.0).Should().BeFalse("Point far from segment");
        }

        /// <summary>
        /// Tests that IsInsideAnyHole with spatial indices works correctly for optimized hole containment queries.
        /// Validates performance-optimized spatial indexing for hole containment testing.
        /// </summary>
        [Fact]
        public void IsInsideAnyHoleWithSpatialIndicesWorks()
        {
            // Arrange
            var hole1 = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var hole2 = Polygon2D.FromPoints(new[] { new Vec2(6, 6), new Vec2(8, 6), new Vec2(8, 8), new Vec2(6, 8) });

            var holeIndices = new SpatialPolygonIndex[]
            {
                new(hole1.Vertices),
                new(hole2.Vertices)
            };

            // Act & Assert
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 3, 3).Should().BeTrue("Point inside first hole");
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 7, 7).Should().BeTrue("Point inside second hole");
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 1, 1).Should().BeFalse("Point outside all holes");
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 5, 5).Should().BeFalse("Point between holes");
        }

        /// <summary>
        /// Tests that IsInsideAnyHole with structure works correctly for basic hole containment queries.
        /// Validates basic hole containment testing without spatial optimization.
        /// </summary>
        [Fact]
        public void IsInsideAnyHoleWithStructureWorks()
        {
            // Arrange
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);

            // Act & Assert
            MeshStructureHelper.IsInsideAnyHole(structure, 5, 5).Should().BeTrue("Point inside hole");
            MeshStructureHelper.IsInsideAnyHole(structure, 1, 1).Should().BeFalse("Point outside hole");
            MeshStructureHelper.IsInsideAnyHole(structure, 4, 4).Should().BeTrue("Point on hole corner");
        }

        /// <summary>
        /// Tests that BuildZLevels correctly excludes points outside the specified Z range.
        /// Validates filtering logic to ensure only relevant Z levels are included in mesh generation.
        /// </summary>
        [Fact]
        public void BuildZLevelsExcludesPointsOutsideRange()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 5, 15);

            // Add points outside the Z range - should be excluded
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 2));   // Below range
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 18));  // Above range
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 10));  // Inside range

            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(5.0), Epsilon = Tolerance.From(1e-6) };

            // Act
            var levels = MeshStructureHelper.BuildZLevels(5, 15, options, structure);

            // Assert
            levels.Should().Contain(10, "Should include point inside range");
            levels.Should().NotContain(2, "Should exclude point below range");
            levels.Should().NotContain(18, "Should exclude point above range");
        }
    }
}
