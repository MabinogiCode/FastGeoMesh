using System;
using System.Collections.Generic;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Simplified functional regression tests - no performance assertions.
    /// These tests validate that the API works correctly without timing constraints.
    /// </summary>
    public sealed class FunctionalRegressionTests
    {
        [Fact]
        public void BatchOperationsWorkCorrectly()
        {
            // Arrange
            var testQuads = GenerateTestQuads(100);

            // Act & Assert - Functional validation only
            using var mesh = new Mesh();

            // Sequential addition should work
            foreach (var quad in testQuads.Take(50))
            {
                mesh.AddQuad(quad);
            }
            mesh.QuadCount.Should().Be(50);

            // Batch addition should work
            mesh.AddQuads(testQuads.Skip(50));
            mesh.QuadCount.Should().Be(100);

            // Collections should be accessible
            mesh.Quads.Should().HaveCount(100);
            mesh.QuadCount.Should().Be(mesh.Quads.Count);
        }

        [Fact]
        public void SpanOperationsProduceCorrectResults()
        {
            // Arrange
            var vertices = new Vec2[]
            {
                new(0, 0),
                new(2, 0),
                new(2, 2),
                new(0, 2)
            };

            // Act
            var centroid = ((ReadOnlySpan<Vec2>)vertices.AsSpan()).ComputeCentroid();
            var bounds = ((ReadOnlySpan<Vec2>)vertices.AsSpan()).ComputeBounds();

            // Assert - Functional correctness only
            centroid.Should().Be(new Vec2(1, 1));
            bounds.min.Should().Be(new Vec2(0, 0));
            bounds.max.Should().Be(new Vec2(2, 2));
        }

        [Fact]
        public void ObjectPoolingFunctionsCorrectly()
        {
            // Arrange & Act
            var list1 = MeshingPools.IntListPool.Get();
            var list2 = MeshingPools.IntListPool.Get();

            // Assert - Different instances
            list1.Should().NotBeSameAs(list2);

            // Act - Use and return
            list1.Add(1);
            list1.Add(2);
            list1.Count.Should().Be(2);

            MeshingPools.IntListPool.Return(list1);

            var list3 = MeshingPools.IntListPool.Get();
            list3.Should().BeEmpty("Returned lists should be cleared");

            MeshingPools.IntListPool.Return(list2);
            MeshingPools.IntListPool.Return(list3);
        }

        [Fact]
        public void IndexedMeshConversionWorksCorrectly()
        {
            // Arrange
            var testQuads = GenerateTestQuads(50);

            using var mesh = new Mesh();
            mesh.AddQuads(testQuads);

            // Act
            var indexed = IndexedMesh.FromMesh(mesh, 1e-9);

            // Assert - Functional validation
            indexed.QuadCount.Should().Be(50);
            indexed.VertexCount.Should().BeGreaterThan(0);
            indexed.EdgeCount.Should().BeGreaterThan(0);

            // Test caching works
            var vertices1 = indexed.Vertices;
            var vertices2 = indexed.Vertices;
            vertices1.Should().BeSameAs(vertices2, "Collections should be cached");

            // Test count properties work
            indexed.VertexCount.Should().Be(indexed.Vertices.Count);
            indexed.QuadCount.Should().Be(indexed.Quads.Count);
        }

        private static List<Quad> GenerateTestQuads(int count)
        {
            var quads = new List<Quad>(count);
            for (int i = 0; i < count; i++)
            {
                var v0 = new Vec3(i, 0, 0);
                var v1 = new Vec3(i + 1, 0, 0);
                var v2 = new Vec3(i + 1, 1, 0);
                var v3 = new Vec3(i, 1, 0);
                quads.Add(new Quad(v0, v1, v2, v3));
            }
            return quads;
        }
    }
}
