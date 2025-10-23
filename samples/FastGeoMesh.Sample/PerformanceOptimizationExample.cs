using FastGeoMesh.Application.Services;

namespace FastGeoMesh.Sample {
    /// <summary>
    /// Demonstrates performance optimization techniques available in FastGeoMesh v2.0.
    /// Showcases Clean Architecture benefits and async processing capabilities.
    /// </summary>
    public static class PerformanceOptimizationExample {
        /// <summary>
        /// Demonstrates memory-efficient processing with Clean Architecture.
        /// </summary>
        public static void DemonstrateMemoryEfficiency() {
            Console.WriteLine("🧠 Memory Efficiency with Clean Architecture");

            // Domain layer provides immutable, efficient data structures
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(100, 0), new Vec2(100, 100), new Vec2(0, 100)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 50);

            // Application layer handles efficient meshing algorithms
            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(2.0)
                .Build();

            if (optionsResult.IsSuccess) {
                var mesher = new PrismMesher();
                var meshResult = mesher.Mesh(structure, optionsResult.Value);

                if (meshResult.IsSuccess) {
                    var mesh = meshResult.Value;
                    Console.WriteLine($"✅ Efficient mesh: {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");
                }
            }
        }

        /// <summary>
        /// Demonstrates async processing advantages in v2.0.
        /// </summary>
        public static async Task DemonstrateAsyncPerformance() {
            Console.WriteLine("⚡ Async Performance with Clean Architecture");

            var structures = CreateTestStructures();
            var optionsResult = MesherOptions.CreateBuilder().WithFastPreset().Build();

            if (optionsResult.IsSuccess) {
                var mesher = new PrismMesher();
                var asyncMesher = (IAsyncMesher)mesher;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var batchResult = await asyncMesher.MeshBatchAsync(structures, optionsResult.Value);
                stopwatch.Stop();

                if (batchResult.IsSuccess) {
                    Console.WriteLine($"✅ Batch processing: {batchResult.Value.Count} meshes in {stopwatch.ElapsedMilliseconds}ms");
                }
            }
        }

        private static PrismStructureDefinition[] CreateTestStructures() {
            return new[]
            {
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2),
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(7, 0), new Vec2(7, 7), new Vec2(0, 7) }),
                    0, 3),
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                    0, 4)
            };
        }
    }
}
