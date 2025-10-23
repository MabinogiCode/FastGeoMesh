using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry {
    public sealed class AddEdgeToTriangleMappingWorksWithMultipleTrianglesTest {
        [Fact]
        public void Test() {
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 2, 10);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 2, 20);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 2, 30);

            var triangles = edgeToTris[(1, 2)];
            triangles.Should().HaveCount(3);
            triangles.Should().Contain(new[] { 10, 20, 30 });
        }
    }
}
