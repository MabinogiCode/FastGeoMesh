using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Sample;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;

/// <summary>
/// Sample application demonstrating FastGeoMesh library usage with various export formats.
/// </summary>
sealed class Program
{
    /// <summary>
    /// Main entry point for the sample application.
    /// Demonstrates mesh generation and export capabilities including new async features.
    /// </summary>
    /// <param name="args">Command line arguments for controlling export formats and demos.</param>
    static async Task Main(string[] args)
    {
        Console.WriteLine("FastGeoMesh v1.4.0-preview Sample Application");
        Console.WriteLine("============================================\n");

        // Check for specific demo flags first
        if (args.Length > 0)
        {
            var firstArg = args[0].ToLowerInvariant();

            switch (firstArg)
            {
                case "--async":
                    Console.WriteLine("Running NEW Async Meshing Demonstrations:\n");
                    await RunAsyncDemonstrations();
                    return;

                case "--performance":
                    Console.WriteLine("Running PERFORMANCE Optimization Demonstrations:\n");
                    await PerformanceOptimizationExample.DemonstratePerformanceMonitoring();
                    await PerformanceOptimizationExample.DemonstrateOptimizedBatchProcessing();
                    await PerformanceOptimizationExample.DemonstrateAsyncOptimizations();
                    return;

                case "--benchmarks":
                    Console.WriteLine("Running COMPREHENSIVE Performance Benchmarks:\n");
                    await PerformanceBenchmarks.RunBenchmarkSuite();
                    return;
            }
        }

        // Default to legacy mode if no specific flag or --legacy
        Console.WriteLine("Running LEGACY Synchronous Demonstrations:\n");
        RunLegacyDemonstrations(args);

        Console.WriteLine("Sample application completed successfully!");
        Console.WriteLine("\nTry running with different flags:");
        Console.WriteLine("  --async        Run async meshing demonstrations");
        Console.WriteLine("  --legacy       Run legacy synchronous demonstrations");
        Console.WriteLine("  --obj          Export only OBJ format (legacy mode)");
        Console.WriteLine("  --gltf         Export only glTF format (legacy mode)");
        Console.WriteLine("  --svg          Export only SVG format (legacy mode)");
        Console.WriteLine("  --performance  Run performance optimization demonstrations");
        Console.WriteLine("  --benchmarks   Run comprehensive performance benchmarks");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates the new asynchronous meshing capabilities of v1.4.0.
    /// </summary>
    private static async Task RunAsyncDemonstrations()
    {
        try
        {
            // Basic async meshing
            await AsyncMeshingExample.DemonstrateBasicAsyncMeshing();

            // Batch processing
            await AsyncMeshingExample.DemonstrateBatchProcessing();

            // Complexity estimation
            await AsyncMeshingExample.DemonstrateComplexityEstimation();

            // Cancellation demo
            await AsyncMeshingExample.DemonstrateCancellation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in async demonstrations: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Runs the original synchronous demonstrations for compatibility.
    /// </summary>
    private static void RunLegacyDemonstrations(string[] args)
    {
        // Test our PointInPolygon fix
        TestPointInPolygon();

        bool exportObj = args.Contains("--obj", StringComparer.OrdinalIgnoreCase);
        bool exportGltf = args.Contains("--gltf", StringComparer.OrdinalIgnoreCase);
        bool exportSvg = args.Contains("--svg", StringComparer.OrdinalIgnoreCase);
        bool exportAll = !exportObj && !exportGltf && !exportSvg;

        var poly = Polygon2D.FromPoints(new[] {
            new Vec2(0, 0), new Vec2(20.0, 0), new Vec2(20.0, 5.0), new Vec2(0, 5.0)
        });
        var structure = new PrismStructureDefinition(poly, -10.0, 10.0);
        structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(20.0, 0)), 2.5);
        structure.Geometry
            .AddPoint(new Vec3(0, 4, 2))
            .AddPoint(new Vec3(20.0, 4, 4))
            .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20.0, 4, 2)));
        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 0.5,
            TargetEdgeLengthZ = 1.0
        };
        var mesh = new PrismMesher().Mesh(structure, options);
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
        Console.WriteLine($"Legacy Demo - Indexed: V={indexed.Vertices.Count}, E={indexed.Edges.Count}, Q={indexed.Quads.Count}");

        if (exportAll || exportObj)
        {
            ObjExporter.Write(indexed, "sample_mesh.obj");
            Console.WriteLine("Exported sample_mesh.obj");
        }
        if (exportAll || exportGltf)
        {
            GltfExporter.Write(indexed, "sample_mesh.gltf");
            Console.WriteLine("Exported sample_mesh.gltf");
        }
        if (exportAll || exportSvg)
        {
            SvgExporter.Write(indexed, "sample_mesh.svg");
            Console.WriteLine("Exported sample_mesh.svg");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Tests the PointInPolygon functionality to verify the fix is working correctly.
    /// </summary>
    static void TestPointInPolygon()
    {
        Console.WriteLine("=== Testing PointInPolygon Fix ===");

        var square = new Vec2[]
        {
            new(0, 0), new(10.0, 0), new(10.0, 10.0), new(0, 10.0)
        };

        // Test center point (5,5) - should be TRUE
        bool centerInside = GeometryHelper.PointInPolygon(square, 5.0, 5.0);
        Console.WriteLine($"Point (5,5) inside square: {centerInside} {(centerInside ? "✅" : "❌")}");

        // Test points on edges - should be TRUE
        bool cornerInside = GeometryHelper.PointInPolygon(square, 0.0, 0.0);
        Console.WriteLine($"Point (0,0) on corner: {cornerInside} {(cornerInside ? "✅" : "❌")}");

        bool edgeInside = GeometryHelper.PointInPolygon(square, 5.0, 0.0);
        Console.WriteLine($"Point (5,0) on edge: {edgeInside} {(edgeInside ? "✅" : "❌")}");

        // Test points outside - should be FALSE
        bool outsideLeft = GeometryHelper.PointInPolygon(square, -1.0, 5.0);
        Console.WriteLine($"Point (-1,5) outside left: {outsideLeft} {(!outsideLeft ? "✅" : "❌")}");

        bool outsideRight = GeometryHelper.PointInPolygon(square, 11.0, 5.0);
        Console.WriteLine($"Point (11,5) outside right: {outsideRight} {(!outsideRight ? "✅" : "❌")}");

        Console.WriteLine("=== Point-in-Polygon Test Complete ===\n");
    }
}
