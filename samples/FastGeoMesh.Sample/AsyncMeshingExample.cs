using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Sample
{
    /// <summary>
    /// Demonstrates the new asynchronous meshing capabilities introduced in FastGeoMesh v1.4.0.
    /// </summary>
    public static class AsyncMeshingExample
    {
        /// <summary>
        /// Demonstrates basic async meshing with progress reporting.
        /// </summary>
        public static async Task DemonstrateBasicAsyncMeshing()
        {
            Console.WriteLine("=== Basic Async Meshing Demo ===");

            // Create a simple rectangular structure
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10)
            });
            var structure = new PrismStructureDefinition(polygon, -5, 5);

            // Configure meshing options
            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(2.0)
                .Build();

            // Create mesher and cast to async interface
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Set up progress reporting
            var progress = new Progress<MeshingProgress>(p =>
            {
                Console.WriteLine($"Progress: {p}");
            });

            Console.WriteLine("Starting async meshing with progress reporting...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Generate mesh asynchronously with progress reporting
                var mesh = await asyncMesher.MeshWithProgressAsync(structure, options, progress);
                stopwatch.Stop();

                Console.WriteLine($"Async meshing completed in {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Generated {mesh.QuadCount} quads and {mesh.TriangleCount} triangles");

                // Convert to indexed mesh and export
                var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
                Console.WriteLine($"Final mesh: {indexed.VertexCount} vertices, {indexed.QuadCount} quads, {indexed.TriangleCount} triangles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during async meshing: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates batch processing of multiple structures with parallel execution.
        /// </summary>
        public static async Task DemonstrateBatchProcessing()
        {
            Console.WriteLine("=== Batch Processing Demo ===");

            // Create multiple structures of varying complexity
            var structures = new[]
            {
                // Simple rectangle
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2),
                
                // Rectangle with hole
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 8), new Vec2(0, 8) }),
                    -1, 3)
                    .AddHole(Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) })),
                
                // L-shaped structure
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] 
                    {
                        new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 3),
                        new Vec2(3, 3), new Vec2(3, 6), new Vec2(0, 6)
                    }),
                    1, 4),
                
                // Complex polygon with multiple holes
                new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(-10, -10), new Vec2(10, -10), new Vec2(10, 10), new Vec2(-10, 10) }),
                    0, 5)
                    .AddHole(Polygon2D.FromPoints(new[] { new Vec2(-3, -3), new Vec2(-1, -3), new Vec2(-1, -1), new Vec2(-3, -1) }))
                    .AddHole(Polygon2D.FromPoints(new[] { new Vec2(1, 1), new Vec2(3, 1), new Vec2(3, 3), new Vec2(1, 3) }))
            };

            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(0.8)
                .Build();

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Set up progress reporting for batch operation
            var batchProgress = new Progress<MeshingProgress>(p =>
            {
                Console.WriteLine($"Batch Progress: {p}");
            });

            Console.WriteLine($"Processing {structures.Length} structures in parallel...");
            var batchStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Process all structures in parallel with progress reporting
                var meshes = await asyncMesher.MeshBatchAsync(
                    structures, 
                    options, 
                    maxDegreeOfParallelism: 4, // Limit to 4 parallel operations
                    progress: batchProgress);

                batchStopwatch.Stop();

                Console.WriteLine($"Batch processing completed in {batchStopwatch.ElapsedMilliseconds}ms");
                
                for (int i = 0; i < meshes.Count; i++)
                {
                    var mesh = meshes[i];
                    Console.WriteLine($"Structure {i + 1}: {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");
                }

                // Export one of the complex meshes as an example
                if (meshes.Count > 1)
                {
                    var complexMesh = meshes[1]; // The one with holes
                    var indexed = IndexedMesh.FromMesh(complexMesh, options.Epsilon);
                    ObjExporter.Write(indexed, "async_batch_example.obj");
                    Console.WriteLine("Exported complex mesh to 'async_batch_example.obj'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch processing: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates complexity estimation before actual meshing.
        /// </summary>
        public static async Task DemonstrateComplexityEstimation()
        {
            Console.WriteLine("=== Complexity Estimation Demo ===");

            // Create structures of varying complexity
            var structures = new[]
            {
                ("Simple Rectangle", new PrismStructureDefinition(
                    Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) }),
                    0, 2)),
                
                ("Complex Polygon", CreateComplexPolygonStructure()),
                
                ("Multi-Hole Structure", CreateMultiHoleStructure())
            };

            var options = MesherOptions.CreateBuilder()
                .WithHighQualityPreset() // Use high quality for better complexity estimates
                .Build();

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            foreach (var (name, structure) in structures)
            {
                Console.WriteLine($"\nAnalyzing '{name}':");
                
                try
                {
                    // Get complexity estimate
                    var estimate = await asyncMesher.EstimateComplexityAsync(structure, options);
                    
                    Console.WriteLine($"  Complexity: {estimate.Complexity}");
                    Console.WriteLine($"  Estimated elements: {estimate.EstimatedQuadCount + estimate.EstimatedTriangleCount}");
                    Console.WriteLine($"  Estimated memory: {estimate.EstimatedPeakMemoryBytes / 1024.0 / 1024.0:F2} MB");
                    Console.WriteLine($"  Estimated time: {estimate.EstimatedComputationTime.TotalMilliseconds:F1} ms");
                    Console.WriteLine($"  Recommended parallelism: {estimate.RecommendedParallelism}");
                    
                    if (estimate.PerformanceHints.Count > 0)
                    {
                        Console.WriteLine("  Performance hints:");
                        foreach (var hint in estimate.PerformanceHints)
                        {
                            Console.WriteLine($"    - {hint}");
                        }
                    }

                    // Actually mesh and compare estimates
                    var actualStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var actualMesh = await asyncMesher.MeshAsync(structure, options);
                    actualStopwatch.Stop();

                    var actualElements = actualMesh.QuadCount + actualMesh.TriangleCount;
                    Console.WriteLine($"  Actual elements: {actualElements} (estimate accuracy: {(double)estimate.EstimatedQuadCount / actualElements * 100:F1}%)");
                    Console.WriteLine($"  Actual time: {actualStopwatch.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error: {ex.Message}");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates cancellation of long-running operations.
        /// </summary>
        public static async Task DemonstrateCancellation()
        {
            Console.WriteLine("=== Cancellation Demo ===");

            // Create a very complex structure that will take some time to mesh
            var complexStructure = CreateVeryComplexStructure();
            
            var options = MesherOptions.CreateBuilder()
                .WithHighQualityPreset()
                .WithTargetEdgeLengthXY(0.1) // Very fine mesh for slower processing
                .Build();

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            using var cts = new CancellationTokenSource();
            
            // Set up progress reporting
            var progress = new Progress<MeshingProgress>(p =>
            {
                Console.WriteLine($"Progress: {p}");
                
                // Cancel after reporting some progress (simulating user cancellation)
                if (p.Percentage > 0.1 && !cts.Token.IsCancellationRequested) // Cancel after 10% progress
                {
                    Console.WriteLine("Requesting cancellation...");
                    cts.Cancel();
                }
            });

            Console.WriteLine("Starting complex meshing operation with cancellation...");

            try
            {
                var mesh = await asyncMesher.MeshWithProgressAsync(complexStructure, options, progress, cts.Token);
                Console.WriteLine("Operation completed successfully (unexpected!)");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was successfully cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static PrismStructureDefinition CreateComplexPolygonStructure()
        {
            // Create a polygon with many vertices (simulating complex geometry)
            var vertices = new List<Vec2>();
            for (int i = 0; i < 50; i++)
            {
                double angle = 2 * Math.PI * i / 50;
                double radius = 10 + 3 * Math.Sin(5 * angle); // Flower-like shape
                vertices.Add(new Vec2(radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }
            
            var polygon = new Polygon2D(vertices);
            return new PrismStructureDefinition(polygon, -2, 8);
        }

        private static PrismStructureDefinition CreateMultiHoleStructure()
        {
            // Large outer polygon
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(-20, -20), new Vec2(20, -20), new Vec2(20, 20), new Vec2(-20, 20)
            });

            var structure = new PrismStructureDefinition(outer, 0, 10);

            // Add multiple holes
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    double x = -15 + i * 7.5;
                    double y = -15 + j * 7.5;
                    var hole = Polygon2D.FromPoints(new[]
                    {
                        new Vec2(x - 1, y - 1), new Vec2(x + 1, y - 1),
                        new Vec2(x + 1, y + 1), new Vec2(x - 1, y + 1)
                    });
                    structure = structure.AddHole(hole);
                }
            }

            return structure;
        }

        private static PrismStructureDefinition CreateVeryComplexStructure()
        {
            // This creates a structure complex enough to take measurable time
            var vertices = new List<Vec2>();
            for (int i = 0; i < 200; i++) // Many vertices
            {
                double angle = 2 * Math.PI * i / 200;
                double radius = 15 + 5 * Math.Sin(8 * angle) + 2 * Math.Cos(13 * angle);
                vertices.Add(new Vec2(radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }
            
            var polygon = new Polygon2D(vertices);
            var structure = new PrismStructureDefinition(polygon, -5, 15);

            // Add several holes to increase complexity
            for (int i = 0; i < 10; i++)
            {
                double angle = 2 * Math.PI * i / 10;
                double x = 8 * Math.Cos(angle);
                double y = 8 * Math.Sin(angle);
                
                var hole = Polygon2D.FromPoints(new[]
                {
                    new Vec2(x - 0.8, y - 0.8), new Vec2(x + 0.8, y - 0.8),
                    new Vec2(x + 0.8, y + 0.8), new Vec2(x - 0.8, y + 0.8)
                });
                structure = structure.AddHole(hole);
            }

            return structure;
        }
    }
}
