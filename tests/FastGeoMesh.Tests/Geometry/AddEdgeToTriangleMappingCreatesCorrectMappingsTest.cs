using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class AddEdgeToTriangleMappingCreatesCorrectMappingsTest
    {
        [Fact]
        public void Test()
        {
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 0, 1, 100);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 0, 200);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 2, 3, 300);

            edgeToTris.Should().HaveCount(2);

            var edge01 = edgeToTris[(0, 1)];
            edge01.Should().HaveCount(2);
            edge01.Should().Contain(100);
            edge01.Should().Contain(200);

            var edge23 = edgeToTris[(2, 3)];
            edge23.Should().HaveCount(1);
            edge23.Should().Contain(300);
        }
    }
}
