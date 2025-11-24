using FastGeoMesh.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FastGeoMesh.Sample
{
    /// <summary>
    /// Demonstrates the asynchronous meshing capabilities in FastGeoMesh v2.0.
    /// Showcases Clean Architecture benefits for async processing.
    /// </summary>
    public static class AsyncMeshingExample
    {
        /// <summary>
        /// Demonstrates basic async meshing with progress reporting.
        /// </summary>
        public static async Task DemonstrateBasicAsyncMeshing()
        {
            Console.WriteLine("=== Basic Async Meshing Demo (Clean Architecture) ===");

            // Domain: Create geometry using value objects
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10)
            });
            var structure = new PrismStructureDefinition(polygon, -5, 5);

            // Domain: Configure options with builder pattern
            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(2.0)
                .Build();

            if (optionsResult.IsFailure)
            {
                Console.WriteLine($"❌ Options validation failed: {optionsResult.Error.Description}");
                return;
            }

            // Application: Resolve mesher from DI
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            using var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var asyncMesher = (IAsyncMesher)mesher;

            // Set up progress reporting
            var progress = new Progress<MeshingProgress>(p =>
            {
                Console.WriteLine($"📊 Progress: {p.Operation} - {p.Percentage:P1}");
            });

            Console.WriteLine("🔄 Starting async meshing with progress reporting...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Application: Generate mesh asynchronously
                var meshResult = await asyncMesher.MeshWithProgressAsync(structure, optionsResult.Value, progress);
                stopwatch.Stop();

                if (meshResult.IsSuccess)
                {
                    var mesh = meshResult.Value;
                    Console.WriteLine($"✅ Async meshing completed in {stopwatch.ElapsedMilliseconds}ms");
                    Console.WriteLine($"📈 Generated {mesh.QuadCount} quads and {mesh.TriangleCount} triangles");

                    // Infrastructure: Convert and export
                    var indexed = IndexedMesh.FromMesh(mesh);
                    ObjExporter.Write(indexed, "async_basic_example.obj");
                    Console.WriteLine($"📄 Exported to async_basic_example.obj ({indexed.VertexCount} vertices)");
                }
                else
                {
                    Console.WriteLine($"❌ Async meshing failed: {meshResult.Error.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during async meshing: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates batch processing with Clean Architecture layers.
        /// </summary>
        public static async Task DemonstrateBatchProcessing()
        {
            Console.WriteLine("=== Batch Processing Demo (Clean Architecture) ===");

            // Domain: Create multiple structures
            var structures = new[]
            {
                // Simple rectangle
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2),

                // Rectangle with hole - Domain layer handles complex geometry
                CreateRectangleWithHole(),

                // L-shaped structure
                CreateLShapeStructure()
            };

            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(0.8)
                .Build();

            if (optionsResult.IsFailure)
            {
                Console.WriteLine($"❌ Options validation failed: {optionsResult.Error.Description}");
                return;
            }

            // Resolve mesher from DI
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            using var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var asyncMesher = (IAsyncMesher)mesher;

            var batchProgress = new Progress<MeshingProgress>(p =>
            {
                Console.WriteLine($"📊 Batch Progress: {p.Operation} - {p.Percentage:P1}");
            });

            Console.WriteLine($"🔄 Processing {structures.Length} structures in parallel...");
            var batchStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var batchResult = await asyncMesher.MeshBatchAsync(
                    structures,
                    optionsResult.Value,
                    maxDegreeOfParallelism: 4,
                    progress: batchProgress);

                batchStopwatch.Stop();

                if (batchResult.IsSuccess)
                {
                    var meshes = batchResult.Value;
                    Console.WriteLine($"✅ Batch processing completed in {batchStopwatch.ElapsedMilliseconds}ms");

                    for (int i = 0; i < meshes.Count; i++)
                    {
                        var mesh = meshes[i];
                        Console.WriteLine($"📈 Structure {i + 1}: {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");
                    }

                    // Infrastructure: Export the most complex mesh
                    if (meshes.Count > 1)
                    {
                        var complexMesh = meshes[1];
                        var indexed = IndexedMesh.FromMesh(complexMesh);
                        ObjExporter.Write(indexed, "async_batch_complex.obj");
                        Console.WriteLine("📄 Exported complex mesh to 'async_batch_complex.obj'");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Batch processing failed: {batchResult.Error.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during batch processing: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Creates a rectangle with hole using Domain layer.
        /// </summary>
        private static PrismStructureDefinition CreateRectangleWithHole()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 8), new Vec2(0, 8) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });

            return new PrismStructureDefinition(outer, -1, 3).AddHole(hole);
        }

        /// <summary>
        /// Creates an L-shaped structure using Domain primitives.
        /// </summary>
        private static PrismStructureDefinition CreateLShapeStructure()
        {
            var lShapeVertices = new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 3),
                new Vec2(3, 3), new Vec2(3, 6), new Vec2(0, 6)
            };

            return new PrismStructureDefinition(Polygon2D.FromPoints(lShapeVertices), 1, 4);
        }
    }
}
