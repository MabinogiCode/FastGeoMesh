using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Tests for class V2ValidationTests.
    /// </summary>
    public sealed class V2ValidationTests
    {
        /// <summary>
        /// Runs test V2ResultPatternOptionsValidationWorks.
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
        /// Runs test V2ResultPatternMeshingWorks.
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
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
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
        /// Runs test V2AsyncPatternWorks.
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
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var asyncMesher = (IAsyncMesher)mesher;
            var asyncResult = await asyncMesher.MeshAsync(structure, options).ConfigureAwait(false);
            // Assert
            asyncResult.IsSuccess.Should().BeTrue();
            asyncResult.Value.Should().NotBeNull();
            asyncResult.Value.QuadCount.Should().BeGreaterThan(0);
        }
        /// <summary>
        /// Runs test V2CleanArchitectureLayerSeparationWorks.
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
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            // Verify types are accessible
            vec2.Should().NotBeNull();
            vec3.Should().NotBeNull();
            polygon.Should().NotBeNull();
            structure.Should().NotBeNull();
            mesher.Should().NotBeNull();
        }
        /// <summary>
        /// Runs test V2ComplexStructureWithHoleWorks.
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
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var meshResult = mesher.Mesh(structure, options);
            // Assert
            meshResult.IsSuccess.Should().BeTrue();
            meshResult.Value.QuadCount.Should().BeGreaterThan(10); // Should have reasonable number of quads
            var indexed = IndexedMesh.FromMesh(meshResult.Value);
            indexed.VertexCount.Should().BeGreaterThan(16); // More vertices due to hole refinement
        }
    }
}
