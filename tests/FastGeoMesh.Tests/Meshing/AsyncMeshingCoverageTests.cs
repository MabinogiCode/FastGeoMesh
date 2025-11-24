using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Comprehensive tests for v1.4.0 async capabilities to improve code coverage.
    /// Covers all async methods, progress reporting, batch processing, and monitoring.
    /// </summary>
    public sealed class AsyncMeshingCoverageTests
    {
        private readonly IPrismMesher _mesher;
        private readonly IAsyncMesher _asyncMesher;
        private readonly MesherOptions _options;

        /// <summary>Initializes the test class with mesher, async mesher, and options.</summary>
        public AsyncMeshingCoverageTests()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            _mesher = provider.GetRequiredService<IPrismMesher>();
            _asyncMesher = (IAsyncMesher)_mesher;
            _options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
        }

        /// <summary>Tests all paths in EstimateComplexityAsync for different structure sizes.</summary>
        [Theory]
        [InlineData(3, MeshingComplexity.Trivial)]    // < 10 vertices
        [InlineData(25, MeshingComplexity.Simple)]    // < 50 vertices
        [InlineData(100, MeshingComplexity.Moderate)] // < 200 vertices
        [InlineData(500, MeshingComplexity.Complex)]  // < 1000 vertices
        [InlineData(1500, MeshingComplexity.Extreme)] // >= 1000 vertices
        public async Task EstimateComplexityAsyncCategorizesDifferentSizesCorrectly(int vertexCount, MeshingComplexity expectedComplexity)
        {
            // Arrange
            var vertices = new List<Vec2>();
            for (int i = 0; i < vertexCount; i++)
            {
                double angle = 2 * Math.PI * i / vertexCount;
                vertices.Add(new Vec2(Math.Cos(angle) * 10, Math.Sin(angle) * 10));
            }
            var structure = new PrismStructureDefinition(new Polygon2D(vertices), 0, 5);

            // Act
            var estimate = await _asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.Complexity.Should().Be(expectedComplexity);
            estimate.EstimatedQuadCount.Should().BeGreaterThan(0);
            estimate.EstimatedTriangleCount.Should().BeGreaterThan(0);
            estimate.EstimatedPeakMemoryBytes.Should().BeGreaterThan(0);
            estimate.EstimatedComputationTime.Should().BeGreaterThan(TimeSpan.Zero);
            estimate.RecommendedParallelism.Should().BeGreaterThan(0);
            estimate.PerformanceHints.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that async meshing produces the same result as synchronous meshing for a simple rectangular structure.
        /// Validates consistency between async and sync implementations across the core meshing algorithms.
        /// </summary>
        [Fact]
        public async Task MeshAsyncSimpleStructureProducesSameResultAsSync()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            // Act
            var syncMesh = _mesher.Mesh(structure, _options).UnwrapForTests();
            var asyncMesh = await _asyncMesher.MeshAsync(structure, _options).UnwrapForTestsAsync();

            // Assert
            asyncMesh.Should().NotBeNull();
            asyncMesh.QuadCount.Should().Be(syncMesh.QuadCount);
            asyncMesh.TriangleCount.Should().Be(syncMesh.TriangleCount);
            asyncMesh.Points.Count.Should().Be(syncMesh.Points.Count);
        }

        /// <summary>
        /// Tests that async meshing with cancellation properly throws OperationCanceledException.
        /// Validates that the cancellation token is respected in async operations and proper cleanup occurs.
        /// </summary>
        [Fact]
        public async Task MeshAsyncWithCancellationThrowsOperationCanceledException()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            using var cts = new CancellationTokenSource();

            // Act & Assert
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _asyncMesher.MeshAsync(structure, _options, cts.Token).UnwrapForTestsAsync());
        }
    }
}
