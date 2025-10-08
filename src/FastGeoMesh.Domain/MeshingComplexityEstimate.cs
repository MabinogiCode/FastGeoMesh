namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Defines the complexity levels for meshing operations.
    /// </summary>
    public enum MeshingComplexity
    {
        /// <summary>
        /// Very simple structures that can use optimized fast paths.
        /// </summary>
        Trivial,
        
        /// <summary>
        /// Simple structures with moderate complexity.
        /// </summary>
        Simple,
        
        /// <summary>
        /// Moderately complex structures requiring standard algorithms.
        /// </summary>
        Moderate,
        
        /// <summary>
        /// Complex structures requiring advanced algorithms.
        /// </summary>
        Complex,
        
        /// <summary>
        /// Very complex structures requiring extensive computation.
        /// </summary>
        VeryComplex,
        
        /// <summary>
        /// Extremely complex structures requiring specialized algorithms and maximum optimization.
        /// </summary>
        Extreme
    }

    /// <summary>
    /// Provides complexity estimates and performance predictions for meshing operations.
    /// </summary>
    public readonly struct MeshingComplexityEstimate
    {
        /// <summary>
        /// Gets the estimated number of quadrilaterals that will be generated.
        /// </summary>
        public int EstimatedQuadCount { get; }
        
        /// <summary>
        /// Gets the estimated number of triangles that will be generated.
        /// </summary>
        public int EstimatedTriangleCount { get; }
        
        /// <summary>
        /// Gets the estimated peak memory usage in bytes during meshing.
        /// </summary>
        public long EstimatedPeakMemoryBytes { get; }
        
        /// <summary>
        /// Gets the estimated computation time for the meshing operation.
        /// </summary>
        public TimeSpan EstimatedComputationTime { get; }
        
        /// <summary>
        /// Gets the recommended degree of parallelism for optimal performance.
        /// </summary>
        public int RecommendedParallelism { get; }
        
        /// <summary>
        /// Gets the overall complexity classification of the meshing operation.
        /// </summary>
        public MeshingComplexity Complexity { get; }
        
        /// <summary>
        /// Gets a collection of performance optimization hints and recommendations.
        /// </summary>
        public IReadOnlyList<string> PerformanceHints { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshingComplexityEstimate"/> struct.
        /// </summary>
        /// <param name="estimatedQuadCount">Expected number of quadrilaterals.</param>
        /// <param name="estimatedTriangleCount">Expected number of triangles.</param>
        /// <param name="estimatedPeakMemoryBytes">Expected peak memory usage in bytes.</param>
        /// <param name="estimatedComputationTime">Expected computation time.</param>
        /// <param name="recommendedParallelism">Recommended parallelism level.</param>
        /// <param name="complexity">Overall complexity classification.</param>
        /// <param name="performanceHints">Optional performance optimization hints.</param>
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
        /// Returns a string representation of the complexity estimate.
        /// </summary>
        /// <returns>A formatted string summarizing the complexity estimates.</returns>
        public override string ToString()
        {
            var memoryMB = EstimatedPeakMemoryBytes / (1024.0 * 1024.0);
            return $"{Complexity} complexity: ~{EstimatedQuadCount + EstimatedTriangleCount} elements, " +
                   $"~{memoryMB:F1} MB peak, ~{EstimatedComputationTime.TotalMilliseconds:F0}ms";
        }
    }
}
