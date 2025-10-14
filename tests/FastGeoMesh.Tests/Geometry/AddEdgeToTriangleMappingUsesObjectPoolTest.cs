using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class AddEdgeToTriangleMappingUsesObjectPoolTest
    {
        [Fact]
        public void Test()
        {
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 10, 20, 500);
            edgeToTris.Should().ContainKey((10, 20));
            var list = edgeToTris[(10, 20)];
            list.Should().NotBeNull();
            list.Should().HaveCount(1);
            list[0].Should().Be(500);
        }
    }
}
