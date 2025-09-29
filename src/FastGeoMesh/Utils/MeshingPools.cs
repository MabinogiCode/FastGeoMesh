using System.Collections.Generic;
using FastGeoMesh.Geometry;
using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Utils
{
    /// <summary>Specialized pools for commonly used collection types in meshing with performance optimizations.</summary>
    public static class MeshingPools
    {
        private static readonly DefaultObjectPoolProvider _provider = new();

        /// <summary>Pool for small integer lists (edge adjacencies) with size-based retention.</summary>
        public static readonly ObjectPool<List<int>> IntListPool = 
            _provider.Create(new ListPoolPolicy<int>(maxRetainedCapacity: 512));

        /// <summary>Pool for triangle lists used in tessellation with optimized retention.</summary>
        public static readonly ObjectPool<List<(int a, int b, int c)>> TriangleListPool = 
            _provider.Create(new ListPoolPolicy<(int a, int b, int c)>(maxRetainedCapacity: 2048));
            
        /// <summary>Pool for edge-to-triangle mappings with memory-conscious disposal.</summary>
        public static readonly ObjectPool<Dictionary<(int, int), List<int>>> EdgeMapPool = 
            _provider.Create(new DictionaryPoolPolicy<(int, int), List<int>>());
            
        /// <summary>Pool for quad candidate lists with performance-optimized capacity management.</summary>
        public static readonly ObjectPool<List<(double score, int t0, int t1, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)>> CandidateListPool = 
            _provider.Create(new ListPoolPolicy<(double score, int t0, int t1, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)>(maxRetainedCapacity: 1024));

        /// <summary>Pool for coordinate arrays used in grid generation.</summary>
        public static readonly ObjectPool<List<double>> DoubleListPool = 
            _provider.Create(new ListPoolPolicy<double>(maxRetainedCapacity: 4096));

        /// <summary>Pool for vertex collections in tessellation operations.</summary>
        public static readonly ObjectPool<List<Vec2>> Vec2ListPool = 
            _provider.Create(new ListPoolPolicy<Vec2>(maxRetainedCapacity: 2048));
    }

    /// <summary>Enhanced pooled object policy for List&lt;T&gt; with configurable capacity limits.</summary>
    internal sealed class ListPoolPolicy<T> : PooledObjectPolicy<List<T>>
    {
        private readonly int _maxRetainedCapacity;

        public ListPoolPolicy(int maxRetainedCapacity = 1024)
        {
            _maxRetainedCapacity = maxRetainedCapacity;
        }

        public override List<T> Create() => new List<T>();

        public override bool Return(List<T> obj)
        {
            if (obj == null)
            {
                return false;
            }
                
            // Clear contents to prevent interference between uses
            obj.Clear();
            
            // Use configurable capacity limit for better memory management
            return obj.Capacity <= _maxRetainedCapacity;
        }
    }

    /// <summary>Enhanced pooled object policy for Dictionary&lt;K,V&gt; with memory optimization.</summary>
    internal sealed class DictionaryPoolPolicy<K, V> : PooledObjectPolicy<Dictionary<K, V>>
        where K : notnull
    {
        private const int MaxRetainedCount = 1024;

        public override Dictionary<K, V> Create() => new Dictionary<K, V>();

        public override bool Return(Dictionary<K, V> obj)
        {
            if (obj == null)
            {
                return false;
            }
                
            // Clear contents to prevent interference between uses
            obj.Clear();
            
            // Only retain dictionaries under a reasonable size threshold
            return obj.Count <= MaxRetainedCount;
        }
    }
}