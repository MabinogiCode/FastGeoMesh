#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Tests for asynchronous meshing capabilities introduced in v1.4.0.
    /// </summary>
    public class AsyncMeshingTests
    {
        private readonly PrismMesher _mesher;
        private readonly MesherOptions _options;

        public AsyncMeshingTests()
        {
            _mesher = new PrismMesher();
            _options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build();
        }

        [Fact]
        public async Task MeshAsync_SimpleStructure_ProducesSameResultAsSync()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            // Act
            var syncMesh = _mesher.Mesh(structure, _options);
            var asyncMesh = await _mesher.MeshAsync(structure, _options);

            // Assert
            asyncMesh.Should().NotBeNull();
            asyncMesh.QuadCount.Should().Be(syncMesh.QuadCount);
            asyncMesh.TriangleCount.Should().Be(syncMesh.TriangleCount);
            asyncMesh.Points.Count.Should().Be(syncMesh.Points.Count);
        }

        [Fact]
        public async Task MeshAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            using var cts = new CancellationTokenSource();

            // Act & Assert
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _mesher.MeshAsync(structure, _options, cts.Token).AsTask());
        }

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
                () => _mesher.MeshAsync(structure, _options, cts.Token).AsTask());
        }

        [Fact]
        public async Task MeshWithProgressAsyncProperlyHandlesCancellation()
        {
            // Arrange - Create a more complex structure for realistic cancellation testing
            var vertices = new List<Vec2>();
            for (int i = 0; i < 50; i++)
            {
                double angle = 2 * Math.PI * i / 50;
                vertices.Add(new Vec2(Math.Cos(angle) * 10, Math.Sin(angle) * 10));
            }
            var structure = new PrismStructureDefinition(new Polygon2D(vertices), -5, 5);
            
            using var cts = new CancellationTokenSource();
            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => {
                progressReports.Add(p);
                // Cancel after first progress report
                if (progressReports.Count == 2)
                {
                    cts.Cancel();
                }
            });

            // Act & Assert
            var asyncMesher = (IAsyncMesher)_mesher;
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => asyncMesher.MeshWithProgressAsync(structure, _options, progress, cts.Token).AsTask());

            // Verify that progress was reported before cancellation
            progressReports.Should().NotBeEmpty();
            progressReports.Should().HaveCountLessThan(10, "Operation should be cancelled early");
        }

        [Fact]
        public async Task MeshBatchAsyncRespectsCancellationToken()
        {
            // Arrange - Create a batch that will take some time
            var structures = Enumerable.Range(0, 20).Select(i =>
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2)
            ).ToArray();

            using var cts = new CancellationTokenSource();
            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => {
                progressReports.Add(p);
                // Cancel after processing a few structures
                if (progressReports.Count >= 3)
                {
                    cts.Cancel();
                }
            });

            // Act & Assert
            var asyncMesher = (IAsyncMesher)_mesher;
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => asyncMesher.MeshBatchAsync(structures, _options, progress: progress, cancellationToken: cts.Token).AsTask());

            // Verify partial progress
            progressReports.Should().NotBeEmpty();
            progressReports.Should().HaveCountLessThan(structures.Length, "Batch should be cancelled before completion");
        }

        [Fact]
        public async Task MeshAsync_WithProgress_ReportsProgress()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));

            // Act
            var asyncMesher = (IAsyncMesher)_mesher;
            var mesh = await asyncMesher.MeshWithProgressAsync(structure, _options, progress);

            // Assert
            mesh.Should().NotBeNull();
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(p => p.Operation == "Initializing");
            progressReports.Should().Contain(p => p.Percentage >= 1.0);
        }

        [Fact]
        public async Task MeshBatchAsync_MultipleStructures_ProcessesAllStructures()
        {
            // Arrange
            var structures = new[]
            {
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2),
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(0, 3) }),
                    -1, 3),
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(12, 0), new Vec2(12, 4), new Vec2(0, 4) }),
                    1, 4)
            };

            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));

            // Act
            var asyncMesher = (IAsyncMesher)_mesher;
            var meshes = await asyncMesher.MeshBatchAsync(structures, _options, progress: progress);

            // Assert
            meshes.Should().HaveCount(3);
            meshes.Should().AllSatisfy(mesh =>
            {
                mesh.Should().NotBeNull();
                mesh.QuadCount.Should().BeGreaterThan(0);
            });

            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(p => p.Operation == "Batch Processing");
        }

        [Fact]
        public async Task MeshBatchAsync_WithParallelism_LimitsParallelism()
        {
            // Arrange
            var structures = Enumerable.Range(0, 10).Select(i =>
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2)
            ).ToArray();

            // Act
            var asyncMesher = (IAsyncMesher)_mesher;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var meshes = await asyncMesher.MeshBatchAsync(structures, _options, maxDegreeOfParallelism: 2);
            stopwatch.Stop();

            // Assert
            meshes.Should().HaveCount(10);
            meshes.Should().AllSatisfy(mesh => mesh.Should().NotBeNull());

            // Performance should be better than sequential but not unlimited parallel
            // This is more of a smoke test - exact timing depends on hardware
            stopwatch.ElapsedMilliseconds.Should().BeLessThan((long)CIEnvironmentHelper.AdjustThreshold(5000));
        }

        [Fact]
        public async Task EstimateComplexityAsync_SimpleStructure_ReturnsReasonableEstimate()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            // Act
            var asyncMesher = (IAsyncMesher)_mesher;
            var estimate = await asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.EstimatedQuadCount.Should().BeGreaterThan(0);
            estimate.EstimatedTriangleCount.Should().BeGreaterThanOrEqualTo(0);
            estimate.EstimatedPeakMemoryBytes.Should().BeGreaterThan(0);
            estimate.EstimatedComputationTime.Should().BeGreaterThan(TimeSpan.Zero);
            estimate.Complexity.Should().BeOneOf(MeshingComplexity.Trivial, MeshingComplexity.Simple);
            estimate.RecommendedParallelism.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task EstimateComplexityAsync_ComplexStructure_ReturnsHigherComplexity()
        {
            // Arrange - Create a complex structure with many vertices and holes
            var vertices = new List<Vec2>();
            for (int i = 0; i < 100; i++)
            {
                double angle = 2 * Math.PI * i / 100;
                vertices.Add(new Vec2(Math.Cos(angle) * 10, Math.Sin(angle) * 10));
            }
            var polygon = new Polygon2D(vertices);

            var hole = Polygon2D.FromPoints(new[] { new Vec2(-2, -2), new Vec2(2, -2), new Vec2(2, 2), new Vec2(-2, 2) });

            var structure = new PrismStructureDefinition(polygon, 0, 2).AddHole(hole);

            // Act
            var asyncMesher = (IAsyncMesher)_mesher;
            var estimate = await asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.Complexity.Should().BeOneOf(MeshingComplexity.Moderate, MeshingComplexity.Complex, MeshingComplexity.Extreme);
            estimate.EstimatedQuadCount.Should().BeGreaterThan(50); // Should be more complex
            estimate.RecommendedParallelism.Should().BeGreaterThanOrEqualTo(1);
            estimate.PerformanceHints.Should().NotBeEmpty();
        }

        [Fact]
        public void MeshingProgress_FromCounts_CalculatesPercentageCorrectly()
        {
            // Act
            var progress = MeshingProgress.FromCounts("Test Operation", 25, 100, "Processing");

            // Assert
            progress.Operation.Should().Be("Test Operation");
            progress.Percentage.Should().BeApproximately(0.25, 0.001);
            progress.ProcessedElements.Should().Be(25);
            progress.TotalElements.Should().Be(100);
            progress.StatusMessage.Should().Be("Processing");
        }

        [Fact]
        public void MeshingProgress_Completed_ReturnsCompletedProgress()
        {
            // Act
            var progress = MeshingProgress.Completed("Test Operation", 150);

            // Assert
            progress.Operation.Should().Be("Test Operation");
            progress.Percentage.Should().Be(1.0);
            progress.ProcessedElements.Should().Be(150);
            progress.TotalElements.Should().Be(150);
            progress.StatusMessage.Should().Be("Completed");
        }

        [Fact]
        public void MeshingProgress_ToString_ReturnsFormattedString()
        {
            // Arrange
            var progress = new MeshingProgress(
                "Test Operation",
                0.456,
                456,
                1000,
                TimeSpan.FromMinutes(2.5),
                "In progress");

            // Act
            var result = progress.ToString();

            // Assert
            result.Should().Contain("Test Operation");
            result.Should().Contain("45.6%");
            result.Should().Contain("456/1000");
            result.Should().Contain("ETA:");
            result.Should().Contain("In progress");
        }

        [Theory]
        [InlineData(MeshingComplexity.Trivial)]
        [InlineData(MeshingComplexity.Simple)]
        [InlineData(MeshingComplexity.Moderate)]
        [InlineData(MeshingComplexity.Complex)]
        [InlineData(MeshingComplexity.Extreme)]
        public void MeshingComplexityEstimate_ToString_ReturnsFormattedString(MeshingComplexity complexity)
        {
            // Arrange
            var estimate = new MeshingComplexityEstimate(
                100, 50, 1024 * 1024, TimeSpan.FromMilliseconds(500), 2, complexity);

            // Act
            var result = estimate.ToString();

            // Assert
            result.Should().Contain(complexity.ToString());
            result.Should().Contain("150 elements"); // 100 + 50
            result.Should().Contain("MB");
            result.Should().Contain("500ms");
        }
    }
}
