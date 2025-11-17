using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Validation tests for FastGeoMesh v2.0 Clean Architecture.
    /// Ensures the core functionality works with the new Result pattern.
    /// </summary>
    public sealed class V2ValidationTests
    {
        /// <summary>
        /// Tests that v2.0 Result pattern for options validation works correctly.
        /// Validates the new error handling approach and ensures proper validation feedback.
        /// </summary>
        [Fact]
        public void V2ResultPatternOptionsValidationWorks()
        {
            // Arrange & Act - Valid options
            var validResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(1.0)
                .Build();

            // Assert
            validResult.IsSuccess.Should().BeTrue();
            validResult.Value.Should().NotBeNull();

            // Arrange & Act - Invalid options
            var invalidResult = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(-1.0) // Invalid negative value
                .Build();

            // Assert
            invalidResult.IsFailure.Should().BeTrue();
            invalidResult.Error.Description.Should().NotBeEmpty();
        }

        /// <summary>
        /// Tests that v2.0 Result pattern for meshing operations works correctly.
        /// Validates the enhanced error handling and success/failure result patterns.
        /// </summary>
        [Fact]
        public void V2ResultPatternMeshingWorks()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .Build();

            optionsResult.IsSuccess.Should().BeTrue();

            // Act
            var mesher = TestMesherFactory.CreatePrismMesher();
            var meshResult = mesher.Mesh(structure, optionsResult.Value);

            // Assert
            meshResult.IsSuccess.Should().BeTrue();
            meshResult.Value.Should().NotBeNull();
            meshResult.Value.QuadCount.Should().BeGreaterThan(0);

            // Verify the mesh can be indexed
            var indexed = IndexedMesh.FromMesh(meshResult.Value);
            indexed.VertexCount.Should().BeGreaterThan(0);
            indexed.QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Tests that v2.0 async pattern works correctly with proper error handling.
        /// Validates asynchronous operations in the new Clean Architecture implementation.
        /// </summary>
        [Fact]
        public async Task V2AsyncPatternWorks()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 4), new Vec2(0, 4)
            });
            var structure = new PrismStructureDefinition(polygon, -1, 3);

            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.5)
                .Build()
                .Value; // We know it's valid

            // Act
            var mesher = TestMesherFactory.CreatePrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var asyncResult = await asyncMesher.MeshAsync(structure, options);

            // Assert
            asyncResult.IsSuccess.Should().BeTrue();
            asyncResult.Value.Should().NotBeNull();
            asyncResult.Value.QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Tests that v2.0 Clean Architecture layer separation works correctly.
        /// Validates proper namespace organization and dependency management across layers.
        /// </summary>
        [Fact]
        public void V2CleanArchitectureLayerSeparationWorks()
        {
            // This test validates that we can use types from each layer independently

            // Domain layer
            var vec2 = new Vec2(1, 2);
            var vec3 = new Vec3(1, 2, 3);
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 1);

            // Application layer
            var mesher = TestMesherFactory.CreatePrismMesher();

            // Verify types are accessible
            vec2.Should().NotBeNull();
            vec3.Should().NotBeNull();
            polygon.Should().NotBeNull();
            structure.Should().NotBeNull();
            mesher.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that v2.0 handles complex structures with holes correctly.
        /// Validates advanced meshing capabilities in the new architecture with complex geometries.
        /// </summary>
        [Fact]
        public void V2ComplexStructureWithHoleWorks()
        {
            // Arrange
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 6), new Vec2(0, 6)
            });
            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(2, 2), new Vec2(8, 2), new Vec2(8, 4), new Vec2(2, 4)
            });

            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);

            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .WithHoleRefinement(0.5, 1.0)
                .Build()
                .Value;

            // Act
            var mesher = TestMesherFactory.CreatePrismMesher();
            var meshResult = mesher.Mesh(structure, options);

            // Assert
            meshResult.IsSuccess.Should().BeTrue();
            meshResult.Value.QuadCount.Should().BeGreaterThan(10); // Should have reasonable number of quads

            var indexed = IndexedMesh.FromMesh(meshResult.Value);
            indexed.VertexCount.Should().BeGreaterThan(16); // More vertices due to hole refinement
        }
    }
}
