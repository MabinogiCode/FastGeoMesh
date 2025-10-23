using FastGeoMesh.Application.Services;
using FastGeoMesh.Infrastructure;

namespace FastGeoMesh.Sample
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("🚀 FastGeoMesh v2.0 - Clean Architecture Sample");
            Console.WriteLine("===========================================");

            try
            {
                SimpleRectangleExample();
                ComplexPolygonExample();
                HoleExample();
                await AsyncExample();

                Console.WriteLine("\n✅ All examples completed successfully!");
                Console.WriteLine("📂 Output files: simple_rectangle.obj, l_shape.obj, polygon_with_hole.obj, async_mesh.obj");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"🔍 Stack: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void SimpleRectangleExample()
        {
            Console.WriteLine("\n🔵 Domain Layer - Simple Rectangle");
            Console.WriteLine("----------------------------------");

            // 1. Domain: Create geometry using domain entities
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, -2, 3);

            // 2. Domain: Configure meshing options with builder pattern
            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(1.0)
                .Build();

            if (optionsResult.IsFailure)
            {
                Console.WriteLine($"❌ Options validation failed: {optionsResult.Error.Description}");
                return;
            }

            // 3. Application: Generate mesh with clean error handling
            var mesher = new PrismMesher();
            var meshResult = mesher.Mesh(structure, optionsResult.Value);

            if (meshResult.IsFailure)
            {
                Console.WriteLine($"❌ Meshing failed: {meshResult.Error.Description}");
                return;
            }

            var mesh = meshResult.Value;
            Console.WriteLine($"✅ Generated mesh: {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");

            // 4. Infrastructure: Export using infrastructure services
            var indexed = IndexedMesh.FromMesh(mesh);
            ObjExporter.Write(indexed, "simple_rectangle.obj");
            Console.WriteLine("📄 Exported to simple_rectangle.obj");
        }

        static void ComplexPolygonExample()
        {
            Console.WriteLine("\n🟡 Application Layer - Complex L-Shape");
            Console.WriteLine("--------------------------------------");

            // Create L-shaped polygon with domain types
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 3),
                new Vec2(3, 3), new Vec2(3, 6), new Vec2(0, 6)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 4);

            // Use high quality preset for better results
            var optionsResult = MesherOptions.CreateBuilder()
                .WithHighQualityPreset()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.8)
                .Build();

            if (optionsResult.IsFailure)
            {
                Console.WriteLine($"❌ Options validation failed: {optionsResult.Error.Description}");
                return;
            }

            // Application layer handles complex meshing
            var mesher = new PrismMesher();
            var meshResult = mesher.Mesh(structure, optionsResult.Value);

            if (meshResult.IsSuccess)
            {
                var mesh = meshResult.Value;
                Console.WriteLine($"✅ L-shaped mesh: {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");

                var indexed = IndexedMesh.FromMesh(mesh);
                ObjExporter.Write(indexed, "l_shape.obj");
                Console.WriteLine("📄 Exported L-shape to l_shape.obj");
            }
            else
            {
                Console.WriteLine($"❌ Failed to generate L-shaped mesh: {meshResult.Error.Description}");
            }
        }

        static void HoleExample()
        {
            Console.WriteLine("\n🟢 Infrastructure Layer - Polygon with Hole");
            Console.WriteLine("-------------------------------------------");

            // Create outer polygon and hole using domain primitives
            var outerPolygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 6), new Vec2(0, 6)
            });

            var holePolygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(2, 2), new Vec2(6, 2), new Vec2(6, 4), new Vec2(2, 4)
            });

            // Domain: Build complex structure with hole
            var structure = new PrismStructureDefinition(outerPolygon, -1, 2)
                .AddHole(holePolygon);

            // Configure hole refinement for better quality near holes
            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(0.4)
                .WithHoleRefinement(0.2, 1.0) // Finer mesh near holes
                .Build();

            if (optionsResult.IsFailure)
            {
                Console.WriteLine($"❌ Options validation failed: {optionsResult.Error.Description}");
                return;
            }

            var mesher = new PrismMesher();
            var meshResult = mesher.Mesh(structure, optionsResult.Value);

            if (meshResult.IsSuccess)
            {
                var mesh = meshResult.Value;
                Console.WriteLine($"✅ Polygon with hole: {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");

                // Infrastructure: High-performance export
                var indexed = IndexedMesh.FromMesh(mesh);
                ObjExporter.Write(indexed, "polygon_with_hole.obj");
                Console.WriteLine("📄 Exported polygon with hole to polygon_with_hole.obj");
            }
            else
            {
                Console.WriteLine($"❌ Failed to generate mesh with hole: {meshResult.Error.Description}");
            }
        }

        static async System.Threading.Tasks.Task AsyncExample()
        {
            Console.WriteLine("\n⚡ Async Performance - Batch Processing");
            Console.WriteLine("--------------------------------------");

            // Create multiple structures for batch processing
            var structures = new[]
            {
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) }),
                    0, 1),
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3) }),
                    0, 1),
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 4), new Vec2(0, 4) }),
                    0, 1)
            };

            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(0.5)
                .Build();

            if (optionsResult.IsFailure)
            {
                Console.WriteLine($"❌ Options validation failed: {optionsResult.Error.Description}");
                return;
            }

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Demonstrate async batch processing with progress
            var progress = new Progress<MeshingProgress>(p =>
                Console.WriteLine($"  📊 {p.Operation}: {p.Percentage:P1}"));

            Console.WriteLine("🔄 Processing 3 structures in parallel...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var batchResult = await asyncMesher.MeshBatchAsync(
                structures,
                optionsResult.Value,
                maxDegreeOfParallelism: 4,
                progress: progress);

            stopwatch.Stop();

            if (batchResult.IsSuccess)
            {
                var meshes = batchResult.Value;
                Console.WriteLine($"✅ Batch processing completed in {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"📈 Generated {meshes.Count} meshes with total {meshes.Sum(m => m.QuadCount)} quads");

                // Export the first mesh as example
                if (meshes.Count > 0)
                {
                    var indexed = IndexedMesh.FromMesh(meshes[0]);
                    ObjExporter.Write(indexed, "async_mesh.obj");
                    Console.WriteLine("📄 Exported async result to async_mesh.obj");
                }
            }
            else
            {
                Console.WriteLine($"❌ Batch processing failed: {batchResult.Error.Description}");
            }
        }
    }
}
