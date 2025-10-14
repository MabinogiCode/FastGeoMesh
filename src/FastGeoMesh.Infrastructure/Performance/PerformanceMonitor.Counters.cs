namespace FastGeoMesh.Infrastructure
{
    /// <summary>
    /// Counters for PerformanceMonitor. Kept in separate file to respect one type per file guideline.
    /// </summary>
    public static class PerformanceMonitorCounters
    {
        private static long _meshingOperations;
        private static long _quadsGenerated;
        private static long _trianglesGenerated;
        private static long _poolHits;
        private static long _poolMisses;

        /// <summary>Gets the total number of meshing operations performed.</summary>
        public static long MeshingOperations => _meshingOperations;

        /// <summary>Gets the total number of quads generated across all operations.</summary>
        public static long QuadsGenerated => _quadsGenerated;

        /// <summary>Gets the total number of triangles generated across all operations.</summary>
        public static long TrianglesGenerated => _trianglesGenerated;

        /// <summary>Gets the total number of object pool cache hits.</summary>
        public static long PoolHits => _poolHits;

        /// <summary>Gets the total number of object pool cache misses.</summary>
        public static long PoolMisses => _poolMisses;

        /// <summary>Increments the count of meshing operations.</summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void IncrementMeshingOperations() => System.Threading.Interlocked.Increment(ref _meshingOperations);

        /// <summary>Adds to the count of generated quads.</summary>
        /// <param name="count">Number of quads to add to the total.</param>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AddQuadsGenerated(int count) => System.Threading.Interlocked.Add(ref _quadsGenerated, count);

        /// <summary>Adds to the count of generated triangles.</summary>
        /// <param name="count">Number of triangles to add to the total.</param>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AddTrianglesGenerated(int count) => System.Threading.Interlocked.Add(ref _trianglesGenerated, count);

        /// <summary>Increments the object pool hit counter.</summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void IncrementPoolHit() => System.Threading.Interlocked.Increment(ref _poolHits);

        /// <summary>Increments the object pool miss counter.</summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void IncrementPoolMiss() => System.Threading.Interlocked.Increment(ref _poolMisses);

        /// <summary>Get current performance statistics.</summary>
        public static PerformanceStatistics GetStatistics()
        {
            return new PerformanceStatistics
            {
                MeshingOperations = _meshingOperations,
                QuadsGenerated = _quadsGenerated,
                TrianglesGenerated = _trianglesGenerated,
                PoolHitRate = _poolHits + _poolMisses > 0 ? (double)_poolHits / (_poolHits + _poolMisses) : 0.0
            };
        }
    }
}
