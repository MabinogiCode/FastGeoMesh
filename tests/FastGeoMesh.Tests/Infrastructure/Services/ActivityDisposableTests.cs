using System.Diagnostics;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Infrastructure.Services
{
    public class ActivityDisposableTests
    {
        [Fact]
        public void DisposeWithNullActivityDoesNotThrow()
        {
            using var _ = new ActivityDisposable(null);
        }

        [Fact]
        public void DisposeWithStartedActivityDisposesSafely()
        {
            using var activity = new Activity("test");
            activity.Start();

            using var wrapper = new ActivityDisposable(activity);
            // Disposal will happen at the end of using scope; just ensure the object is created.
            wrapper.Should().NotBeNull();
        }
    }
}
