namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Provides estimated computational complexity and resource requirements for meshing operations.
    /// </summary>
    public readonly struct MeshingComplexityEstimate
    {
        /// <summary>
        /// Estimated number of quads that will be generated.
        /// </summary>
        public int EstimatedQuadCount { get; }

        /// <summary>
        /// Estimated number of triangles that will be generated.
        /// </summary>
        public int EstimatedTriangleCount { get; }

        /// <summary>
        /// Estimated peak memory usage in bytes.
        /// </summary>
        public long EstimatedPeakMemoryBytes { get; }

        /// <summary>
        /// Estimated computation time based on current hardware.
        /// </summary>
        public TimeSpan EstimatedComputationTime { get; }

        /// <summary>
        /// Recommended degree of parallelism for optimal performance.
        /// </summary>
        public int RecommendedParallelism { get; }

        /// <summary>
        /// Complexity classification for the operation.
        /// </summary>
        public MeshingComplexity Complexity { get; }

        /// <summary>
        /// Additional performance hints and recommendations.
        /// </summary>
        public IReadOnlyList<string> PerformanceHints { get; }

        /// <summary>
        /// Creates a new complexity estimate.
        /// </summary>
        public MeshingComplexityEstimate(
            int estimatedQuadCount,
            int estimatedTriangleCount,
            long estimatedPeakMemoryBytes,
            TimeSpan estimatedComputationTime,
            int recommendedParallelism,
            MeshingComplexity complexity,
            IReadOnlyList<string>? performanceHints = null)
        {
            EstimatedQuadCount = estimatedQuadCount;
            EstimatedTriangleCount = estimatedTriangleCount;
            EstimatedPeakMemoryBytes = estimatedPeakMemoryBytes;
            EstimatedComputationTime = estimatedComputationTime;
            RecommendedParallelism = recommendedParallelism;
            Complexity = complexity;
            PerformanceHints = performanceHints ?? Array.Empty<string>();
        }

        /// <summary>
        /// Returns a human-readable summary of the complexity estimate.
        /// </summary>
        public override string ToString()
        {
            var memoryMB = EstimatedPeakMemoryBytes / (1024.0 * 1024.0);
            return $"{Complexity} complexity: ~{EstimatedQuadCount + EstimatedTriangleCount} elements, " +
                   $"~{memoryMB:F1} MB peak, ~{EstimatedComputationTime.TotalMilliseconds:F0}ms";
        }
    }
}
