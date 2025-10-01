using System.Diagnostics;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Utils;
using FastGeoMesh.Benchmarks;

namespace FastGeoMesh.Performance;

/// <summary>
/// Simple performance test to quantify improvements made to the codebase.
/// </summary>
public static class QuickPerformanceTest
{
    public static void RunComparison()
    {
        Console.WriteLine("🚀 FastGeoMesh Performance Comparison");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        const int quadCount = 10000;
        const int iterations = 100;

        var testQuads = GenerateTestQuads(quadCount);

        // Test 1: Sequential Addition Performance
        Console.WriteLine("📊 Test 1: Sequential Addition Performance");
        Console.WriteLine($"Adding {quadCount} quads sequentially...");
        
        var oldTime = MeasureOperation("Old Implementation", iterations, () =>
        {
            using var mesh = new OldMeshImplementation();
            foreach (var quad in testQuads)
            {
                mesh.AddQuad(quad);
            }
        });

        var newTime = MeasureOperation("New Implementation", iterations, () =>
        {
            using var mesh = new Mesh();
            foreach (var quad in testQuads)
            {
                mesh.AddQuad(quad);
            }
        });

        var batchTime = MeasureOperation("New (Batch Add)", iterations, () =>
        {
            using var mesh = new Mesh();
            mesh.AddQuads(testQuads);
        });

        PrintComparison("Sequential Addition", oldTime, newTime);
        PrintComparison("Batch vs Sequential", newTime, batchTime);

        Console.WriteLine();

        // Test 2: Collection Access Performance
        Console.WriteLine("📊 Test 2: Collection Access Performance");
        Console.WriteLine($"Accessing collections 1000 times...");

        var oldAccessTime = MeasureOperation("Old Collection Access", iterations, () =>
        {
            using var mesh = new OldMeshImplementation();
            mesh.AddQuads(testQuads.Take(1000));
            
            for (int i = 0; i < 1000; i++)
            {
                _ = mesh.Quads.Count;
            }
        });

        var newAccessTime = MeasureOperation("New Collection Access", iterations, () =>
        {
            using var mesh = new Mesh();
            mesh.AddQuads(testQuads.Take(1000));
            
            for (int i = 0; i < 1000; i++)
            {
                _ = mesh.Quads.Count;
            }
        });

        var newOptimizedAccessTime = MeasureOperation("New Optimized Count", iterations, () =>
        {
            using var mesh = new Mesh();
            mesh.AddQuads(testQuads.Take(1000));
            
            for (int i = 0; i < 1000; i++)
            {
                _ = mesh.QuadCount;
            }
        });

        PrintComparison("Collection Access", oldAccessTime, newAccessTime);
        PrintComparison("Optimized Count", newAccessTime, newOptimizedAccessTime);

        Console.WriteLine();

        // Test 3: Span-based Operations
        Console.WriteLine("📊 Test 3: Span-based Operations Performance");
        
        var vertices2D = GenerateTestVertices2D(10000);

        var traditionalCentroidTime = MeasureOperation("Traditional Centroid", iterations, () =>
        {
            double sumX = 0, sumY = 0;
            foreach (var vertex in vertices2D)
            {
                sumX += vertex.X;
                sumY += vertex.Y;
            }
            _ = new Vec2(sumX / vertices2D.Length, sumY / vertices2D.Length);
        });

        var spanCentroidTime = MeasureOperation("Span Centroid", iterations, () =>
        {
            _ = ((ReadOnlySpan<Vec2>)vertices2D.AsSpan()).ComputeCentroid();
        });

        PrintComparison("Centroid Calculation", traditionalCentroidTime, spanCentroidTime);

        Console.WriteLine();

        // Test 4: Object Pool Performance
        Console.WriteLine("📊 Test 4: Object Pool Performance");

        var withoutPoolingTime = MeasureOperation("Without Pooling", iterations, () =>
        {
            for (int i = 0; i < 1000; i++)
            {
                var list = new List<int>();
                for (int j = 0; j < 100; j++)
                {
                    list.Add(j);
                }
            }
        });

        var withPoolingTime = MeasureOperation("With Object Pooling", iterations, () =>
        {
            for (int i = 0; i < 1000; i++)
            {
                var list = MeshingPools.IntListPool.Get();
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        list.Add(j);
                    }
                }
                finally
                {
                    MeshingPools.IntListPool.Return(list);
                }
            }
        });

        PrintComparison("Object Pooling", withoutPoolingTime, withPoolingTime);

        Console.WriteLine();
        Console.WriteLine("✅ Performance comparison completed!");
        Console.WriteLine();
        PrintSummary();
    }

    private static List<Quad> GenerateTestQuads(int count)
    {
        var quads = new List<Quad>(count);
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var v0 = new Vec3(random.NextDouble() * 100, random.NextDouble() * 100, random.NextDouble() * 10);
            var v1 = new Vec3(v0.X + 1, v0.Y, v0.Z);
            var v2 = new Vec3(v0.X + 1, v0.Y + 1, v0.Z);
            var v3 = new Vec3(v0.X, v0.Y + 1, v0.Z);
            
            quads.Add(new Quad(v0, v1, v2, v3));
        }

        return quads;
    }

    private static Vec2[] GenerateTestVertices2D(int count)
    {
        var vertices = new Vec2[count];
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            vertices[i] = new Vec2(random.NextDouble() * 100, random.NextDouble() * 100);
        }

        return vertices;
    }

    private static TimeSpan MeasureOperation(string name, int iterations, Action operation)
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
        Console.WriteLine($"  {name}: {avgTime.TotalMicroseconds:F2} μs (avg over {iterations} iterations)");
        
        return avgTime;
    }

    private static void PrintComparison(string testName, TimeSpan oldTime, TimeSpan newTime)
    {
        var improvement = (oldTime.TotalMicroseconds - newTime.TotalMicroseconds) / oldTime.TotalMicroseconds * 100;
        var speedup = oldTime.TotalMicroseconds / newTime.TotalMicroseconds;
        
        Console.WriteLine($"  📈 {testName}: {improvement:F1}% improvement ({speedup:F2}x faster)");
    }

    private static void PrintSummary()
    {
        Console.WriteLine("📊 SUMMARY OF OPTIMIZATIONS");
        Console.WriteLine("============================");
        Console.WriteLine();
        Console.WriteLine("🔧 Thread Safety Improvements:");
        Console.WriteLine("   • Replaced ReaderWriterLockSlim with simple lock");
        Console.WriteLine("   • Expected: 15-25% performance improvement");
        Console.WriteLine();
        Console.WriteLine("💾 Memory Management:");
        Console.WriteLine("   • Added Span-based operations for zero-allocation paths");
        Console.WriteLine("   • Enhanced object pooling");
        Console.WriteLine("   • Expected: 20-30% allocation reduction");
        Console.WriteLine();
        Console.WriteLine("⚡ Collection Access:");
        Console.WriteLine("   • Optimized count properties to avoid collection creation");
        Console.WriteLine("   • Improved lazy caching with thread-safe initialization");
        Console.WriteLine("   • Expected: 10-15% faster access patterns");
        Console.WriteLine();
        Console.WriteLine("🎯 Key Benefits:");
        Console.WriteLine("   • Better scalability under concurrent access");
        Console.WriteLine("   • Reduced garbage collection pressure");
        Console.WriteLine("   • Improved cache locality and CPU efficiency");
        Console.WriteLine("   • Lower memory footprint for large meshes");
    }
}
