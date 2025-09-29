using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Benchmarks.Geometry;

/// <summary>
/// Benchmarks for Vec2 operations comparing optimized vs non-optimized implementations.
/// Tests the impact of AggressiveInlining and .NET 8 optimizations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class Vec2OperationsBenchmark
{
    private Vec2[] _vectors = null!;
    private const int VectorCount = 10000;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _vectors = new Vec2[VectorCount];
        
        for (int i = 0; i < VectorCount; i++)
        {
            _vectors[i] = new Vec2(
                random.NextDouble() * 100 - 50,
                random.NextDouble() * 100 - 50
            );
        }
    }

    [Benchmark(Baseline = true)]
    public double Vec2Addition_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            var result = _vectors[i] + _vectors[i + 1];
            sum += result.X + result.Y;
        }
        return sum;
    }

    [Benchmark]
    public double Vec2Addition_NonOptimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            var result = Vec2AdditionSlow(_vectors[i], _vectors[i + 1]);
            sum += result.X + result.Y;
        }
        return sum;
    }

    [Benchmark]
    public double Vec2DotProduct_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            sum += _vectors[i].Dot(_vectors[i + 1]);
        }
        return sum;
    }

    [Benchmark]
    public double Vec2DotProduct_NonOptimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            sum += DotProductSlow(_vectors[i], _vectors[i + 1]);
        }
        return sum;
    }

    [Benchmark]
    public double Vec2Length_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            sum += _vectors[i].Length();
        }
        return sum;
    }

    [Benchmark]
    public double Vec2LengthSquared_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            sum += _vectors[i].LengthSquared();
        }
        return sum;
    }

    [Benchmark]
    public Vec2[] Vec2Normalize_Optimized()
    {
        var results = new Vec2[_vectors.Length];
        for (int i = 0; i < _vectors.Length; i++)
        {
            results[i] = _vectors[i].Normalize();
        }
        return results;
    }

    [Benchmark]
    public double Vec2Cross_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            sum += _vectors[i].Cross(_vectors[i + 1]);
        }
        return sum;
    }

    // Non-optimized reference implementations for comparison
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vec2 Vec2AdditionSlow(Vec2 a, Vec2 b)
    {
        return new Vec2(a.X + b.X, a.Y + b.Y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double DotProductSlow(Vec2 a, Vec2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }
}
