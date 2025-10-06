using System.Diagnostics;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Comprehensive tests for v1.4.0 async capabilities to improve code coverage.
    /// Covers all async methods, progress reporting, batch processing, and monitoring.
    /// </summary>
    public sealed class AsyncMeshingCoverageTests
    {
        private readonly PrismMesher _mesher;
        private readonly IAsyncMesher _asyncMesher;
        private readonly MesherOptions _options;

        /// <summary>Initializes the test class with mesher, async mesher, and options.</summary>
        public AsyncMeshingCoverageTests()
        {
            _mesher = new PrismMesher();
            _asyncMesher = (IAsyncMesher)_mesher;
            _options = MesherOptions.CreateBuilder().WithFastPreset().Build();
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

        /// <summary>Tests complexity estimation with holes to cover hole-specific paths.</summary>
        [Fact]
        public async Task EstimateComplexityAsyncWithManyHolesGeneratesHoleHint()
        {
            // Arrange - Create structure with 6 holes (> 5 triggers hint)
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 20), new Vec2(0, 20)
            });
            var structure = new PrismStructureDefinition(outer, 0, 5);

            for (int i = 0; i < 6; i++)
            {
                var x = 3 + (i % 3) * 6;
                var y = 3 + (i / 3) * 6;
                var hole = Polygon2D.FromPoints(new[]
                {
                    new Vec2(x, y), new Vec2(x+2, y), new Vec2(x+2, y+2), new Vec2(x, y+2)
                });
                structure = structure.AddHole(hole);
            }

            // Act
            var estimate = await _asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.PerformanceHints.Should().Contain(hint =>
                hint.Contains("Large number of holes detected"));
        }

        /// <summary>Tests complexity estimation for large geometry to cover size hint path.</summary>
        [Fact]
        public async Task EstimateComplexityAsyncWithLargeGeometryGeneratesSizeHint()
        {
            // Arrange - Create structure with > 500 vertices
            var vertices = new List<Vec2>();
            for (int i = 0; i < 600; i++)
            {
                double angle = 2 * Math.PI * i / 600;
                vertices.Add(new Vec2(Math.Cos(angle) * 10, Math.Sin(angle) * 10));
            }
            var structure = new PrismStructureDefinition(new Polygon2D(vertices), 0, 5);

            // Act
            var estimate = await _asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.PerformanceHints.Should().Contain(hint =>
                hint.Contains("Large geometry detected"));
        }

        /// <summary>Tests trivial structure hint generation.</summary>
        [Fact]
        public async Task EstimateComplexityAsyncWithTrivialStructureGeneratesTrivialHint()
        {
            // Arrange - Create trivial structure (< 10 vertices)
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2)
                }), 0, 1);

            // Act
            var estimate = await _asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.PerformanceHints.Should().Contain(hint =>
                hint.Contains("Simple geometry - synchronous processing is optimal"));
        }

        /// <summary>Tests complex structure parallelism recommendation.</summary>
        [Fact]
        public async Task EstimateComplexityAsyncWithComplexStructureRecommendParallelism()
        {
            // Arrange - Create complex structure
            var vertices = new List<Vec2>();
            for (int i = 0; i < 800; i++)
            {
                double angle = 2 * Math.PI * i / 800;
                vertices.Add(new Vec2(Math.Cos(angle) * 10, Math.Sin(angle) * 10));
            }
            var structure = new PrismStructureDefinition(new Polygon2D(vertices), 0, 5);

            // Act
            var estimate = await _asyncMesher.EstimateComplexityAsync(structure, _options);

            // Assert
            estimate.RecommendedParallelism.Should().BeGreaterThan(1);
            estimate.PerformanceHints.Should().Contain(hint =>
                hint.Contains("Consider using parallel batch processing"));
        }

        /// <summary>Tests GetLivePerformanceStatsAsync functionality.</summary>
        [Fact]
        public async Task GetLivePerformanceStatsAsyncReturnsValidStats()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            var statsBefore = await _asyncMesher.GetLivePerformanceStatsAsync();

            // Act
            _ = await _asyncMesher.MeshAsync(structure, _options);
            var statsAfter = await _asyncMesher.GetLivePerformanceStatsAsync();

            // Assert
            statsAfter.MeshingOperations.Should().BeGreaterThan(statsBefore.MeshingOperations);
            statsAfter.QuadsGenerated.Should().BeGreaterThan(statsBefore.QuadsGenerated);
            statsAfter.PoolHitRate.Should().BeInRange(0.0, 1.0);
        }

        /// <summary>Tests batch processing with different parallelism settings.</summary>
        [Theory]
        [InlineData(1)]   // Sequential
        [InlineData(2)]   // Limited parallelism
        [InlineData(4)]   // Moderate parallelism
        [InlineData(-1)]  // Unlimited parallelism
        public async Task MeshBatchAsyncWithDifferentParallelismCompletesSuccessfully(int maxParallelism)
        {
            // Arrange
            var structures = new[]
            {
                new PrismStructureDefinition(Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2)
                }), 0, 1),
                new PrismStructureDefinition(Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3)
                }), 0, 1),
                new PrismStructureDefinition(Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 4), new Vec2(0, 4)
                }), 0, 1)
            };

            // Act
            var meshes = await _asyncMesher.MeshBatchAsync(structures, _options, maxParallelism);

            // Assert
            meshes.Should().HaveCount(3);
            meshes.Should().AllSatisfy(mesh => mesh.QuadCount.Should().BeGreaterThan(0));
        }

        /// <summary>Tests batch processing with progress reporting.</summary>
        [Fact]
        public async Task MeshBatchAsyncWithProgressReportsProgress()
        {
            // Arrange
            var structures = new[]
            {
                new PrismStructureDefinition(Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2),
                new PrismStructureDefinition(Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 6), new Vec2(0, 6)
                }), 0, 2)
            };

            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));

            // Act
            var meshes = await _asyncMesher.MeshBatchAsync(structures, _options, progress: progress);

            // Assert
            meshes.Should().HaveCount(2);
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(p => p.Operation == "Batch Processing");
        }

        /// <summary>Tests single item batch processing optimization path.</summary>
        [Fact]
        public async Task MeshBatchAsyncWithSingleItemUsesFastPath()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));

            // Act
            var meshes = await _asyncMesher.MeshBatchAsync(new[] { structure }, _options, progress: progress);

            // Assert
            meshes.Should().HaveCount(1);
            meshes[0].QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>Tests MeshWithProgressAsync detailed progress reporting.</summary>
        [Fact]
        public async Task MeshWithProgressAsyncReportsDetailedProgress()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 8), new Vec2(5, 8), new Vec2(5, 10), new Vec2(0, 10)
                }), -2, 8);

            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));

            // Act
            var mesh = await _asyncMesher.MeshWithProgressAsync(structure, _options, progress);

            // Assert
            mesh.QuadCount.Should().BeGreaterThan(0);
            progressReports.Should().NotBeEmpty();

            // Should have different operation phases
            var operations = progressReports.Select(p => p.Operation).Distinct().ToList();
            operations.Should().Contain("Initializing");
            operations.Should().Contain("Side Faces");
        }

        /// <summary>Tests async with trivial optimization path.</summary>
        [Fact]
        public async Task MeshAsyncWithTrivialStructureUsesFastPath()
        {
            // Arrange - Trivial structure without cancellation token
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2)
                }), 0, 1);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var mesh = await _asyncMesher.MeshAsync(structure, _options);
            stopwatch.Stop();

            // Assert
            mesh.QuadCount.Should().BeGreaterThan(0);
            // Trivial structures should be reasonably fast - use generous threshold for CI stability
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        }

        /// <summary>Tests async with cancellation token forces async path.</summary>
        [Fact]
        public async Task MeshAsyncWithCancellationTokenUsesAsyncPath()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2)
                }), 0, 1);

            using var cts = new CancellationTokenSource();

            // Act
            var mesh = await _asyncMesher.MeshAsync(structure, _options, cts.Token);

            // Assert
            mesh.QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>Tests performance monitoring during mesh generation.</summary>
        [Fact]
        public async Task MeshAsyncUpdatesPerformanceCounters()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            var statsBefore = await _asyncMesher.GetLivePerformanceStatsAsync();

            // Act
            var mesh = await _asyncMesher.MeshAsync(structure, _options);

            // Assert
            var statsAfter = await _asyncMesher.GetLivePerformanceStatsAsync();

            (statsAfter.MeshingOperations - statsBefore.MeshingOperations).Should().BeGreaterThan(0);
            // Use range check instead of exact match to handle concurrent operations
            (statsAfter.QuadsGenerated - statsBefore.QuadsGenerated).Should().BeGreaterThanOrEqualTo(mesh.QuadCount);
            (statsAfter.TrianglesGenerated - statsBefore.TrianglesGenerated).Should().BeGreaterThanOrEqualTo(mesh.TriangleCount);
        }

        /// <summary>Tests empty structures collection validation.</summary>
        [Fact]
        public async Task MeshBatchAsyncWithEmptyCollectionThrowsArgumentException()
        {
            // Arrange
            var emptyStructures = new PrismStructureDefinition[0];

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _asyncMesher.MeshBatchAsync(emptyStructures, _options).AsTask());
        }

        /// <summary>Tests null parameters validation in async methods.</summary>
        [Fact]
        public async Task AsyncMethodsWithNullParametersThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _asyncMesher.MeshAsync(null!, _options).AsTask());

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _asyncMesher.EstimateComplexityAsync(null!, _options).AsTask());

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _asyncMesher.MeshBatchAsync(null!, _options).AsTask());
        }
    }
}
