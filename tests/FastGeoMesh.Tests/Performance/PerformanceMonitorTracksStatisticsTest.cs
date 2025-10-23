using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance {
    public sealed class PerformanceMonitorTracksStatisticsTest {
        [Fact]
        public void Test() {
            var initialStats = PerformanceMonitorCounters.GetStatistics();
            PerformanceMonitorCounters.IncrementMeshingOperations();
            PerformanceMonitorCounters.AddQuadsGenerated(10);
            PerformanceMonitorCounters.AddTrianglesGenerated(5);
            PerformanceMonitorCounters.IncrementPoolHit();
            PerformanceMonitorCounters.IncrementPoolMiss();
            var finalStats = PerformanceMonitorCounters.GetStatistics();
            finalStats.MeshingOperations.Should().Be(initialStats.MeshingOperations + 1);
            finalStats.QuadsGenerated.Should().Be(initialStats.QuadsGenerated + 10);
            finalStats.TrianglesGenerated.Should().Be(initialStats.TrianglesGenerated + 5);
            finalStats.PoolHitRate.Should().BeGreaterThan(0.0);
        }
    }
}
