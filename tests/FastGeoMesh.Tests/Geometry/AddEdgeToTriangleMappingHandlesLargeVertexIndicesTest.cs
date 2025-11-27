using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class AddEdgeToTriangleMappingHandlesLargeVertexIndicesTest.
    /// </summary>
    public sealed class AddEdgeToTriangleMappingHandlesLargeVertexIndicesTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1000000, 999999, 42);
            edgeToTris.Should().ContainKey((999999, 1000000));
            edgeToTris[(999999, 1000000)].Should().Contain(42);
        }
    }
}
