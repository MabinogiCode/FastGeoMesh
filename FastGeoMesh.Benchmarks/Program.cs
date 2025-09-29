using BenchmarkDotNet.Running;
using FastGeoMesh.Benchmarks.Geometry;
using FastGeoMesh.Benchmarks.Meshing;
using FastGeoMesh.Benchmarks.Utils;

namespace FastGeoMesh.Benchmarks;

/// <summary>
/// Entry point for FastGeoMesh performance benchmarks.
/// Measures the impact of .NET 8 optimizations implemented in the library.
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FastGeoMesh Performance Benchmarks - .NET 8 Optimizations");
        Console.WriteLine("===========================================================");
        Console.WriteLine();

        if (args.Length == 0)
        {
            Console.WriteLine("Available benchmark categories:");
            Console.WriteLine("  --geometry    : Vec2/Vec3 operations and geometric algorithms");
            Console.WriteLine("  --meshing     : Prism meshing and mesh generation");
            Console.WriteLine("  --utils       : Utility classes and helper functions");
            Console.WriteLine("  --collections : Collection optimizations (FrozenCollections)");
            Console.WriteLine("  --async       : Async patterns and ValueTask performance");
            Console.WriteLine("  --all         : Run all benchmarks");
            Console.WriteLine();
            Console.WriteLine("Example: dotnet run --configuration Release -- --geometry");
            return;
        }

        var category = args[0].ToLowerInvariant();
        
        switch (category)
        {
            case "--geometry":
                RunGeometryBenchmarks();
                break;
            case "--meshing":
                RunMeshingBenchmarks();
                break;
            case "--utils":
                RunUtilsBenchmarks();
                break;
            case "--collections":
                RunCollectionsBenchmarks();
                break;
            case "--async":
                RunAsyncBenchmarks();
                break;
            case "--all":
                RunAllBenchmarks();
                break;
            default:
                Console.WriteLine($"Unknown category: {category}");
                break;
        }
    }

    private static void RunGeometryBenchmarks()
    {
        Console.WriteLine("Running Geometry Benchmarks...");
        BenchmarkRunner.Run<Vec2OperationsBenchmark>();
        BenchmarkRunner.Run<Vec3OperationsBenchmark>();
        BenchmarkRunner.Run<GeometryHelperBenchmark>();
    }

    private static void RunMeshingBenchmarks()
    {
        Console.WriteLine("Running Meshing Benchmarks...");
        BenchmarkRunner.Run<PrismMeshingBenchmark>();
        BenchmarkRunner.Run<MeshingOptionsBenchmark>();
    }

    private static void RunUtilsBenchmarks()
    {
        Console.WriteLine("Running Utils Benchmarks...");
        BenchmarkRunner.Run<OptimizedConstantsBenchmark>();
        BenchmarkRunner.Run<ObjectPoolingBenchmark>();
    }

    private static void RunCollectionsBenchmarks()
    {
        Console.WriteLine("Running Collections Benchmarks...");
        BenchmarkRunner.Run<FrozenCollectionsBenchmark>();
    }

    private static void RunAsyncBenchmarks()
    {
        Console.WriteLine("Running Async Benchmarks...");
        BenchmarkRunner.Run<AsyncMeshingBenchmark>();
    }

    private static void RunAllBenchmarks()
    {
        Console.WriteLine("Running All Benchmarks...");
        RunGeometryBenchmarks();
        RunMeshingBenchmarks();
        RunUtilsBenchmarks();
        RunCollectionsBenchmarks();
        RunAsyncBenchmarks();
    }
}
