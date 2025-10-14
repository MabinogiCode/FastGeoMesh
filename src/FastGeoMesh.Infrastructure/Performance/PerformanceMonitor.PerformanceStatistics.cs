namespace FastGeoMesh.Infrastructure
{
    /// <summary>Performance statistics snapshot.</summary>
    public readonly struct PerformanceStatistics
    {
        /// <summary>Total number of meshing operations performed.</summary>
        public long MeshingOperations { get; init; }

        /// <summary>Total number of quads generated.</summary>
        public long QuadsGenerated { get; init; }

        /// <summary>Total number of triangles generated.</summary>
        public long TrianglesGenerated { get; init; }

        /// <summary>Object pool hit rate as a percentage (0.0 to 1.0).</summary>
        public double PoolHitRate { get; init; }

        /// <summary>Returns a string representation of the performance statistics.</summary>
        public override string ToString()
        {
            return $"Operations: {MeshingOperations}, Quads: {QuadsGenerated}, Triangles: {TrianglesGenerated}, Pool Hit Rate: {PoolHitRate:P2}";
        }
    }
}
