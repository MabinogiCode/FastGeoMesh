using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance {
    public sealed class TessPoolStatisticsWorkTest {
        [Fact]
        public void Test() {
            var stats = TessPool.GetStatistics();
            stats.PooledCount.Should().BeGreaterThanOrEqualTo(0);
            stats.IsShuttingDown.Should().BeFalse();
        }
    }
}
