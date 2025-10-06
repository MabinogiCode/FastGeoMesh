using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Critical validation tests for v1.4.0 release candidate.
    /// </summary>
    public sealed class V14CriticalValidationTests
    {
        /// <summary>Tests that basic sync meshing functionality works correctly.</summary>
        [Fact]
        public void PrismMesher_BasicFunctionality_Works()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            
            // Add auxiliary geometry to ensure Points collection is populated
            structure.Geometry.AddPoint(new Vec3(5, 2.5, 1));
            
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var mesher = new PrismMesher();

            // Act
            var mesh = mesher.Mesh(structure, options);

            // Assert
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
            mesh.Points.Should().NotBeEmpty();
        }

        /// <summary>Tests that basic async meshing functionality works correctly.</summary>
        [Fact]
        public async Task AsyncMesher_BasicFunctionality_Works()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            
            // Add auxiliary geometry to ensure Points collection is populated
            structure.Geometry.AddPoint(new Vec3(5, 2.5, 1));
            
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Act
            var mesh = await asyncMesher.MeshAsync(structure, options);

            // Assert
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
            mesh.Points.Should().NotBeEmpty();
        }

        /// <summary>Tests that async meshing properly handles cancellation tokens.</summary>
        [Fact]
        public async Task AsyncMesher_WithCancellation_ThrowsCorrectly()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => asyncMesher.MeshAsync(structure, options, cts.Token).AsTask());
        }

        /// <summary>Tests that performance monitoring functionality works without errors.</summary>
        [Fact]
        public async Task PerformanceMonitoring_Works()
        {
            // Arrange
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Act
            var stats = await asyncMesher.GetLivePerformanceStatsAsync();

            // Assert
            stats.Should().NotBeNull();
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
        }

        /// <summary>Tests that complexity estimation functionality works correctly.</summary>
        [Fact]
        public async Task ComplexityEstimation_Works()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Act
            var estimate = await asyncMesher.EstimateComplexityAsync(structure, options);

            // Assert
            estimate.Should().NotBeNull();
            estimate.EstimatedQuadCount.Should().BeGreaterThan(0);
            estimate.EstimatedTriangleCount.Should().BeGreaterThan(0);
            estimate.EstimatedComputationTime.Should().BeGreaterThan(TimeSpan.Zero);
            estimate.Complexity.Should().BeOneOf(MeshingComplexity.Trivial, MeshingComplexity.Simple); // Accept both valid complexities
        }
    }
}
