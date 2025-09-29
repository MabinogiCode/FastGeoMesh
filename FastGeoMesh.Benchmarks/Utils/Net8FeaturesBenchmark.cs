using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using FastGeoMesh.Utils;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Benchmarks.Utils;

/// <summary>
/// Benchmarks demonstrating .NET 8 specific performance features and optimizations.
/// Tests new language features, runtime improvements, and API optimizations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class Net8FeaturesBenchmark
{
    private Vec2[] _vectors = null!;
    private double[] _values = null!;
    private const int ItemCount = 10000;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _vectors = new Vec2[ItemCount];
        _values = new double[ItemCount];
        
        for (int i = 0; i < ItemCount; i++)
        {
            _vectors[i] = new Vec2(random.NextDouble() * 100, random.NextDouble() * 100);
            _values[i] = random.NextDouble() * 1000;
        }
    }

    [Benchmark(Baseline = true)]
    public Vec2[] Record_Struct_Operations()
    {
        var results = new Vec2[_vectors.Length];
        
        for (int i = 0; i < _vectors.Length; i++)
        {
            // Test record struct equality and operations
            var v = _vectors[i];
            results[i] = v + Vec2.UnitX * 0.5;
        }
        return results;
    }

    [Benchmark]
    public Vec2[] Traditional_Struct_Operations()
    {
        var results = new Vec2[_vectors.Length];
        
        for (int i = 0; i < _vectors.Length; i++)
        {
            // Simulate traditional struct operations without record optimizations
            var v = _vectors[i];
            results[i] = TraditionalAdd(v, ScaleTraditional(Vec2.UnitX, 0.5));
        }
        return results;
    }

    [Benchmark]
    public double[] Generic_Math_Operations()
    {
        var results = new double[_values.Length];
        
        for (int i = 0; i < _values.Length; i++)
        {
            // Test .NET 8 generic math improvements
            results[i] = GenericMathHelper<double>.Sqrt(_values[i]);
        }
        return results;
    }

    [Benchmark]
    public double[] Traditional_Math_Operations()
    {
        var results = new double[_values.Length];
        
        for (int i = 0; i < _values.Length; i++)
        {
            results[i] = Math.Sqrt(_values[i]);
        }
        return results;
    }

    [Benchmark]
    public FrozenSet<string> FrozenSet_Creation_Small()
    {
        var items = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            items.Add($"Item_{i}");
        }
        return items.ToFrozenSet();
    }

    [Benchmark]
    public HashSet<string> HashSet_Creation_Small()
    {
        var items = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            items.Add($"Item_{i}");
        }
        return items;
    }

    [Benchmark]
    public bool[] Span_Operations_Optimized()
    {
        var results = new bool[_vectors.Length];
        ReadOnlySpan<Vec2> vectorSpan = _vectors.AsSpan();
        
        for (int i = 0; i < vectorSpan.Length; i++)
        {
            ref readonly var v = ref vectorSpan[i];
            results[i] = v.LengthSquared() > 50.0;
        }
        return results;
    }

    [Benchmark]
    public bool[] Array_Operations_Traditional()
    {
        var results = new bool[_vectors.Length];
        
        for (int i = 0; i < _vectors.Length; i++)
        {
            var v = _vectors[i];
            results[i] = (v.X * v.X + v.Y * v.Y) > 50.0; // No LengthSquared optimization
        }
        return results;
    }

    [Benchmark]
    public double StringLength_FrozenDictionary()
    {
        var lookup = OptimizedConstants.QualityThresholds;
        double sum = 0;
        
        for (int i = 0; i < 1000; i++)
        {
            if (lookup.TryGetValue("MinCapQuad", out var value))
                sum += value;
            if (lookup.TryGetValue("PreferredCapQuad", out value))
                sum += value;
            if (lookup.TryGetValue("ExcellentCapQuad", out value))
                sum += value;
        }
        return sum;
    }

    [Benchmark]
    public double StringLength_RegularDictionary()
    {
        var lookup = new Dictionary<string, double>
        {
            ["MinCapQuad"] = 0.3,
            ["PreferredCapQuad"] = 0.7,
            ["ExcellentCapQuad"] = 0.9
        };
        
        double sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (lookup.TryGetValue("MinCapQuad", out var value))
                sum += value;
            if (lookup.TryGetValue("PreferredCapQuad", out value))
                sum += value;
            if (lookup.TryGetValue("ExcellentCapQuad", out value))
                sum += value;
        }
        return sum;
    }

    [Benchmark]
    public int Pattern_Matching_Switch_Expression()
    {
        int count = 0;
        
        for (int i = 0; i < _values.Length; i++)
        {
            var category = _values[i] switch
            {
                < 100 => "Small",
                >= 100 and < 500 => "Medium",
                >= 500 and < 900 => "Large",
                _ => "XLarge"
            };
            count += category.Length;
        }
        return count;
    }

    [Benchmark]
    public int Pattern_Matching_Traditional_IfElse()
    {
        int count = 0;
        
        for (int i = 0; i < _values.Length; i++)
        {
            string category;
            var value = _values[i];
            
            if (value < 100)
                category = "Small";
            else if (value < 500)
                category = "Medium";
            else if (value < 900)
                category = "Large";
            else
                category = "XLarge";
                
            count += category.Length;
        }
        return count;
    }

    // Helper methods for comparison
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vec2 TraditionalAdd(Vec2 a, Vec2 b)
    {
        return new Vec2(a.X + b.X, a.Y + b.Y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vec2 ScaleTraditional(Vec2 v, double scale)
    {
        return new Vec2(v.X * scale, v.Y * scale);
    }

    /// <summary>Generic math helper demonstrating .NET 8 static abstract members.</summary>
    private static class GenericMathHelper<T> where T : struct, IFloatingPoint<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sqrt(T value) => T.Sqrt(value);
    }
}
