using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage {
    /// <summary>
    /// Tests for PerformanceMonitor and related metrics functionality to improve code coverage.
    /// Covers performance tracking, activity monitoring, and statistics collection.
    /// </summary>
    public sealed class PerformanceMonitorCoverageTests {
        /// <summary>Tests PerformanceMonitor.Counters basic functionality.</summary>
        [Fact]
        public void PerformanceMonitorCountersTracksOperations() {
            // Arrange
            var initialStats = PerformanceMonitorCounters.GetStatistics();
            var initialOperations = initialStats.MeshingOperations;

            // Act
            PerformanceMonitorCounters.IncrementMeshingOperations();
            PerformanceMonitorCounters.IncrementMeshingOperations();

            // Assert
            var finalStats = PerformanceMonitorCounters.GetStatistics();
            (finalStats.MeshingOperations - initialOperations).Should().Be(2);
        }

        /// <summary>Tests PerformanceMonitor activity creation.</summary>
        [Fact]
        public void PerformanceMonitorStartMeshingActivityCreatesActivity() {
            // Arrange
            var operationData = new { VertexCount = 100, Complexity = "Moderate" };

            // Act
            using var activity = PerformanceMonitor.StartMeshingActivity("TestOperation", operationData);

            // Assert
            activity.Should().NotBeNull();
        }

        /// <summary>Tests PerformanceMonitor statistics properties.</summary>
        [Fact]
        public void PerformanceMonitorStatisticsHasAllProperties() {
            // Act
            var stats = PerformanceMonitorCounters.GetStatistics();

            // Assert
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
            stats.QuadsGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.TrianglesGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.PoolHitRate.Should().BeInRange(0.0, 1.0);
        }
    }
}
