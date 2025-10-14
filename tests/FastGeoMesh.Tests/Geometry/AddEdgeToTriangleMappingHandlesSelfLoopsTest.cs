using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class AddEdgeToTriangleMappingHandlesSelfLoopsTest
    {
        [Fact]
        public void Test()
        {
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 5, 5, 100);
            edgeToTris.Should().ContainKey((5, 5));
            edgeToTris[(5, 5)].Should().Contain(100);
        }
    }
}
