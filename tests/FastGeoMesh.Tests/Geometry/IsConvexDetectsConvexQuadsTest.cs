using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class IsConvexDetectsConvexQuadsTest.
    /// </summary>
    public sealed class IsConvexDetectsConvexQuadsTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var convexSquare = (new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10));
            var concaveQuad = (new Vec2(0, 0), new Vec2(10, 10), new Vec2(10, 0), new Vec2(0, 10));
            helper.IsConvex(convexSquare).Should().BeTrue();
            helper.IsConvex(concaveQuad).Should().BeFalse();
        }
    }
}
