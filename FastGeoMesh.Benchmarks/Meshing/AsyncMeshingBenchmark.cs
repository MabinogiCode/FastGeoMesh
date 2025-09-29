using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Benchmarks.Meshing;

/// <summary>
/// Benchmarks for async meshing operations comparing Task vs ValueTask performance.
/// Tests the impact of .NET 8 async optimizations and ValueTask usage.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class AsyncMeshingBenchmark
{
    private PrismStructureDefinition _structure = null!;
    private MesherOptions _options = null!;
    private TestAsyncMesher _asyncMesher = null!;
    private TestTaskMesher _taskMesher = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create a moderately complex structure for benchmarking
        var polygon = Polygon2D.FromPoints(new[]
        {
            new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 10), new Vec2(0, 10)
        });
        _structure = new PrismStructureDefinition(polygon, 0, 5);

        _options = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(2.0)
            .WithTargetEdgeLengthZ(1.5)
            .WithCaps(bottom: true, top: true)
            .Build();

        _asyncMesher = new TestAsyncMesher();
        _taskMesher = new TestTaskMesher();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> AsyncMeshing_ValueTask()
    {
        var mesh = await _asyncMesher.MeshAsync(_structure, _options, CancellationToken.None);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int> AsyncMeshing_Task()
    {
        var mesh = await _taskMesher.MeshAsync(_structure, _options, CancellationToken.None);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int[]> BatchAsyncMeshing_ValueTask()
    {
        const int batchSize = 10;
        var tasks = new ValueTask<Mesh>[batchSize];
        
        for (int i = 0; i < batchSize; i++)
        {
            tasks[i] = _asyncMesher.MeshAsync(_structure, _options, CancellationToken.None);
        }

        var results = new int[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            var mesh = await tasks[i];
            results[i] = mesh.Quads.Count + mesh.Triangles.Count;
        }
        return results;
    }

    [Benchmark]
    public async Task<int[]> BatchAsyncMeshing_Task()
    {
        const int batchSize = 10;
        var tasks = new Task<Mesh>[batchSize];
        
        for (int i = 0; i < batchSize; i++)
        {
            tasks[i] = _taskMesher.MeshAsync(_structure, _options, CancellationToken.None);
        }

        var meshes = await Task.WhenAll(tasks);
        var results = new int[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            results[i] = meshes[i].Quads.Count + meshes[i].Triangles.Count;
        }
        return results;
    }

    [Benchmark]
    public async Task<int> AsyncMeshingWithCancellation_ValueTask()
    {
        using var cts = new CancellationTokenSource();
        var mesh = await _asyncMesher.MeshAsync(_structure, _options, cts.Token);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int> AsyncMeshingWithCancellation_Task()
    {
        using var cts = new CancellationTokenSource();
        var mesh = await _taskMesher.MeshAsync(_structure, _options, cts.Token);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int> ConfigureAwaitFalse_ValueTask()
    {
        var mesh = await _asyncMesher.MeshAsync(_structure, _options, CancellationToken.None).ConfigureAwait(false);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int> ConfigureAwaitFalse_Task()
    {
        var mesh = await _taskMesher.MeshAsync(_structure, _options, CancellationToken.None).ConfigureAwait(false);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int> FastCompletionPath_ValueTask()
    {
        // This tests the fast completion path where ValueTask completes synchronously
        var fastOptions = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(5.0) // Larger edge length = faster meshing
            .WithTargetEdgeLengthZ(5.0)
            .WithCaps(bottom: false, top: false) // No caps = faster
            .Build();

        var mesh = await _asyncMesher.MeshAsync(_structure, fastOptions, CancellationToken.None);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public async Task<int[]> SequentialAsync_ValueTask()
    {
        const int iterations = 5;
        var results = new int[iterations];
        
        for (int i = 0; i < iterations; i++)
        {
            var mesh = await _asyncMesher.MeshAsync(_structure, _options, CancellationToken.None);
            results[i] = mesh.Quads.Count + mesh.Triangles.Count;
        }
        return results;
    }

    [Benchmark]
    public async Task<int[]> SequentialAsync_Task()
    {
        const int iterations = 5;
        var results = new int[iterations];
        
        for (int i = 0; i < iterations; i++)
        {
            var mesh = await _taskMesher.MeshAsync(_structure, _options, CancellationToken.None);
            results[i] = mesh.Quads.Count + mesh.Triangles.Count;
        }
        return results;
    }
}
