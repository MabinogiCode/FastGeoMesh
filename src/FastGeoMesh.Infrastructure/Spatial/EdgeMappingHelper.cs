using FastGeoMesh.Infrastructure.Performance;

namespace FastGeoMesh.Infrastructure {
    /// <summary>Helper class for edge mapping operations in tessellation.</summary>
    public static class EdgeMappingHelper {
        /// <summary>Add edge to triangle mapping, using object pool for list allocation.</summary>
        public static void AddEdgeToTriangleMapping(Dictionary<(int, int), List<int>> edgeToTris, int i, int j, int triangle) {
            ArgumentNullException.ThrowIfNull(edgeToTris);
            var key = i < j ? (i, j) : (j, i);
            if (!edgeToTris.TryGetValue(key, out var list)) {
                list = MeshingPools.IntListPool.Get();
                edgeToTris[key] = list;
            }
            list.Add(triangle);
        }
    }
}
