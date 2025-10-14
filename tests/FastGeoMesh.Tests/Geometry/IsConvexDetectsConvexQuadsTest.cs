using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class IsConvexDetectsConvexQuadsTest
    {
        [Fact]
        public void Test()
        {
            var convexSquare = (new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10));
            var concaveQuad = (new Vec2(0, 0), new Vec2(10, 10), new Vec2(10, 0), new Vec2(0, 10));
            GeometryHelper.IsConvex(convexSquare).Should().BeTrue();
            GeometryHelper.IsConvex(concaveQuad).Should().BeFalse();
        }
    }
}
