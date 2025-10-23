using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class AddEdgeToTriangleMappingHandlesLargeVertexIndicesTest {
        [Fact]
        public void Test() {
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1000000, 999999, 42);
            edgeToTris.Should().ContainKey((999999, 1000000));
            edgeToTris[(999999, 1000000)].Should().Contain(42);
        }
    }
}
