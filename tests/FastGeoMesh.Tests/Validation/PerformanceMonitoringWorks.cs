using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Tests for class PerformanceMonitoringWorks.
    /// </summary>
    public sealed class PerformanceMonitoringWorks
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
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
