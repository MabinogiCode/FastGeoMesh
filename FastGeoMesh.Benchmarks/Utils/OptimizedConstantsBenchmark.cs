using BenchmarkDotNet.Attributes;
using FastGeoMesh.Utils;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Benchmarks.Utils;

/// <summary>
/// Benchmarks for OptimizedConstants comparing FrozenCollections vs regular collections.
/// Tests the performance benefit of .NET 8 FrozenDictionary and FrozenSet.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class OptimizedConstantsBenchmark
{
    private readonly Dictionary<string, (double Min, double Max)> _regularDictionary;
    private readonly FrozenDictionary<string, (double Min, double Max)> _frozenDictionary;
    private readonly HashSet<string> _regularSet;
    private readonly FrozenSet<string> _frozenSet;
    private readonly string[] _lookupKeys;
    private readonly string[] _performanceRules;
    private const int LookupCount = 100000;

    public OptimizedConstantsBenchmark()
    {
        // Regular dictionary for comparison
        _regularDictionary = new Dictionary<string, (double, double)>
        {
            ["XY"] = (1e-6, 1e6),
            ["Z"] = (1e-6, 1e6),
            ["HoleRefinement"] = (1e-6, 1e4),
            ["SegmentRefinement"] = (1e-6, 1e4)
        };

        _frozenDictionary = OptimizedConstants.EdgeLengthLimits;

        // Regular set for comparison
        _regularSet = new HashSet<string>
        {
            "CA1859", "CA1860", "CA1861", "CA1825", "CA1826", "CA1827", "CA1828", "CA1829"
        };

        _frozenSet = OptimizedConstants.CriticalPerformanceRules;

        // Lookup keys for testing
        _lookupKeys = new[] { "XY", "Z", "HoleRefinement", "SegmentRefinement", "Invalid" };
        _performanceRules = new[] { "CA1859", "CA1860", "CA1861", "CA1825", "Invalid" };
    }

    [Benchmark(Baseline = true)]
    public int EdgeLengthLookup_FrozenDictionary()
    {
        int successCount = 0;
        
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            if (_frozenDictionary.TryGetValue(key, out _))
            {
                successCount++;
            }
        }
        return successCount;
    }

    [Benchmark]
    public int EdgeLengthLookup_RegularDictionary()
    {
        int successCount = 0;
        
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            if (_regularDictionary.TryGetValue(key, out _))
            {
                successCount++;
            }
        }
        return successCount;
    }

    [Benchmark]
    public int PerformanceRuleLookup_FrozenSet()
    {
        int successCount = 0;
        
        for (int i = 0; i < LookupCount; i++)
        {
            string rule = _performanceRules[i % _performanceRules.Length];
            if (_frozenSet.Contains(rule))
            {
                successCount++;
            }
        }
        return successCount;
    }

    [Benchmark]
    public int PerformanceRuleLookup_RegularSet()
    {
        int successCount = 0;
        
        for (int i = 0; i < LookupCount; i++)
        {
            string rule = _performanceRules[i % _performanceRules.Length];
            if (_regularSet.Contains(rule))
            {
                successCount++;
            }
        }
        return successCount;
    }

    [Benchmark]
    public bool[] EdgeLengthValidation_OptimizedConstants()
    {
        var results = new bool[LookupCount];
        var values = new double[] { 1e-7, 1.0, 100.0, 1e5, 1e7 };
        
        for (int i = 0; i < LookupCount; i++)
        {
            string category = _lookupKeys[i % (_lookupKeys.Length - 1)]; // Skip "Invalid"
            double value = values[i % values.Length];
            results[i] = OptimizedConstants.IsValidEdgeLength(category, value);
        }
        return results;
    }

    [Benchmark]
    public bool[] EdgeLengthValidation_DirectCheck()
    {
        var results = new bool[LookupCount];
        var values = new double[] { 1e-7, 1.0, 100.0, 1e5, 1e7 };
        
        for (int i = 0; i < LookupCount; i++)
        {
            string category = _lookupKeys[i % (_lookupKeys.Length - 1)]; // Skip "Invalid"
            double value = values[i % values.Length];
            results[i] = ValidateEdgeLengthSlow(category, value);
        }
        return results;
    }

    [Benchmark]
    public double[] QualityThresholdLookup_OptimizedConstants()
    {
        var results = new double[LookupCount];
        var categories = new[] { "MinCapQuad", "PreferredCapQuad", "ExcellentCapQuad" };
        
        for (int i = 0; i < LookupCount; i++)
        {
            string category = categories[i % categories.Length];
            OptimizedConstants.QualityThresholds.TryGetValue(category, out results[i]);
        }
        return results;
    }

    [Benchmark]
    public bool[] QualityThresholdValidation_OptimizedConstants()
    {
        var results = new bool[LookupCount];
        var categories = new[] { "MinCapQuad", "PreferredCapQuad", "ExcellentCapQuad" };
        var values = new double[] { 0.1, 0.3, 0.5, 0.7, 0.9 };
        
        for (int i = 0; i < LookupCount; i++)
        {
            string category = categories[i % categories.Length];
            double value = values[i % values.Length];
            results[i] = OptimizedConstants.MeetsQualityThreshold(category, value);
        }
        return results;
    }

    [Benchmark]
    public FrozenDictionary<string, double> CreateFrozenDictionary()
    {
        return new Dictionary<string, double>
        {
            ["Test1"] = 1.0,
            ["Test2"] = 2.0,
            ["Test3"] = 3.0,
            ["Test4"] = 4.0,
            ["Test5"] = 5.0
        }.ToFrozenDictionary();
    }

    [Benchmark]
    public Dictionary<string, double> CreateRegularDictionary()
    {
        return new Dictionary<string, double>
        {
            ["Test1"] = 1.0,
            ["Test2"] = 2.0,
            ["Test3"] = 3.0,
            ["Test4"] = 4.0,
            ["Test5"] = 5.0
        };
    }

    // Non-optimized reference implementation for comparison
    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool ValidateEdgeLengthSlow(string category, double value)
    {
        if (_regularDictionary.TryGetValue(category, out var limits))
        {
            return value >= limits.Min && value <= limits.Max;
        }
        return false;
    }
}
