using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Tests for class TessPoolStatisticsWorkTest.
    /// </summary>
    public sealed class TessPoolStatisticsWorkTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var stats = TessPool.GetStatistics();
            stats.PooledCount.Should().BeGreaterThanOrEqualTo(0);
            stats.IsShuttingDown.Should().BeFalse();
        }
    }
}
