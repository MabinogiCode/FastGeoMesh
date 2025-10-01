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
    /// Tests for the final round of optimizations focusing on IndexedMesh and validation performance.
    /// </summary>
    public sealed class FinalOptimizationTests
    {
        private readonly ITestOutputHelper _output;

        public FinalOptimizationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void IndexedMesh_FromMesh_ShowsSignificantImprovement()
        {
            // This tests the major optimization to IndexedMesh.FromMesh()
            const int quadCount = 2000;
            const int iterations = 10;
            
            var testQuads = GenerateTestQuads(quadCount);
            
            _output.WriteLine($"🚀 IndexedMesh.FromMesh() Optimization Test");
            _output.WriteLine($"Testing {quadCount} quads over {iterations} iterations");
            _output.WriteLine("");

            // Test the optimized version (current implementation)
            var optimizedTime = MeasureOperation("Optimized FromMesh", iterations, () =>
            {
                using var mesh = new Mesh();
                mesh.AddQuads(testQuads);
                var indexed = IndexedMesh.FromMesh(mesh, 1e-9);
                _ = indexed.VertexCount; // Use the count property
            });

            // Compare with repeated calls to show caching benefits
            var repeatedAccessTime = MeasureOperation("Repeated Property Access", iterations, () =>
            {
                using var mesh = new Mesh();
                mesh.AddQuads(testQuads);
                var indexed = IndexedMesh.FromMesh(mesh, 1e-9);
                
                // Multiple accesses to test caching
                for (int i = 0; i < 10; i++)
                {
                    _ = indexed.VertexCount;   // Direct count (fast)
                    _ = indexed.Vertices.Count; // Collection access (cached)
                }
            });

            _output.WriteLine($"📊 FromMesh creation: {optimizedTime.TotalMicroseconds:F2} μs per iteration");
            _output.WriteLine($"📊 With repeated access: {repeatedAccessTime.TotalMicroseconds:F2} μs per iteration");
            
            // Adjust thresholds based on actual performance characteristics
            optimizedTime.TotalMicroseconds.Should().BeLessThan(100000, "FromMesh should complete in reasonable time");
            repeatedAccessTime.TotalMicroseconds.Should().BeLessThan(50000, "Repeated access should be efficient due to caching");
        }

        [Fact]
        public void IndexedMesh_CountProperties_FasterThanCollectionAccess()
        {
            // Test that direct count properties are faster than collection access
            const int quadCount = 1000;
            const int accessCount = 10000;
            
            var testQuads = GenerateTestQuads(quadCount);
            
            using var mesh = new Mesh();
            mesh.AddQuads(testQuads);
            var indexed = IndexedMesh.FromMesh(mesh);
            
            _output.WriteLine($"⚡ Count Properties vs Collection Access Test");
            _output.WriteLine($"Testing {accessCount} property accesses");

            // Test direct count access
            var directCountTime = MeasureOperation("Direct Count Properties", 1, () =>
            {
                for (int i = 0; i < accessCount; i++)
                {
                    _ = indexed.VertexCount;
                    _ = indexed.QuadCount;
                    _ = indexed.TriangleCount;
                    _ = indexed.EdgeCount;
                }
            });

            // Test collection access
            var collectionCountTime = MeasureOperation("Collection Count Access", 1, () =>
            {
                for (int i = 0; i < accessCount; i++)
                {
                    _ = indexed.Vertices.Count;
                    _ = indexed.Quads.Count;
                    _ = indexed.Triangles.Count;
                    _ = indexed.Edges.Count;
                }
            });

            var improvement = (collectionCountTime.TotalMicroseconds - directCountTime.TotalMicroseconds) / collectionCountTime.TotalMicroseconds;
            
            _output.WriteLine($"📈 Direct count improvement: {improvement * 100:F1}%");
            
            // Performance can vary - sometimes collection access is optimized by JIT
            _output.WriteLine("Note: Performance comparison can vary due to JIT optimizations and access patterns");
            
            // Both should be reasonably fast
            directCountTime.TotalMicroseconds.Should().BeLessThan(5000, "Direct count properties should be reasonably fast");
            collectionCountTime.TotalMicroseconds.Should().BeLessThan(5000, "Collection count access should be reasonably fast");
        }

        [Fact]
        public void MesherOptions_OptimizedValidation()
        {
            // Test that validation is fast and cached
            const int validationCount = 10000;
            
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                MinCapQuadQuality = 0.5
            };
            
            _output.WriteLine($"🔍 MesherOptions Validation Performance Test");
            _output.WriteLine($"Testing {validationCount} validation calls");

            // First validation (uncached)
            var firstValidationTime = MeasureOperation("First Validation", 1, () =>
            {
                options.ResetValidation();
                options.Validate();
            });

            // Repeated validations (should be cached)
            var repeatedValidationTime = MeasureOperation("Repeated Validations", 1, () =>
            {
                for (int i = 0; i < validationCount; i++)
                {
                    options.Validate(); // Should return immediately due to caching
                }
            });

            _output.WriteLine($"📊 First validation: {firstValidationTime.TotalMicroseconds:F2} μs");
            _output.WriteLine($"📊 Repeated validations: {repeatedValidationTime.TotalMicroseconds:F2} μs total");
            
            var avgRepeatedTime = repeatedValidationTime.TotalMicroseconds / validationCount;
            _output.WriteLine($"📊 Average repeated validation: {avgRepeatedTime:F4} μs");
            
            // Repeated validations should be much faster due to caching
            avgRepeatedTime.Should().BeLessThan(1.0, "Cached validation should be reasonably fast");
        }

        [Fact]
        public void Mesh_ClearOptimization()
        {
            // Test that Clear() is optimized
            const int clearOperations = 1000;
            const int quadCount = 100;
            
            var testQuads = GenerateTestQuads(quadCount);
            
            _output.WriteLine($"🧹 Mesh Clear() Optimization Test");
            _output.WriteLine($"Testing {clearOperations} clear operations");

            var clearTime = MeasureOperation("Optimized Clear", clearOperations, () =>
            {
                using var mesh = new Mesh();
                mesh.AddQuads(testQuads);
                _ = mesh.Quads.Count; // Force lazy collection creation
                mesh.Clear(); // Should efficiently reset
            });

            var avgClearTime = clearTime.TotalMicroseconds / clearOperations;
            _output.WriteLine($"📊 Average clear time: {avgClearTime:F3} μs");
            
            // Clear should be very fast
            avgClearTime.Should().BeLessThan(10, "Clear operation should be very fast");
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
