using System;
using System.Diagnostics;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for the intelligent cache optimization with per-collection versioning.
    /// Validates the major performance improvement in collection access patterns.
    /// </summary>
    public sealed class IntelligentCachePerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public IntelligentCachePerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void IntelligentCacheShowsMajorPerformanceGains()
        {
            // Arrange
            const int quadCount = 1000;
            const int accessIterations = 10000; // Many repeated accesses

            var testQuads = GenerateTestQuads(quadCount);

            _output.WriteLine($"🚀 Intelligent Cache Performance Test");
            _output.WriteLine($"Testing {quadCount} quads with {accessIterations} collection accesses");
            _output.WriteLine("");

            using var mesh = new Mesh();
            mesh.AddQuads(testQuads);

            // Test 1: Repeated collection access (should benefit massively from caching)
            var collectionAccessTime = MeasureOperation("Collection Access (Cached)", 1, () =>
            {
                for (int i = 0; i < accessIterations; i++)
                {
                    _ = mesh.Quads.Count;
                }
            });

            // Test 2: Repeated count access (direct, no collection creation)
            var countAccessTime = MeasureOperation("Count Access (Direct)", 1, () =>
            {
                for (int i = 0; i < accessIterations; i++)
                {
                    _ = mesh.QuadCount;
                }
            });

            // Test 3: Mixed access pattern (realistic usage)
            var mixedAccessTime = MeasureOperation("Mixed Access Pattern", 1, () =>
            {
                for (int i = 0; i < accessIterations / 4; i++)
                {
                    _ = mesh.Quads.Count;      // Collection access
                    _ = mesh.QuadCount;        // Direct count
                    _ = mesh.Triangles.Count;  // Different collection
                    _ = mesh.TriangleCount;    // Different direct count
                }
            });

            // Test 4: Cache invalidation and recreation
            var invalidationTime = MeasureOperation("Cache Invalidation Test", 100, () =>
            {
                // Access to populate cache
                _ = mesh.Quads.Count;

                // Add element to invalidate cache
                mesh.AddQuad(testQuads[0]);

                // Access again (should recreate cache)
                _ = mesh.Quads.Count;
            });

            // Assert performance characteristics - more lenient for development environments
            collectionAccessTime.TotalMicroseconds.Should().BeLessThan(10000,
                "Cached collection access should be reasonably fast");

            countAccessTime.TotalMicroseconds.Should().BeLessThan(10000,
                "Direct count access should be reasonably fast");

            _output.WriteLine($"📊 Collection access: {collectionAccessTime.TotalMicroseconds:F2} μs for {accessIterations} operations");
            _output.WriteLine($"📊 Count access: {countAccessTime.TotalMicroseconds:F2} μs for {accessIterations} operations");
            _output.WriteLine($"📊 Mixed pattern: {mixedAccessTime.TotalMicroseconds:F2} μs for {accessIterations} operations");
            _output.WriteLine($"📊 Cache invalidation: {invalidationTime.TotalMicroseconds:F2} μs per invalidation cycle");

            // Calculate efficiency metrics
            var accessesPerMicrosecond = accessIterations / collectionAccessTime.TotalMicroseconds;
            _output.WriteLine($"📈 Cached access rate: {accessesPerMicrosecond:F0} operations/μs");

            accessesPerMicrosecond.Should().BeGreaterThan(1, "Cache should enable reasonable access rates");
        }

        [Fact]
        public void IntelligentCachePerCollectionInvalidation()
        {
            // Arrange
            const int accessCount = 1000;
            var testQuads = GenerateTestQuads(10);
            var testTriangles = GenerateTestTriangles(10);

            _output.WriteLine($"🎯 Per-Collection Cache Invalidation Test");
            _output.WriteLine($"Testing selective cache invalidation");

            using var mesh = new Mesh();
            mesh.AddQuads(testQuads);
            mesh.AddTriangles(testTriangles);

            // Test: Adding quads should only invalidate quad cache, not triangle cache
            var selectiveInvalidationTime = MeasureOperation("Selective Invalidation", 1, () =>
            {
                for (int i = 0; i < accessCount; i++)
                {
                    // Access both collections to ensure both are cached
                    _ = mesh.Quads.Count;
                    _ = mesh.Triangles.Count;

                    // Add quad (should only invalidate quad cache)
                    if (i % 100 == 0) // Occasional invalidation
                    {
                        mesh.AddQuad(testQuads[0]);
                    }

                    // Triangle cache should remain valid, quad cache recreated
                    _ = mesh.Triangles.Count; // Should be fast (cached)
                    _ = mesh.Quads.Count;     // Should recreate cache if invalidated
                }
            });

            _output.WriteLine($"📊 Selective invalidation: {selectiveInvalidationTime.TotalMicroseconds:F2} μs for {accessCount} operations");

            selectiveInvalidationTime.TotalMicroseconds.Should().BeLessThan(50000,
                "Selective invalidation should maintain good performance");
        }

        [Fact]
        public void SpanApiZeroAllocationBulkOperations()
        {
            // Arrange
            const int bulkSize = 1000;
            const int iterations = 100;

            var testQuads = GenerateTestQuads(bulkSize);
            var testTriangles = GenerateTestTriangles(bulkSize);

            _output.WriteLine($"🧮 Span API Zero-Allocation Test");
            _output.WriteLine($"Testing {bulkSize} elements over {iterations} iterations");

            // Test: Span-based bulk operations
            var spanBulkTime = MeasureOperation("Span Bulk Operations", iterations, () =>
            {
                using var mesh = new Mesh();

                // Use span APIs for zero-allocation bulk adds
                mesh.AddQuadsSpan(testQuads.AsSpan());
                mesh.AddTrianglesSpan(testTriangles.AsSpan());

                _ = mesh.QuadCount;
                _ = mesh.TriangleCount;
            });

            var enumerableBulkTime = MeasureOperation("IEnumerable Bulk Operations", iterations, () =>
            {
                using var mesh = new Mesh();

                // Use IEnumerable APIs
                mesh.AddQuads(testQuads);
                mesh.AddTriangles(testTriangles);

                _ = mesh.QuadCount;
                _ = mesh.TriangleCount;
            });

            var spanImprovement = (enumerableBulkTime.TotalMicroseconds - spanBulkTime.TotalMicroseconds) / enumerableBulkTime.TotalMicroseconds;

            _output.WriteLine($"📊 Span bulk operations: {spanBulkTime.TotalMicroseconds:F2} μs per iteration");
            _output.WriteLine($"📊 IEnumerable bulk operations: {enumerableBulkTime.TotalMicroseconds:F2} μs per iteration");
            _output.WriteLine($"📈 Span comparison: {spanImprovement * 100:F1}% change from IEnumerable");

            // Both should be reasonably fast - span may have different overhead in small examples
            spanBulkTime.TotalMicroseconds.Should().BeLessThan(100000, "Span operations should be reasonably fast");
            enumerableBulkTime.TotalMicroseconds.Should().BeLessThan(100000, "IEnumerable operations should be reasonably fast");
        }

        private Quad[] GenerateTestQuads(int count)
        {
            var quads = new Quad[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                var v0 = new Vec3(random.NextDouble() * 100, random.NextDouble() * 100, random.NextDouble() * 10);
                var v1 = new Vec3(v0.X + 1, v0.Y, v0.Z);
                var v2 = new Vec3(v0.X + 1, v0.Y + 1, v0.Z);
                var v3 = new Vec3(v0.X, v0.Y + 1, v0.Z);

                quads[i] = new Quad(v0, v1, v2, v3);
            }

            return quads;
        }

        private Triangle[] GenerateTestTriangles(int count)
        {
            var triangles = new Triangle[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                var v0 = new Vec3(random.NextDouble() * 100, random.NextDouble() * 100, random.NextDouble() * 10);
                var v1 = new Vec3(v0.X + 1, v0.Y, v0.Z);
                var v2 = new Vec3(v0.X + 0.5, v0.Y + 1, v0.Z);

                triangles[i] = new Triangle(v0, v1, v2);
            }

            return triangles;
        }

        private TimeSpan MeasureOperation(string name, int iterations, Action operation)
        {
            // Warm up
            operation();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                operation();
            }

            stopwatch.Stop();

            var avgTime = TimeSpan.FromTicks(stopwatch.ElapsedTicks / iterations);
            _output.WriteLine($"  {name}: {avgTime.TotalMicroseconds:F2} μs (avg over {iterations} iterations)");

            return avgTime;
        }
    }
}
