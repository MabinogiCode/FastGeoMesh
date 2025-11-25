using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Validates that performance monitoring works and returns statistics.
    /// </summary>
    public sealed class PerformanceMonitoringWorks
    {
        [Fact]
        public async Task Test()
        {
            var mesher = TestServiceProvider.CreatePrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var stats = await asyncMesher.GetLivePerformanceStatsAsync();
            stats.Should().NotBeNull();
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
