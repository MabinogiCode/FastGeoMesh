using FastGeoMesh.Application.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Application.Services
{
    public class NullPerformanceMonitorTests
    {
        [Fact]
        public void StartMeshingActivityReturnsDisposableAndDoesNotThrowOnDispose()
        {
            var monitor = new NullPerformanceMonitor();
            using var disp = monitor.StartMeshingActivity("test");
            disp.Should().NotBeNull();
        }

        [Fact]
        public void StatsAndNoOpsDoNotThrow()
        {
            var monitor = new NullPerformanceMonitor();
            monitor.IncrementMeshingOperations();
            monitor.AddQuadsGenerated(5);
            monitor.AddTrianglesGenerated(3);
            var stats = monitor.GetLiveStatistics();
            stats.Should().NotBeNull();
        }
    }
}
