using System.Collections.Generic;
using FastGeoMesh.Geometry;
using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Utils
{
    /// <summary>Specialized pools for commonly used collection types in meshing.</summary>
    public static class MeshingPools
    {
        private static readonly DefaultObjectPoolProvider _provider = new();

        /// <summary>Pool for small integer lists (edge adjacencies).</summary>
        public static readonly ObjectPool<List<int>> IntListPool = 
            _provider.Create(new ListPoolPolicy<int>());

        /// <summary>Pool for triangle lists used in tessellation.</summary>
        public static readonly ObjectPool<List<(int a, int b, int c)>> TriangleListPool = 
            _provider.Create(new ListPoolPolicy<(int a, int b, int c)>());
            
        /// <summary>Pool for edge-to-triangle mappings.</summary>
        public static readonly ObjectPool<Dictionary<(int, int), List<int>>> EdgeMapPool = 
            _provider.Create(new DictionaryPoolPolicy<(int, int), List<int>>());
            
        /// <summary>Pool for quad candidate lists.</summary>
        public static readonly ObjectPool<List<(double score, int t0, int t1, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)>> CandidateListPool = 
            _provider.Create(new ListPoolPolicy<(double score, int t0, int t1, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)>());
    }

    /// <summary>Pooled object policy for List&lt;T&gt; that clears contents on return.</summary>
    internal sealed class ListPoolPolicy<T> : PooledObjectPolicy<List<T>>
    {
        public override List<T> Create() => new List<T>();

        public override bool Return(List<T> obj)
        {
            if (obj == null)
            {
                return false;
            }
                
            // Clear contents to prevent interference between uses
            obj.Clear();
            
            // Don't return extremely large lists to avoid memory bloat
            const int maxCapacity = 1024;
            return obj.Capacity <= maxCapacity;
        }
    }

    /// <summary>Pooled object policy for Dictionary&lt;K,V&gt; that clears contents on return.</summary>
    internal sealed class DictionaryPoolPolicy<K, V> : PooledObjectPolicy<Dictionary<K, V>>
        where K : notnull
    {
        public override Dictionary<K, V> Create() => new Dictionary<K, V>();

        public override bool Return(Dictionary<K, V> obj)
        {
            if (obj == null)
            {
                return false;
            }
                
            // Clear contents to prevent interference between uses
            obj.Clear();
            
            // Always return to pool since Dictionary doesn't expose capacity easily
            return true;
        }
    }
}