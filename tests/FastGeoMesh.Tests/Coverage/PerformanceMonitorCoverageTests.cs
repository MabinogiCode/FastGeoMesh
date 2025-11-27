using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for class PerformanceMonitorCoverageTests.
    /// </summary>
    public sealed class PerformanceMonitorCoverageTests
    {
        /// <summary>
        /// Runs test PerformanceMonitorCountersTracksOperations.
        /// </summary>
        [Fact]
        public void PerformanceMonitorCountersTracksOperations()
        {
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
        /// <summary>
        /// Runs test PerformanceMonitorStartMeshingActivityCreatesActivity.
        /// </summary>
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
        /// <summary>
        /// Runs test PerformanceMonitorStatisticsHasAllProperties.
        /// </summary>
        [Fact]
        public void PerformanceMonitorStatisticsHasAllProperties()
        {
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
