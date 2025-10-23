using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class AddEdgeToTriangleMappingNormalizesEdgeOrderTest {
        [Fact]
        public void Test() {
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 5, 2, 100);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 2, 5, 200);
            edgeToTris.Should().ContainKey((2, 5));
            edgeToTris.Should().NotContainKey((5, 2));
            var triangles = edgeToTris[(2, 5)];
            triangles.Should().HaveCount(2);
            triangles.Should().Contain(100).And.Contain(200);
        }
    }
}
