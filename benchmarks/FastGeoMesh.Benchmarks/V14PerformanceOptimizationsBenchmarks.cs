using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>
    /// Benchmarks for v1.4.0 performance optimizations.
    /// Tests async vs sync paths, trivial structure optimization, and monitoring overhead.
    /// </summary>
    [Config(typeof(PerformanceOptimizationConfig))]
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class V14PerformanceOptimizationsBenchmarks
    {
        private PrismMesher _mesher = null!;
        private IAsyncMesher _asyncMesher = null!;
        private MesherOptions _fastOptions = null!;
        
        // Test structures
        private PrismStructureDefinition _trivialStructure = null!;
        private PrismStructureDefinition _simpleStructure = null!;
        private PrismStructureDefinition _moderateStructure = null!;
        private PrismStructureDefinition _complexStructure = null!;
        
        // Batch test data
        private List<PrismStructureDefinition> _batchSmall = null!;
        private List<PrismStructureDefinition> _batchMedium = null!;
        private List<PrismStructureDefinition> _batchLarge = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _mesher = new PrismMesher();
            _asyncMesher = (IAsyncMesher)_mesher;
            
            _fastOptions = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build();

            // Setup test structures
            SetupTestStructures();
            SetupBatchTestData();
        }

        private void SetupTestStructures()
        {
            // Trivial structure (< 10 vertices)
            _trivialStructure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] 
                { 
                    new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) 
                }), 
                0, 1);

            // Simple structure (< 50 vertices)
            _simpleStructure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] 
                { 
                    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5) 
                }), 
                0, 3);

            // Moderate structure (< 200 vertices) - L-shape with hole
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 8),
                new Vec2(8, 8), new Vec2(8, 15), new Vec2(0, 15)
            });
            _moderateStructure = new PrismStructureDefinition(lShape, -2, 8)
                .AddHole(Polygon2D.FromPoints(new[]
                {
                    new Vec2(3, 3), new Vec2(6, 3), new Vec2(6, 6), new Vec2(3, 6)
                }));

            // Complex structure (> 200 vertices) - Star shape with multiple holes
            var vertices = new List<Vec2>();
            for (int i = 0; i < 64; i++)
            {
                double angle = 2 * Math.PI * i / 64;
                double radius = 10 + 3 * Math.Sin(4 * angle);
                vertices.Add(new Vec2(radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }
            
            _complexStructure = new PrismStructureDefinition(new Polygon2D(vertices), -5, 10);
            for (int i = 0; i < 4; i++)
            {
                double x = 4 * Math.Cos(2 * Math.PI * i / 4);
                double y = 4 * Math.Sin(2 * Math.PI * i / 4);
                var hole = Polygon2D.FromPoints(new[]
                {
                    new Vec2(x - 1, y - 1), new Vec2(x + 1, y - 1),
                    new Vec2(x + 1, y + 1), new Vec2(x - 1, y + 1)
                });
                _complexStructure = _complexStructure.AddHole(hole);
            }
        }

        private void SetupBatchTestData()
        {
            _batchSmall = Enumerable.Range(0, 4)
                .Select(i => i % 2 == 0 ? _trivialStructure : _simpleStructure)
                .ToList();

            _batchMedium = Enumerable.Range(0, 16)
                .Select(i => (i % 4) switch
                {
                    0 => _trivialStructure,
                    1 => _simpleStructure,
                    2 => _moderateStructure,
                    _ => _complexStructure
                })
                .ToList();

            _batchLarge = Enumerable.Range(0, 64)
                .Select(i => (i % 4) switch
                {
                    0 => _trivialStructure,
                    1 => _simpleStructure,
                    2 => _moderateStructure,
                    _ => _complexStructure
                })
                .ToList();
        }

        // =================== SYNC VS ASYNC COMPARISON ===================

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("SyncVsAsync", "Trivial")]
        public Mesh Trivial_Sync()
        {
            return _mesher.Mesh(_trivialStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Trivial")]
        public async ValueTask<Mesh> Trivial_Async()
        {
            return await _asyncMesher.MeshAsync(_trivialStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Simple")]
        public Mesh Simple_Sync()
        {
            return _mesher.Mesh(_simpleStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Simple")]
        public async ValueTask<Mesh> Simple_Async()
        {
            return await _asyncMesher.MeshAsync(_simpleStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Moderate")]
        public Mesh Moderate_Sync()
        {
            return _mesher.Mesh(_moderateStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Moderate")]
        public async ValueTask<Mesh> Moderate_Async()
        {
            return await _asyncMesher.MeshAsync(_moderateStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Complex")]
        public Mesh Complex_Sync()
        {
            return _mesher.Mesh(_complexStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("SyncVsAsync", "Complex")]
        public async ValueTask<Mesh> Complex_Async()
        {
            return await _asyncMesher.MeshAsync(_complexStructure, _fastOptions);
        }

        // =================== PERFORMANCE MONITORING OVERHEAD ===================

        [Benchmark]
        [BenchmarkCategory("Monitoring", "Overhead")]
        public async ValueTask<PerformanceMonitor.PerformanceStatistics> GetPerformanceStats()
        {
            return await _asyncMesher.GetLivePerformanceStatsAsync();
        }

        [Benchmark]
        [BenchmarkCategory("Monitoring", "Estimation")]
        public async ValueTask<MeshingComplexityEstimate> EstimateComplexity_Simple()
        {
            return await _asyncMesher.EstimateComplexityAsync(_simpleStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("Monitoring", "Estimation")]
        public async ValueTask<MeshingComplexityEstimate> EstimateComplexity_Complex()
        {
            return await _asyncMesher.EstimateComplexityAsync(_complexStructure, _fastOptions);
        }

        // =================== BATCH PROCESSING PERFORMANCE ===================

        [Benchmark]
        [BenchmarkCategory("Batch", "Small")]
        public async ValueTask<IReadOnlyList<Mesh>> Batch_Small_Sequential()
        {
            var results = new Mesh[_batchSmall.Count];
            for (int i = 0; i < _batchSmall.Count; i++)
            {
                results[i] = await _asyncMesher.MeshAsync(_batchSmall[i], _fastOptions);
            }
            return results;
        }

        [Benchmark]
        [BenchmarkCategory("Batch", "Small")]
        public async ValueTask<IReadOnlyList<Mesh>> Batch_Small_Parallel()
        {
            return await _asyncMesher.MeshBatchAsync(_batchSmall, _fastOptions, maxDegreeOfParallelism: 4);
        }

        [Benchmark]
        [BenchmarkCategory("Batch", "Medium")]
        public async ValueTask<IReadOnlyList<Mesh>> Batch_Medium_Sequential()
        {
            var results = new Mesh[_batchMedium.Count];
            for (int i = 0; i < _batchMedium.Count; i++)
            {
                results[i] = await _asyncMesher.MeshAsync(_batchMedium[i], _fastOptions);
            }
            return results;
        }

        [Benchmark]
        [BenchmarkCategory("Batch", "Medium")]
        public async ValueTask<IReadOnlyList<Mesh>> Batch_Medium_Parallel()
        {
            return await _asyncMesher.MeshBatchAsync(_batchMedium, _fastOptions, maxDegreeOfParallelism: 4);
        }

        [Benchmark]
        [BenchmarkCategory("Batch", "Large")]
        public async ValueTask<IReadOnlyList<Mesh>> Batch_Large_Sequential()
        {
            var results = new Mesh[_batchLarge.Count];
            for (int i = 0; i < _batchLarge.Count; i++)
            {
                results[i] = await _asyncMesher.MeshAsync(_batchLarge[i], _fastOptions);
            }
            return results;
        }

        [Benchmark]
        [BenchmarkCategory("Batch", "Large")]
        public async ValueTask<IReadOnlyList<Mesh>> Batch_Large_Parallel()
        {
            return await _asyncMesher.MeshBatchAsync(_batchLarge, _fastOptions, maxDegreeOfParallelism: 4);
        }

        // =================== OPTIMIZED PATH VALIDATION ===================

        [Benchmark]
        [BenchmarkCategory("Optimization", "TrivialPath")]
        public async ValueTask<Mesh> TrivialStructure_OptimizedPath()
        {
            // Should use synchronous path optimization for trivial structures
            return await _asyncMesher.MeshAsync(_trivialStructure, _fastOptions);
        }

        [Benchmark]
        [BenchmarkCategory("Optimization", "MemoryPooling")]
        public async ValueTask<IReadOnlyList<Mesh>> BatchProcessing_WithPooling()
        {
            // Tests object pooling and memory optimization in batch processing
            return await _asyncMesher.MeshBatchAsync(_batchMedium, _fastOptions);
        }

        // =================== PROGRESS REPORTING OVERHEAD ===================

        [Benchmark]
        [BenchmarkCategory("Progress", "WithProgress")]
        public async ValueTask<Mesh> Meshing_WithProgress()
        {
            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));
            
            return await _asyncMesher.MeshWithProgressAsync(_moderateStructure, _fastOptions, progress);
        }

        [Benchmark]
        [BenchmarkCategory("Progress", "WithoutProgress")]
        public async ValueTask<Mesh> Meshing_WithoutProgress()
        {
            return await _asyncMesher.MeshWithProgressAsync(_moderateStructure, _fastOptions, null);
        }
    }
}
