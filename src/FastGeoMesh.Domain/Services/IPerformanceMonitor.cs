namespace FastGeoMesh.Domain.Services
{
    /// <summary>
    /// Interface for performance monitoring services.
    /// Allows application layer to track performance without depending on infrastructure.
    /// </summary>
    public interface IPerformanceMonitor
    {
        /// <summary>Gets current performance statistics.</summary>
        PerformanceStatistics GetLiveStatistics();

        /// <summary>
        /// Starts a new meshing activity for performance tracking.
        /// </summary>
        /// <param name="activityName">Name of the activity being tracked.</param>
        /// <param name="metadata">Optional metadata for the activity.</param>
        /// <returns>Disposable activity tracker.</returns>
        IDisposable StartMeshingActivity(string activityName, object? metadata = null);

        /// <summary>
        /// Increments the meshing operations counter.
        /// </summary>
        void IncrementMeshingOperations();

        /// <summary>
        /// Adds to the generated quads counter.
        /// </summary>
        /// <param name="count">Number of quads generated.</param>
        void AddQuadsGenerated(int count);

        /// <summary>
        /// Adds to the generated triangles counter.
        /// </summary>
        /// <param name="count">Number of triangles generated.</param>
        void AddTrianglesGenerated(int count);
    }

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
