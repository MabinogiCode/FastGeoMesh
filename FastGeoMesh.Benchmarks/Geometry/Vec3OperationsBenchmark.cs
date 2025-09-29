using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Benchmarks.Geometry;

/// <summary>
/// Benchmarks for Vec3 operations comparing optimized vs non-optimized implementations.
/// Tests the impact of AggressiveInlining and new Vec3 methods including batch operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class Vec3OperationsBenchmark
{
    private Vec3[] _vectors = null!;
    private Vec3[] _vectorsB = null!;
    private Vec3[] _results = null!;
    private const int VectorCount = 10000;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _vectors = new Vec3[VectorCount];
        _vectorsB = new Vec3[VectorCount];
        _results = new Vec3[VectorCount];
        
        for (int i = 0; i < VectorCount; i++)
        {
            _vectors[i] = new Vec3(
                random.NextDouble() * 100 - 50,
                random.NextDouble() * 100 - 50,
                random.NextDouble() * 100 - 50
            );
            _vectorsB[i] = new Vec3(
                random.NextDouble() * 100 - 50,
                random.NextDouble() * 100 - 50,
                random.NextDouble() * 100 - 50
            );
        }
    }

    [Benchmark(Baseline = true)]
    public double Vec3Addition_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            var result = _vectors[i] + _vectors[i + 1];
            sum += result.X + result.Y + result.Z;
        }
        return sum;
    }

    [Benchmark]
    public double Vec3Addition_NonOptimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            var result = Vec3AdditionSlow(_vectors[i], _vectors[i + 1]);
            sum += result.X + result.Y + result.Z;
        }
        return sum;
    }

    [Benchmark]
    public double Vec3DotProduct_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            sum += _vectors[i].Dot(_vectors[i + 1]);
        }
        return sum;
    }

    // NEW: Batch operations benchmarks
    [Benchmark]
    public double Vec3AccumulateDot_Batch()
    {
        return Vec3.AccumulateDot(_vectors, _vectorsB);
    }

    [Benchmark]
    public double Vec3AccumulateDot_Loop()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            sum += _vectors[i].Dot(_vectorsB[i]);
        }
        return sum;
    }

    [Benchmark]
    public void Vec3Add_Batch()
    {
        Vec3.Add(_vectors, _vectorsB, _results);
    }

    [Benchmark]
    public void Vec3Add_Loop()
    {
        for (int i = 0; i < _vectors.Length; i++)
        {
            _results[i] = _vectors[i] + _vectorsB[i];
        }
    }

    [Benchmark]
    public void Vec3Cross_Batch()
    {
        Vec3.Cross(_vectors, _vectorsB, _results);
    }

    [Benchmark]
    public void Vec3Cross_Loop()
    {
        for (int i = 0; i < _vectors.Length; i++)
        {
            _results[i] = _vectors[i].Cross(_vectorsB[i]);
        }
    }

    [Benchmark]
    public Vec3[] Vec3CrossProduct_Optimized()
    {
        var results = new Vec3[_vectors.Length - 1];
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            results[i] = _vectors[i].Cross(_vectors[i + 1]);
        }
        return results;
    }

    [Benchmark]
    public Vec3[] Vec3CrossProduct_NonOptimized()
    {
        var results = new Vec3[_vectors.Length - 1];
        for (int i = 0; i < _vectors.Length - 1; i++)
        {
            results[i] = CrossProductSlow(_vectors[i], _vectors[i + 1]);
        }
        return results;
    }

    [Benchmark]
    public double Vec3Length_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            sum += _vectors[i].Length();
        }
        return sum;
    }

    [Benchmark]
    public double Vec3LengthSquared_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            sum += _vectors[i].LengthSquared();
        }
        return sum;
    }

    [Benchmark]
    public Vec3[] Vec3Normalize_Optimized()
    {
        var results = new Vec3[_vectors.Length];
        for (int i = 0; i < _vectors.Length; i++)
        {
            results[i] = _vectors[i].Normalize();
        }
        return results;
    }

    [Benchmark]
    public double Vec3Scaling_Optimized()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            var scaled = _vectors[i] * 2.5;
            sum += scaled.X + scaled.Y + scaled.Z;
        }
        return sum;
    }

    [Benchmark]
    public double Vec3Scaling_Commutative()
    {
        double sum = 0;
        for (int i = 0; i < _vectors.Length; i++)
        {
            var scaled = 2.5 * _vectors[i]; // Test commutative multiplication
            sum += scaled.X + scaled.Y + scaled.Z;
        }
        return sum;
    }

    // Non-optimized reference implementations for comparison
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vec3 Vec3AdditionSlow(Vec3 a, Vec3 b)
    {
        return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vec3 CrossProductSlow(Vec3 a, Vec3 b)
    {
        return new Vec3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }
}
