using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using System.Diagnostics;

namespace FastGeoMesh.Sample
{
    /// <summary>
    /// High-precision performance benchmarks for v1.4.0 optimizations.
    /// Measures the impact of our async optimizations, memory pooling, and monitoring.
    /// </summary>
    public static class PerformanceBenchmarks
    {
        /// <summary>
        /// Runs comprehensive benchmarks to validate v1.4.0 performance improvements.
        /// </summary>
        public static async Task RunBenchmarkSuite()
        {
            Console.WriteLine("🔥 FastGeoMesh v1.4.0 Performance Benchmarks");
            Console.WriteLine("============================================");
            Console.WriteLine();

            await BenchmarkSyncVsAsync();
            await BenchmarkBatchProcessing();
            await BenchmarkMonitoringOverhead();
            await BenchmarkTrivialOptimization();
            await BenchmarkProgressReporting();

            Console.WriteLine("✅ All benchmarks completed!");
        }

        /// <summary>
        /// Benchmarks sync vs async performance to validate our optimizations.
        /// </summary>
        private static async Task BenchmarkSyncVsAsync()
        {
            Console.WriteLine("🔬 Sync vs Async Performance Benchmark");
            Console.WriteLine("=====================================");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();

            var testCases = new[]
            {
                ("Trivial (4 vertices)", CreateTrivialStructure()),
                ("Simple (4 vertices, larger)", CreateSimpleStructure()),
                ("Moderate (L-shape + hole)", CreateModerateStructure()),
                ("Complex (64 vertices + holes)", CreateComplexStructure())
            };

            const int iterations = 100; // More iterations for better precision

            foreach (var (name, structure) in testCases)
            {
                Console.WriteLine($"\n{name}:");

                // Warmup
                for (int i = 0; i < 10; i++)
                {
                    _ = mesher.Mesh(structure, options);
                    _ = await asyncMesher.MeshAsync(structure, options);
                }

                // Benchmark Sync
                var syncStopwatch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    _ = mesher.Mesh(structure, options);
                }
                syncStopwatch.Stop();
                var syncAvg = syncStopwatch.Elapsed.TotalMicroseconds / iterations;

                // Benchmark Async
                var asyncStopwatch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    _ = await asyncMesher.MeshAsync(structure, options);
                }
                asyncStopwatch.Stop();
                var asyncAvg = asyncStopwatch.Elapsed.TotalMicroseconds / iterations;

                // Calculate overhead
                var overhead = ((asyncAvg - syncAvg) / syncAvg) * 100;
                var throughput = 1_000_000 / syncAvg; // operations per second

                Console.WriteLine($"  Sync:  {syncAvg:F1}μs avg  ({throughput:F0} ops/sec)");
                Console.WriteLine($"  Async: {asyncAvg:F1}μs avg  ({overhead:+0.0;-0.0;±0}% overhead)");

                // Validate our trivial optimization
                if (name.Contains("Trivial") && overhead > 50)
                {
                    Console.WriteLine($"  ⚠️  Trivial optimization may not be working (high overhead)");
                }
                else if (overhead < 10)
                {
                    Console.WriteLine($"  ✅ Excellent async efficiency");
                }
            }
        }

        /// <summary>
        /// Benchmarks batch processing performance vs sequential processing.
        /// </summary>
        private static async Task BenchmarkBatchProcessing()
        {
            Console.WriteLine("\n\n🔬 Batch Processing Performance Benchmark");
            Console.WriteLine("========================================");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();

            var batchSizes = new[] { 4, 8, 16, 32 };
            
            foreach (var batchSize in batchSizes)
            {
                Console.WriteLine($"\nBatch size: {batchSize}");

                // Create mixed complexity batch
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

                const int iterations = 10;

                // Warmup
                for (int i = 0; i < 3; i++)
                {
                    _ = await asyncMesher.MeshBatchAsync(structures, options);
                }

                // Benchmark Sequential
                var seqStopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations; iter++)
                {
                    var results = new List<Mesh>();
                    foreach (var structure in structures)
                    {
                        results.Add(await asyncMesher.MeshAsync(structure, options));
                    }
                }
                seqStopwatch.Stop();
                var seqAvg = seqStopwatch.Elapsed.TotalMilliseconds / iterations;

                // Benchmark Parallel (default parallelism)
                var parStopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations; iter++)
                {
                    _ = await asyncMesher.MeshBatchAsync(structures, options);
                }
                parStopwatch.Stop();
                var parAvg = parStopwatch.Elapsed.TotalMilliseconds / iterations;

                // Benchmark Parallel (limited parallelism)
                var par2Stopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations; iter++)
                {
                    _ = await asyncMesher.MeshBatchAsync(structures, options, maxDegreeOfParallelism: 2);
                }
                par2Stopwatch.Stop();
                var par2Avg = par2Stopwatch.Elapsed.TotalMilliseconds / iterations;

                var speedupUnlimited = seqAvg / parAvg;
                var speedupLimited = seqAvg / par2Avg;
                var throughput = batchSize / seqAvg * 1000; // structures per second

                Console.WriteLine($"  Sequential:        {seqAvg:F1}ms ({throughput:F0} struct/sec)");
                Console.WriteLine($"  Parallel (auto):   {parAvg:F1}ms ({speedupUnlimited:F1}x speedup)");
                Console.WriteLine($"  Parallel (max=2):  {par2Avg:F1}ms ({speedupLimited:F1}x speedup)");

                if (speedupUnlimited > 1.5)
                {
                    Console.WriteLine($"  ✅ Good parallel scaling");
                }
            }
        }

        /// <summary>
        /// Benchmarks performance monitoring overhead.
        /// </summary>
        private static async Task BenchmarkMonitoringOverhead()
        {
            Console.WriteLine("\n\n🔬 Performance Monitoring Overhead Benchmark");
            Console.WriteLine("==========================================");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var structure = CreateSimpleStructure();

            const int iterations = 1000;

            // Benchmark stats retrieval
            var statsStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = await asyncMesher.GetLivePerformanceStatsAsync();
            }
            statsStopwatch.Stop();
            var statsAvg = statsStopwatch.Elapsed.TotalNanoseconds / iterations;

            // Benchmark complexity estimation
            var estimateStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = await asyncMesher.EstimateComplexityAsync(structure, options);
            }
            estimateStopwatch.Stop();
            var estimateAvg = estimateStopwatch.Elapsed.TotalMicroseconds / iterations;

            Console.WriteLine($"Performance stats retrieval: {statsAvg:F0}ns per call");
            Console.WriteLine($"Complexity estimation:       {estimateAvg:F1}μs per call");

            if (statsAvg < 1000) // < 1μs
            {
                Console.WriteLine("✅ Stats retrieval overhead is negligible");
            }
            if (estimateAvg < 10) // < 10μs
            {
                Console.WriteLine("✅ Complexity estimation overhead is minimal");
            }
        }

        /// <summary>
        /// Validates trivial structure optimization.
        /// </summary>
        private static async Task BenchmarkTrivialOptimization()
        {
            Console.WriteLine("\n\n🔬 Trivial Structure Optimization Benchmark");
            Console.WriteLine("=========================================");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var trivialStructure = CreateTrivialStructure();

            const int iterations = 1000;

            // Warmup
            for (int i = 0; i < 100; i++)
            {
                _ = mesher.Mesh(trivialStructure, options);
                _ = await asyncMesher.MeshAsync(trivialStructure, options);
            }

            // Benchmark without cancellation token (should use sync path)
            var optimizedStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = await asyncMesher.MeshAsync(trivialStructure, options);
            }
            optimizedStopwatch.Stop();
            var optimizedAvg = optimizedStopwatch.Elapsed.TotalMicroseconds / iterations;

            // Benchmark with cancellation token (should use async path)
            using var cts = new CancellationTokenSource();
            var asyncStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = await asyncMesher.MeshAsync(trivialStructure, options, cts.Token);
            }
            asyncStopwatch.Stop();
            var asyncAvg = asyncStopwatch.Elapsed.TotalMicroseconds / iterations;

            // Benchmark pure sync
            var syncStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = mesher.Mesh(trivialStructure, options);
            }
            syncStopwatch.Stop();
            var syncAvg = syncStopwatch.Elapsed.TotalMicroseconds / iterations;

            var optimizedOverhead = ((optimizedAvg - syncAvg) / syncAvg) * 100;
            var asyncOverhead = ((asyncAvg - syncAvg) / syncAvg) * 100;

            Console.WriteLine($"Pure sync:              {syncAvg:F1}μs");
            Console.WriteLine($"Async (optimized path): {optimizedAvg:F1}μs ({optimizedOverhead:+0.0;-0.0;±0}% overhead)");
            Console.WriteLine($"Async (full async):     {asyncAvg:F1}μs ({asyncOverhead:+0.0;-0.0;±0}% overhead)");

            if (optimizedOverhead < 20) // Less than 20% overhead
            {
                Console.WriteLine("✅ Trivial structure optimization is working well");
            }
            else
            {
                Console.WriteLine("⚠️  Trivial structure optimization needs improvement");
            }
        }

        /// <summary>
        /// Benchmarks progress reporting overhead.
        /// </summary>
        private static async Task BenchmarkProgressReporting()
        {
            Console.WriteLine("\n\n🔬 Progress Reporting Overhead Benchmark");
            Console.WriteLine("======================================");

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
            var structure = CreateModerateStructure();

            const int iterations = 50;

            // Benchmark without progress
            var withoutProgressStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = await asyncMesher.MeshWithProgressAsync(structure, options, null);
            }
            withoutProgressStopwatch.Stop();
            var withoutProgressAvg = withoutProgressStopwatch.Elapsed.TotalMilliseconds / iterations;

            // Benchmark with progress
            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));
            
            var withProgressStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = await asyncMesher.MeshWithProgressAsync(structure, options, progress);
            }
            withProgressStopwatch.Stop();
            var withProgressAvg = withProgressStopwatch.Elapsed.TotalMilliseconds / iterations;

            var overhead = ((withProgressAvg - withoutProgressAvg) / withoutProgressAvg) * 100;

            Console.WriteLine($"Without progress: {withoutProgressAvg:F1}ms");
            Console.WriteLine($"With progress:    {withProgressAvg:F1}ms ({overhead:+0.0;-0.0;±0}% overhead)");
            Console.WriteLine($"Progress reports: ~{progressReports.Count / iterations} per operation");

            if (overhead < 10) // Less than 10% overhead
            {
                Console.WriteLine("✅ Progress reporting overhead is acceptable");
            }
        }

        // Helper methods to create test structures
        private static PrismStructureDefinition CreateTrivialStructure()
        {
            return new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] 
                { 
                    new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) 
                }), 
                0, 1);
        }

        private static PrismStructureDefinition CreateSimpleStructure()
        {
            return new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] 
                { 
                    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) 
                }), 
                0, 3);
        }

        private static PrismStructureDefinition CreateModerateStructure()
        {
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 8),
                new Vec2(8, 8), new Vec2(8, 15), new Vec2(0, 15)
            });
            
            return new PrismStructureDefinition(lShape, -2, 8)
                .AddHole(Polygon2D.FromPoints(new[]
                {
                    new Vec2(3, 3), new Vec2(6, 3), new Vec2(6, 6), new Vec2(3, 6)
                }));
        }

        private static PrismStructureDefinition CreateComplexStructure()
        {
            var vertices = new List<Vec2>();
            for (int i = 0; i < 32; i++)
            {
                double angle = 2 * Math.PI * i / 32;
                double radius = 10 + 3 * Math.Sin(4 * angle);
                vertices.Add(new Vec2(radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }
            
            var structure = new PrismStructureDefinition(new Polygon2D(vertices), -5, 10);
            
            for (int i = 0; i < 2; i++)
            {
                double x = 4 * Math.Cos(2 * Math.PI * i / 2);
                double y = 4 * Math.Sin(2 * Math.PI * i / 2);
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
