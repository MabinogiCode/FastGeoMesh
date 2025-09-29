using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using FastGeoMesh.Utils;
using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Benchmarks.Utils;

/// <summary>
/// Benchmarks for object pooling comparing pooled vs non-pooled object creation.
/// Tests the performance benefit of Microsoft.Extensions.ObjectPool.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class ObjectPoolingBenchmark
{
    private const int IterationCount = 10000;

    [Benchmark(Baseline = true)]
    public int ObjectPooling_IntList_Pooled()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount; i++)
        {
            var list = MeshingPools.IntListPool.Get();
            try
            {
                // Simulate typical usage
                list.Add(i);
                list.Add(i * 2);
                list.Add(i * 3);
                totalCount += list.Count;
            }
            finally
            {
                MeshingPools.IntListPool.Return(list);
            }
        }
        return totalCount;
    }

    [Benchmark]
    public int ObjectPooling_IntList_NonPooled()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount; i++)
        {
            var list = new List<int>();
            list.Add(i);
            list.Add(i * 2);
            list.Add(i * 3);
            totalCount += list.Count;
            // No explicit disposal - let GC handle it
        }
        return totalCount;
    }

    [Benchmark]
    public int ObjectPooling_DoubleList_Pooled()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount; i++)
        {
            var list = MeshingPools.DoubleListPool.Get();
            try
            {
                list.Add(i * 0.1);
                list.Add(i * 0.2);
                list.Add(i * 0.3);
                list.Add(i * 0.4);
                totalCount += list.Count;
            }
            finally
            {
                MeshingPools.DoubleListPool.Return(list);
            }
        }
        return totalCount;
    }

    [Benchmark]
    public int ObjectPooling_DoubleList_NonPooled()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount; i++)
        {
            var list = new List<double>();
            list.Add(i * 0.1);
            list.Add(i * 0.2);
            list.Add(i * 0.3);
            list.Add(i * 0.4);
            totalCount += list.Count;
        }
        return totalCount;
    }

    [Benchmark]
    public int ObjectPooling_Vec2List_Pooled()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount; i++)
        {
            var list = MeshingPools.Vec2ListPool.Get();
            try
            {
                list.Add(new Vec2(i, i * 2));
                list.Add(new Vec2(i * 3, i * 4));
                totalCount += list.Count;
            }
            finally
            {
                MeshingPools.Vec2ListPool.Return(list);
            }
        }
        return totalCount;
    }

    [Benchmark]
    public int ObjectPooling_Vec2List_NonPooled()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount; i++)
        {
            var list = new List<Vec2>();
            list.Add(new Vec2(i, i * 2));
            list.Add(new Vec2(i * 3, i * 4));
            totalCount += list.Count;
        }
        return totalCount;
    }

    [Benchmark]
    public int MixedPoolUsage_Realistic()
    {
        int totalOperations = 0;
        
        for (int i = 0; i < IterationCount / 10; i++) // Fewer iterations for complex operations
        {
            // Simulate realistic meshing scenario
            var intList = MeshingPools.IntListPool.Get();
            var doubleList = MeshingPools.DoubleListPool.Get();
            var vec2List = MeshingPools.Vec2ListPool.Get();
            
            try
            {
                // Simulate building indices
                for (int j = 0; j < 20; j++)
                {
                    intList.Add(j);
                }
                
                // Simulate storing distances
                for (int j = 0; j < 15; j++)
                {
                    doubleList.Add(j * 0.5);
                }
                
                // Simulate storing vertices
                for (int j = 0; j < 10; j++)
                {
                    vec2List.Add(new Vec2(j, j * 1.5));
                }
                
                totalOperations += intList.Count + doubleList.Count + vec2List.Count;
            }
            finally
            {
                MeshingPools.IntListPool.Return(intList);
                MeshingPools.DoubleListPool.Return(doubleList);
                MeshingPools.Vec2ListPool.Return(vec2List);
            }
        }
        return totalOperations;
    }

    [Benchmark]
    public int MixedPoolUsage_NonPooled()
    {
        int totalOperations = 0;
        
        for (int i = 0; i < IterationCount / 10; i++) // Fewer iterations for complex operations
        {
            var intList = new List<int>();
            var doubleList = new List<double>();
            var vec2List = new List<Vec2>();
            
            // Simulate building indices
            for (int j = 0; j < 20; j++)
            {
                intList.Add(j);
            }
            
            // Simulate storing distances
            for (int j = 0; j < 15; j++)
            {
                doubleList.Add(j * 0.5);
            }
            
            // Simulate storing vertices
            for (int j = 0; j < 10; j++)
            {
                vec2List.Add(new Vec2(j, j * 1.5));
            }
            
            totalOperations += intList.Count + doubleList.Count + vec2List.Count;
        }
        return totalOperations;
    }

    [Benchmark]
    public bool PoolStatistics_Performance()
    {
        // Test the performance of accessing pool statistics
        bool result = true;
        
        for (int i = 0; i < 1000; i++)
        {
            // This tests if statistics tracking has significant overhead
            var list = MeshingPools.IntListPool.Get();
            list.Add(i);
            MeshingPools.IntListPool.Return(list);
            
            // Access some pool if available (implementation dependent)
            result &= (list != null);
        }
        return result;
    }

    [Benchmark]
    public int NestedPoolUsage()
    {
        int totalCount = 0;
        
        for (int i = 0; i < IterationCount / 100; i++)
        {
            var outerList = MeshingPools.IntListPool.Get();
            try
            {
                for (int j = 0; j < 10; j++)
                {
                    var innerList = MeshingPools.DoubleListPool.Get();
                    try
                    {
                        innerList.Add(j * 0.1);
                        innerList.Add(j * 0.2);
                        outerList.Add(innerList.Count);
                    }
                    finally
                    {
                        MeshingPools.DoubleListPool.Return(innerList);
                    }
                }
                totalCount += outerList.Count;
            }
            finally
            {
                MeshingPools.IntListPool.Return(outerList);
            }
        }
        return totalCount;
    }
}
