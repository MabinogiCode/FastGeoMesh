using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Sample
{
    /// <summary>
    /// Demonstrates the advanced performance monitoring and optimization features.
    /// </summary>
    public static class PerformanceOptimizationExample
    {
        /// <summary>
        /// Shows real-time performance monitoring during meshing operations.
        /// </summary>
        public static async Task DemonstratePerformanceMonitoring()
        {
            Console.WriteLine("=== Performance Monitoring Demo ===");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Create various complexity structures
            var structures = new[]
            {
                ("Trivial", CreateTrivialStructure()),
                ("Simple", CreateSimpleStructure()),
                ("Moderate", CreateModerateStructure()),
                ("Complex", CreateComplexStructure())
            };

            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build();

            Console.WriteLine("Testing different complexity levels with performance monitoring:\n");

            foreach (var (name, structure) in structures)
            {
                Console.WriteLine($"Processing {name} structure:");

                // Get baseline stats
                var statsBefore = await asyncMesher.GetLivePerformanceStatsAsync();
                
                // Estimate complexity
                var estimate = await asyncMesher.EstimateComplexityAsync(structure, options);
                Console.WriteLine($"  Estimated: {estimate.EstimatedComputationTime.TotalMicroseconds:F0}μs, " +
                                $"{estimate.EstimatedPeakMemoryBytes / 1024:F0}KB, " +
                                $"Complexity: {estimate.Complexity}");

                // Perform meshing with timing
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var mesh = await asyncMesher.MeshAsync(structure, options);
                stopwatch.Stop();

                // Get updated stats
                var statsAfter = await asyncMesher.GetLivePerformanceStatsAsync();
                
                Console.WriteLine($"  Actual: {stopwatch.Elapsed.TotalMicroseconds:F0}μs, " +
                                $"{mesh.QuadCount + mesh.TriangleCount} elements");
                Console.WriteLine($"  Pool efficiency: {statsAfter.PoolHitRate:P1} " +
                                $"(operations: +{statsAfter.MeshingOperations - statsBefore.MeshingOperations})");
                
                // Show accuracy
                var accuracy = estimate.EstimatedComputationTime.TotalMicroseconds / stopwatch.Elapsed.TotalMicroseconds;
                Console.WriteLine($"  Prediction accuracy: {accuracy:P1}");
                Console.WriteLine();
            }

            // Show final global stats
            var finalStats = await asyncMesher.GetLivePerformanceStatsAsync();
            Console.WriteLine($"Session totals:");
            Console.WriteLine($"  Total operations: {finalStats.MeshingOperations}");
            Console.WriteLine($"  Total quads: {finalStats.QuadsGenerated:N0}");
            Console.WriteLine($"  Total triangles: {finalStats.TrianglesGenerated:N0}");
            Console.WriteLine($"  Overall pool efficiency: {finalStats.PoolHitRate:P1}");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates optimized batch processing with adaptive parallelism.
        /// </summary>
        public static async Task DemonstrateOptimizedBatchProcessing()
        {
            Console.WriteLine("=== Optimized Batch Processing Demo ===");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Create different batch sizes to test optimization
            var batchSizes = new[] { 1, 4, 8, 16 };
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();

            foreach (var batchSize in batchSizes)
            {
                Console.WriteLine($"\nBatch size: {batchSize}");

                // Create structures of varying complexity
                var structures = new List<PrismStructureDefinition>();
                for (int i = 0; i < batchSize; i++)
                {
                    structures.Add((i % 4) switch
                    {
                        0 => CreateTrivialStructure(),
                        1 => CreateSimpleStructure(),
                        2 => CreateModerateStructure(),
                        _ => CreateComplexStructure()
                    });
                }

                // Test different parallelism settings
                var parallelismLevels = new[] { 1, 2, 4, -1 }; // -1 = unlimited
                
                foreach (var parallelism in parallelismLevels)
                {
                    var parallelismName = parallelism == -1 ? "unlimited" : parallelism.ToString();
                    
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var meshes = await asyncMesher.MeshBatchAsync(structures, options, parallelism);
                    stopwatch.Stop();

                    var totalElements = meshes.Sum(m => m.QuadCount + m.TriangleCount);
                    var throughput = totalElements / stopwatch.Elapsed.TotalMilliseconds * 1000; // elements/sec

                    Console.WriteLine($"  Parallelism {parallelismName}: {stopwatch.ElapsedMilliseconds}ms, " +
                                    $"{totalElements} elements, {throughput:F0} elem/sec");
                }
            }
        }

        /// <summary>
        /// Shows performance comparison between sync and async paths.
        /// </summary>
        public static async Task DemonstrateAsyncOptimizations()
        {
            Console.WriteLine("=== Async Performance Optimizations Demo ===");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();

            // Test structures of different complexities
            var testCases = new[]
            {
                ("Trivial (sync path optimized)", CreateTrivialStructure()),
                ("Simple", CreateSimpleStructure()),
                ("Moderate", CreateModerateStructure()),
                ("Complex", CreateComplexStructure())
            };

            foreach (var (name, structure) in testCases)
            {
                Console.WriteLine($"\n{name}:");

                // Test sync version
                var syncStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var syncMesh = mesher.Mesh(structure, options);
                syncStopwatch.Stop();

                // Test async version
                var asyncStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var asyncMesh = await asyncMesher.MeshAsync(structure, options);
                asyncStopwatch.Stop();

                Console.WriteLine($"  Sync:  {syncStopwatch.Elapsed.TotalMicroseconds:F0}μs");
                Console.WriteLine($"  Async: {asyncStopwatch.Elapsed.TotalMicroseconds:F0}μs");

                var overhead = (asyncStopwatch.Elapsed.TotalMicroseconds - syncStopwatch.Elapsed.TotalMicroseconds) 
                              / syncStopwatch.Elapsed.TotalMicroseconds * 100;
                
                Console.WriteLine($"  Async overhead: {overhead:+0.0;-0.0;0}%");

                // Verify results are identical
                var resultsMatch = syncMesh.QuadCount == asyncMesh.QuadCount && 
                                 syncMesh.TriangleCount == asyncMesh.TriangleCount;
                Console.WriteLine($"  Results match: {(resultsMatch ? "✅" : "❌")}");
            }
        }

        private static PrismStructureDefinition CreateTrivialStructure()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2)
            });
            return new PrismStructureDefinition(polygon, 0, 1);
        }

        private static PrismStructureDefinition CreateSimpleStructure()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            return new PrismStructureDefinition(polygon, 0, 3);
        }

        private static PrismStructureDefinition CreateModerateStructure()
        {
            // L-shaped polygon
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 8),
                new Vec2(8, 8), new Vec2(8, 15), new Vec2(0, 15)
            });
            
            var structure = new PrismStructureDefinition(polygon, -2, 8);
            
            // Add a hole
            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(3, 3), new Vec2(6, 3), new Vec2(6, 6), new Vec2(3, 6)
            });
            structure = structure.AddHole(hole);
            
            return structure;
        }

        private static PrismStructureDefinition CreateComplexStructure()
        {
            // Complex polygon with many vertices
            var vertices = new List<Vec2>();
            for (int i = 0; i < 32; i++)
            {
                double angle = 2 * Math.PI * i / 32;
                double radius = 10 + 3 * Math.Sin(4 * angle); // Star-like shape
                vertices.Add(new Vec2(radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }
            
            var polygon = new Polygon2D(vertices);
            var structure = new PrismStructureDefinition(polygon, -5, 10);
            
            // Add multiple holes
            for (int i = 0; i < 3; i++)
            {
                double x = 4 * Math.Cos(2 * Math.PI * i / 3);
                double y = 4 * Math.Sin(2 * Math.PI * i / 3);
                var hole = Polygon2D.FromPoints(new[]
                {
                    new Vec2(x - 1, y - 1), new Vec2(x + 1, y - 1),
                    new Vec2(x + 1, y + 1), new Vec2(x - 1, y + 1)
                });
                structure = structure.AddHole(hole);
            }
            
            return structure;
        }
    }
}
