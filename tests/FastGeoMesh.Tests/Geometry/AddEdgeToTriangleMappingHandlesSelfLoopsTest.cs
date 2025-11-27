using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class AddEdgeToTriangleMappingHandlesSelfLoopsTest.
    /// </summary>
    public sealed class AddEdgeToTriangleMappingHandlesSelfLoopsTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
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
