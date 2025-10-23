using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation {
    /// <summary>
    /// Validates that performance monitoring works and returns statistics.
    /// </summary>
    public sealed class PerformanceMonitoringWorks {
        [Fact]
        public async Task Test() {
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var stats = await asyncMesher.GetLivePerformanceStatsAsync();
            stats.Should().NotBeNull();
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
