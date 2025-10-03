using Xunit;
using FastGeoMesh.Utils;
using FastGeoMesh.Meshing;
using FastGeoMesh.Geometry;
using FastGeoMesh.Structures;
using FluentAssertions;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for PerformanceMonitor and related metrics functionality to improve code coverage.
    /// Covers performance tracking, activity monitoring, and statistics collection.
    /// </summary>
    public sealed class PerformanceMonitorCoverageTests
    {
        /// <summary>Tests PerformanceMonitor.Counters basic functionality.</summary>
        [Fact]
        public void PerformanceMonitor_Counters_TracksOperations()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();
            var initialOperations = initialStats.MeshingOperations;

            // Act
            PerformanceMonitor.Counters.IncrementMeshingOperations();
            PerformanceMonitor.Counters.IncrementMeshingOperations();

            // Assert
            var finalStats = PerformanceMonitor.Counters.GetStatistics();
            (finalStats.MeshingOperations - initialOperations).Should().Be(2);
        }

        /// <summary>Tests PerformanceMonitor.Counters quad tracking.</summary>
        [Fact]
        public void PerformanceMonitor_Counters_TracksQuads()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();
            var initialQuads = initialStats.QuadsGenerated;

            // Act
            PerformanceMonitor.Counters.AddQuadsGenerated(150);
            PerformanceMonitor.Counters.AddQuadsGenerated(250);

            // Assert
            var finalStats = PerformanceMonitor.Counters.GetStatistics();
            (finalStats.QuadsGenerated - initialQuads).Should().Be(400);
        }

        /// <summary>Tests PerformanceMonitor.Counters triangle tracking.</summary>
        [Fact]
        public void PerformanceMonitor_Counters_TracksTriangles()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();
            var initialTriangles = initialStats.TrianglesGenerated;

            // Act
            PerformanceMonitor.Counters.AddTrianglesGenerated(75);
            PerformanceMonitor.Counters.AddTrianglesGenerated(125);

            // Assert
            var finalStats = PerformanceMonitor.Counters.GetStatistics();
            (finalStats.TrianglesGenerated - initialTriangles).Should().Be(200);
        }

        /// <summary>Tests PerformanceMonitor activity creation.</summary>
        [Fact]
        public void PerformanceMonitor_StartMeshingActivity_CreatesActivity()
        {
            // Arrange
            var operationData = new { VertexCount = 100, Complexity = "Moderate" };

            // Act
            using var activity = PerformanceMonitor.StartMeshingActivity("TestOperation", operationData);

            // Assert
            activity.Should().NotBeNull();
        }

        /// <summary>Tests PerformanceMonitor activity with null data.</summary>
        [Fact]
        public void PerformanceMonitor_StartMeshingActivity_WithNullData_WorksCorrectly()
        {
            // Act
            using var activity = PerformanceMonitor.StartMeshingActivity("TestOperation", null);

            // Assert
            activity.Should().NotBeNull();
        }

        /// <summary>Tests PerformanceMonitor activity disposal.</summary>
        [Fact]
        public void PerformanceMonitor_StartMeshingActivity_DisposesCorrectly()
        {
            // Arrange & Act
            var activity = PerformanceMonitor.StartMeshingActivity("TestOperation", new { Test = true });
            
            // Should not throw
            activity.Dispose();
            activity.Dispose(); // Multiple dispose calls should be safe
        }

        /// <summary>Tests PerformanceMonitor with concurrent operations.</summary>
        [Fact]
        public async Task PerformanceMonitor_Counters_ThreadSafe()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();
            var tasks = new List<Task>();

            // Act - Run concurrent operations
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        PerformanceMonitor.Counters.IncrementMeshingOperations();
                        PerformanceMonitor.Counters.AddQuadsGenerated(1);
                        PerformanceMonitor.Counters.AddTrianglesGenerated(1);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var finalStats = PerformanceMonitor.Counters.GetStatistics();
            (finalStats.MeshingOperations - initialStats.MeshingOperations).Should().Be(100);
            (finalStats.QuadsGenerated - initialStats.QuadsGenerated).Should().Be(100);
            (finalStats.TrianglesGenerated - initialStats.TrianglesGenerated).Should().Be(100);
        }

        /// <summary>Tests PerformanceMonitor statistics properties.</summary>
        [Fact]
        public void PerformanceMonitor_Statistics_HasAllProperties()
        {
            // Act
            var stats = PerformanceMonitor.Counters.GetStatistics();

            // Assert
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
            stats.QuadsGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.TrianglesGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.PoolHitRate.Should().BeInRange(0.0, 1.0);
        }

        /// <summary>Tests PerformanceMonitor with edge case values.</summary>
        [Fact]
        public void PerformanceMonitor_Counters_HandlesLargeValues()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();

            // Act - Add very large values
            PerformanceMonitor.Counters.AddQuadsGenerated(int.MaxValue / 2);
            PerformanceMonitor.Counters.AddTrianglesGenerated(int.MaxValue / 2);

            // Assert
            var finalStats = PerformanceMonitor.Counters.GetStatistics();
            finalStats.QuadsGenerated.Should().BeGreaterThan(initialStats.QuadsGenerated);
            finalStats.TrianglesGenerated.Should().BeGreaterThan(initialStats.TrianglesGenerated);
        }

        /// <summary>Tests PerformanceMonitor with zero values.</summary>
        [Fact]
        public void PerformanceMonitor_Counters_HandlesZeroValues()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();

            // Act
            PerformanceMonitor.Counters.AddQuadsGenerated(0);
            PerformanceMonitor.Counters.AddTrianglesGenerated(0);

            // Assert
            var finalStats = PerformanceMonitor.Counters.GetStatistics();
            finalStats.QuadsGenerated.Should().Be(initialStats.QuadsGenerated);
            finalStats.TrianglesGenerated.Should().Be(initialStats.TrianglesGenerated);
        }

        /// <summary>Tests PerformanceMonitor integration with actual meshing.</summary>
        [Fact]
        public async Task PerformanceMonitor_IntegrationWithMeshing_TracksCorrectly()
        {
            // Arrange
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();

            var statsBefore = await asyncMesher.GetLivePerformanceStatsAsync();

            // Act
            var mesh = await asyncMesher.MeshAsync(structure, options);
            var statsAfter = await asyncMesher.GetLivePerformanceStatsAsync();

            // Assert
            (statsAfter.MeshingOperations - statsBefore.MeshingOperations).Should().BeGreaterThan(0);
            (statsAfter.QuadsGenerated - statsBefore.QuadsGenerated).Should().Be(mesh.QuadCount);
            (statsAfter.TrianglesGenerated - statsBefore.TrianglesGenerated).Should().Be(mesh.TriangleCount);
        }

        /// <summary>Tests PerformanceMonitor activity with complex data structures.</summary>
        [Fact]
        public void PerformanceMonitor_StartMeshingActivity_WithComplexData_WorksCorrectly()
        {
            // Arrange
            var complexData = new
            {
                VertexCount = 1000,
                HoleCount = 5,
                ConstraintCount = 10,
                Metadata = new { CreatedBy = "Test", Version = "1.4.0" },
                Tags = new[] { "complex", "performance", "test" }
            };

            // Act & Assert - Should not throw
            using var activity = PerformanceMonitor.StartMeshingActivity("ComplexOperation", complexData);
            activity.Should().NotBeNull();
        }

        /// <summary>Tests PerformanceMonitor with nested activities.</summary>
        [Fact]
        public void PerformanceMonitor_NestedActivities_WorkCorrectly()
        {
            // Act & Assert - Should not throw with nested activities
            using var outerActivity = PerformanceMonitor.StartMeshingActivity("OuterOperation", new { Level = 1 });
            using var innerActivity = PerformanceMonitor.StartMeshingActivity("InnerOperation", new { Level = 2 });
            
            outerActivity.Should().NotBeNull();
            innerActivity.Should().NotBeNull();
        }

        /// <summary>Tests PerformanceMonitor pool hit rate calculation.</summary>
        [Fact]
        public void PerformanceMonitor_PoolHitRate_CalculatesCorrectly()
        {
            // Arrange - Perform some operations to establish baseline
            PerformanceMonitor.Counters.IncrementMeshingOperations();
            PerformanceMonitor.Counters.AddQuadsGenerated(10);

            // Act
            var stats = PerformanceMonitor.Counters.GetStatistics();

            // Assert
            stats.PoolHitRate.Should().BeInRange(0.0, 1.0);
        }
    }
}
