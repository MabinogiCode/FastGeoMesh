



using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

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
        public void PerformanceMonitorCountersTracksOperations()
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

        /// <summary>Tests PerformanceMonitor activity creation.</summary>
        [Fact]
        public void PerformanceMonitorStartMeshingActivityCreatesActivity()
        {
            // Arrange
            var operationData = new { VertexCount = 100, Complexity = "Moderate" };

            // Act
            using var activity = PerformanceMonitor.StartMeshingActivity("TestOperation", operationData);

            // Assert
            activity.Should().NotBeNull();
        }

        /// <summary>Tests PerformanceMonitor statistics properties.</summary>
        [Fact]
        public void PerformanceMonitorStatisticsHasAllProperties()
        {
            // Act
            var stats = PerformanceMonitor.Counters.GetStatistics();

            // Assert
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
            stats.QuadsGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.TrianglesGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.PoolHitRate.Should().BeInRange(0.0, 1.0);
        }
    }
}
