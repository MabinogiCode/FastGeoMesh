using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class LerpInterpolatesCorrectlyTest
    {
        [Fact]
        public void Test()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var a = new Vec2(0, 0);
            var b = new Vec2(10, 20);
            var start = helper.Lerp(a, b, 0.0);
            start.Should().Be(a);
            var end = helper.Lerp(a, b, 1.0);
            end.Should().Be(b);
            var middle = helper.Lerp(a, b, 0.5);
            middle.Should().Be(new Vec2(5, 10));
            var quarter = helper.Lerp(a, b, 0.25);
            quarter.Should().Be(new Vec2(2.5, 5));
        }
    }
}
