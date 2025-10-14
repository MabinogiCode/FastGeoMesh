using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;
using System.Diagnostics;

namespace FastGeoMesh.Tests.Performance
{
    public sealed class PerformanceMonitorActivitySourceWorksTest
    {
        [Fact]
        public void Test()
        {
            using var activity1 = PerformanceMonitor.StartMeshingActivity("TestOperation", new { EdgeLength = 1.0, QuadCount = 100 });

            using var listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == "FastGeoMesh",
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(listener);

            using var activity2 = PerformanceMonitor.StartMeshingActivity("TestOperationWithListener", new { EdgeLength = 2.0, QuadCount = 200 });

            activity1.Should().NotBeNull();
            activity2.Should().NotBeNull();

            if (activity1 != null)
            {
                activity1.OperationName.Should().Be("TestOperation");
            }

            if (activity2 != null)
            {
                activity2.OperationName.Should().Be("TestOperationWithListener");
            }
        }
    }
}
