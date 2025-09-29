using System.Collections.Generic;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for EdgeMappingHelper functions.</summary>
    public sealed class EdgeMappingHelperTests
    {
        [Fact]
        public void AddEdgeToTriangleMappingCreatesCorrectMappings()
        {
            // Arrange
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            // Act
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 0, 1, 100);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 0, 200); // Same edge, different order
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 2, 3, 300);

            // Assert
            edgeToTris.Should().HaveCount(2, "Should have 2 unique edges");

            var edge01 = edgeToTris[(0, 1)];
            edge01.Should().HaveCount(2, "Edge (0,1) should have 2 triangles");
            edge01.Should().Contain(100, "Should contain first triangle");
            edge01.Should().Contain(200, "Should contain second triangle");

            var edge23 = edgeToTris[(2, 3)];
            edge23.Should().HaveCount(1, "Edge (2,3) should have 1 triangle");
            edge23.Should().Contain(300, "Should contain triangle 300");
        }

        [Fact]
        public void AddEdgeToTriangleMappingNormalizesEdgeOrder()
        {
            // Arrange
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            // Act - Add same edge with different vertex orders
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 5, 2, 100);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 2, 5, 200);

            // Assert - Should be stored under consistent key (smaller index first)
            edgeToTris.Should().ContainKey((2, 5), "Edge should be normalized to (2,5)");
            edgeToTris.Should().NotContainKey((5, 2), "Should not have reverse order key");

            var triangles = edgeToTris[(2, 5)];
            triangles.Should().HaveCount(2, "Should have both triangles");
            triangles.Should().Contain(100).And.Contain(200);
        }

        [Fact]
        public void AddEdgeToTriangleMappingUsesObjectPool()
        {
            // Arrange
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            // Act
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 10, 20, 500);

            // Assert
            edgeToTris.Should().ContainKey((10, 20));
            var list = edgeToTris[(10, 20)];
            list.Should().NotBeNull("List should be created from pool");
            list.Should().HaveCount(1, "Should contain one triangle");
            list[0].Should().Be(500);
        }

        [Fact]
        public void AddEdgeToTriangleMappingHandlesSelfLoops()
        {
            // Arrange
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            // Act - Edge from vertex to itself
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 5, 5, 100);

            // Assert
            edgeToTris.Should().ContainKey((5, 5), "Should handle self-loop edge");
            edgeToTris[(5, 5)].Should().Contain(100);
        }

        [Fact]
        public void AddEdgeToTriangleMappingWorksWithMultipleTriangles()
        {
            // Arrange
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            // Act - Add multiple triangles to same edge
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 2, 10);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 2, 20);
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1, 2, 30);

            // Assert
            var triangles = edgeToTris[(1, 2)];
            triangles.Should().HaveCount(3, "Should accumulate all triangles");
            triangles.Should().Contain(new[] { 10, 20, 30 }, "Should contain all added triangles");
        }

        [Fact]
        public void AddEdgeToTriangleMappingHandlesLargeVertexIndices()
        {
            // Arrange
            var edgeToTris = new Dictionary<(int, int), List<int>>();

            // Act
            EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, 1000000, 999999, 42);

            // Assert
            edgeToTris.Should().ContainKey((999999, 1000000), "Should handle large indices");
            edgeToTris[(999999, 1000000)].Should().Contain(42);
        }
    }
}
