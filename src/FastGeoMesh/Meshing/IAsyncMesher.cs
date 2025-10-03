using FastGeoMesh.Structures;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Asynchronous meshing interface with cancellation and progress support.
    /// Extends the base IMesher interface with async capabilities for large-scale operations.
    /// </summary>
    public interface IAsyncMesher : IMesher<PrismStructureDefinition>
    {
        /// <summary>
        /// Generate mesh asynchronously with progress reporting and cancellation support.
        /// </summary>
        /// <param name="structureDefinition">The prismatic structure to mesh.</param>
        /// <param name="options">Meshing options and parameters.</param>
        /// <param name="progress">Progress reporter for operation updates.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>ValueTask that resolves to the generated mesh.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <exception cref="ArgumentNullException">Thrown when structure or options are null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when meshing fails due to invalid geometry.</exception>
        ValueTask<Mesh> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate multiple meshes in parallel with load balancing.
        /// </summary>
        /// <param name="structures">Collection of structures to mesh.</param>
        /// <param name="options">Meshing options applied to all structures.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of parallel operations. Use -1 for unlimited.</param>
        /// <param name="progress">Progress reporter for batch operation updates.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>ValueTask that resolves to a collection of generated meshes in the same order as input structures.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <exception cref="ArgumentNullException">Thrown when structures or options are null.</exception>
        /// <exception cref="ArgumentException">Thrown when structures collection is empty.</exception>
        ValueTask<IReadOnlyList<Mesh>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Estimates the computational complexity and memory requirements for a meshing operation.
        /// Useful for progress estimation and resource planning.
        /// </summary>
        /// <param name="structureDefinition">The structure to analyze.</param>
        /// <param name="options">Meshing options that would be used.</param>
        /// <returns>ValueTask that resolves to complexity estimation.</returns>
        ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options);

        /// <summary>
        /// Gets real-time performance statistics for monitoring and optimization.
        /// Provides insights into pool efficiency, operation counts, and performance metrics.
        /// </summary>
        /// <returns>ValueTask that resolves to current performance statistics.</returns>
        ValueTask<PerformanceMonitor.PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            // Default implementation - can be overridden by implementers
            return new ValueTask<PerformanceMonitor.PerformanceStatistics>(
                PerformanceMonitor.Counters.GetStatistics());
        }
    }

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

    /// <summary>
    /// Classification of meshing operation complexity.
    /// </summary>
    public enum MeshingComplexity
    {
        /// <summary>Simple geometry with minimal computational requirements.</summary>
        Trivial,
        /// <summary>Straightforward geometry suitable for synchronous processing.</summary>
        Simple,
        /// <summary>Moderate complexity benefiting from async processing.</summary>
        Moderate,
        /// <summary>Complex geometry requiring parallel processing.</summary>
        Complex,
        /// <summary>Very large or intricate geometry requiring advanced optimization.</summary>
        Extreme
    }
}
