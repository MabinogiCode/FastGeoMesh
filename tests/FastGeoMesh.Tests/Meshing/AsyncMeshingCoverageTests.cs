using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Domain.Factories;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Tests for class AsyncMeshingCoverageTests.
    /// </summary>
    public sealed class AsyncMeshingCoverageTests
    {
        private readonly IPrismMesher _mesher;
        private readonly IAsyncMesher _asyncMesher;
        private readonly MesherOptions _options;
        /// <summary>
        /// Runs test AsyncMeshingCoverageTests.
        /// </summary>
        public AsyncMeshingCoverageTests()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            _mesher = provider.GetRequiredService<IPrismMesher>();
            _asyncMesher = (IAsyncMesher)_mesher;
            _options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
        }
        /// <summary>
        /// Runs test EstimateComplexityAsyncCategorizesDifferentSizesCorrectly.
        /// </summary>
        [Theory]
        [InlineData(3, MeshingComplexity.Trivial)]    // < 10 vertices
        [InlineData(25, MeshingComplexity.Simple)]    // < 50 vertices
        [InlineData(100, MeshingComplexity.Moderate)] // < 200 vertices
        [InlineData(500, MeshingComplexity.Complex)]  // < 1000 vertices
        [InlineData(1500, MeshingComplexity.Extreme)] // >= 1000 vertices
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
            var estimate = await _asyncMesher.EstimateComplexityAsync(structure, _options).ConfigureAwait(true);

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
        /// Runs test MeshAsyncSimpleStructureProducesSameResultAsSync.
        /// </summary>
        [Fact]
        public async Task MeshAsyncSimpleStructureProducesSameResultAsSync()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            // Act
            var syncMesh = _mesher.Mesh(structure, _options).UnwrapForTests();
            var asyncMesh = await _asyncMesher.MeshAsync(structure, _options).ConfigureAwait(true);

            // Assert
            asyncMesh.Value.Should().NotBeNull();
            asyncMesh.Value.QuadCount.Should().Be(syncMesh.QuadCount);
            asyncMesh.Value.TriangleCount.Should().Be(syncMesh.TriangleCount);
            asyncMesh.Value.Points.Count.Should().Be(syncMesh.Points.Count);
        }
        /// <summary>
        /// Runs test MeshAsyncWithCancellationThrowsOperationCanceledException.
        /// </summary>
        [Fact]
        public async Task MeshAsyncWithCancellationThrowsOperationCanceledException()
        {
            // Arrange
            var polygon = Polygon2DFactory.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            using var cts = new CancellationTokenSource();

            // Act
            cts.Cancel();
            var result = await _asyncMesher.MeshAsync(structure, _options, cts.Token).ConfigureAwait(true);

            // Assert - MeshAsync uses Result Pattern instead of throwing
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.Cancelled");
        }
    }
}
