using FastGeoMesh.Domain;
using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Infrastructure.Performance
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
}
