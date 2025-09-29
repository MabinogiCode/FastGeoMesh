using BenchmarkDotNet.Attributes;
using System.Collections.Frozen;

namespace FastGeoMesh.Benchmarks.Utils;

/// <summary>
/// Benchmarks comparing .NET 8 FrozenCollections with regular collections.
/// Tests various scenarios to demonstrate performance improvements.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class FrozenCollectionsBenchmark
{
    private readonly Dictionary<string, int> _regularDictionary;
    private readonly FrozenDictionary<string, int> _frozenDictionary;
    private readonly HashSet<string> _regularSet;
    private readonly FrozenSet<string> _frozenSet;
    private readonly string[] _lookupKeys;
    private const int LookupCount = 100000;

    public FrozenCollectionsBenchmark()
    {
        // Create test data
        var dictionaryData = new Dictionary<string, int>();
        var setData = new HashSet<string>();
        
        for (int i = 0; i < 100; i++)
        {
            string key = $"Key_{i:D3}";
            dictionaryData[key] = i;
            setData.Add(key);
        }

        _regularDictionary = dictionaryData;
        _frozenDictionary = dictionaryData.ToFrozenDictionary();
        _regularSet = setData;
        _frozenSet = setData.ToFrozenSet();

        // Keys for lookup testing (mix of existing and non-existing)
        _lookupKeys = new string[20];
        for (int i = 0; i < 15; i++)
        {
            _lookupKeys[i] = $"Key_{i * 7:D3}"; // Existing keys
        }
        for (int i = 15; i < 20; i++)
        {
            _lookupKeys[i] = $"NonExistent_{i}"; // Non-existing keys
        }
    }

    [Benchmark(Baseline = true)]
    public int Dictionary_FrozenDictionary_Lookup()
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
    public int Dictionary_RegularDictionary_Lookup()
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
    public int Set_FrozenSet_Contains()
    {
        int successCount = 0;
        
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            if (_frozenSet.Contains(key))
            {
                successCount++;
            }
        }
        return successCount;
    }

    [Benchmark]
    public int Set_RegularSet_Contains()
    {
        int successCount = 0;
        
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            if (_regularSet.Contains(key))
            {
                successCount++;
            }
        }
        return successCount;
    }

    [Benchmark]
    public int[] Dictionary_FrozenDictionary_GetValues()
    {
        var results = new int[LookupCount];
        
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            _frozenDictionary.TryGetValue(key, out results[i]);
        }
        return results;
    }

    [Benchmark]
    public int[] Dictionary_RegularDictionary_GetValues()
    {
        var results = new int[LookupCount];
        
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            _regularDictionary.TryGetValue(key, out results[i]);
        }
        return results;
    }

    [Benchmark]
    public int Dictionary_FrozenDictionary_Enumerate()
    {
        int sum = 0;
        for (int iteration = 0; iteration < 1000; iteration++)
        {
            foreach (var kvp in _frozenDictionary)
            {
                sum += kvp.Value;
            }
        }
        return sum;
    }

    [Benchmark]
    public int Dictionary_RegularDictionary_Enumerate()
    {
        int sum = 0;
        for (int iteration = 0; iteration < 1000; iteration++)
        {
            foreach (var kvp in _regularDictionary)
            {
                sum += kvp.Value;
            }
        }
        return sum;
    }

    [Benchmark]
    public int Set_FrozenSet_Enumerate()
    {
        int count = 0;
        for (int iteration = 0; iteration < 1000; iteration++)
        {
            foreach (var item in _frozenSet)
            {
                count += item.Length;
            }
        }
        return count;
    }

    [Benchmark]
    public int Set_RegularSet_Enumerate()
    {
        int count = 0;
        for (int iteration = 0; iteration < 1000; iteration++)
        {
            foreach (var item in _regularSet)
            {
                count += item.Length;
            }
        }
        return count;
    }

    [Benchmark]
    public FrozenDictionary<string, int> CreateFrozenDictionary_Small()
    {
        var data = new Dictionary<string, int>();
        for (int i = 0; i < 10; i++)
        {
            data[$"Key_{i}"] = i;
        }
        return data.ToFrozenDictionary();
    }

    [Benchmark]
    public FrozenDictionary<string, int> CreateFrozenDictionary_Medium()
    {
        var data = new Dictionary<string, int>();
        for (int i = 0; i < 100; i++)
        {
            data[$"Key_{i}"] = i;
        }
        return data.ToFrozenDictionary();
    }

    [Benchmark]
    public FrozenDictionary<string, int> CreateFrozenDictionary_Large()
    {
        var data = new Dictionary<string, int>();
        for (int i = 0; i < 1000; i++)
        {
            data[$"Key_{i}"] = i;
        }
        return data.ToFrozenDictionary();
    }

    [Benchmark]
    public FrozenSet<string> CreateFrozenSet_Small()
    {
        var data = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            data.Add($"Key_{i}");
        }
        return data.ToFrozenSet();
    }

    [Benchmark]
    public FrozenSet<string> CreateFrozenSet_Medium()
    {
        var data = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            data.Add($"Key_{i}");
        }
        return data.ToFrozenSet();
    }

    [Benchmark]
    public FrozenSet<string> CreateFrozenSet_Large()
    {
        var data = new HashSet<string>();
        for (int i = 0; i < 1000; i++)
        {
            data.Add($"Key_{i}");
        }
        return data.ToFrozenSet();
    }

    [Benchmark]
    public bool Dictionary_ContainsKey_Frozen()
    {
        bool result = true;
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            result &= _frozenDictionary.ContainsKey(key);
        }
        return result;
    }

    [Benchmark]
    public bool Dictionary_ContainsKey_Regular()
    {
        bool result = true;
        for (int i = 0; i < LookupCount; i++)
        {
            string key = _lookupKeys[i % _lookupKeys.Length];
            result &= _regularDictionary.ContainsKey(key);
        }
        return result;
    }
}
