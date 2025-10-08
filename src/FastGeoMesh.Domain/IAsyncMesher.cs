namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Defines an asynchronous interface for mesh generation operations with advanced features.
    /// </summary>
    public interface IAsyncMesher : IMesher<PrismStructureDefinition>
    {
        /// <summary>
        /// Generates a mesh asynchronously with progress reporting support.
        /// </summary>
        /// <param name="structureDefinition">The prism structure to mesh.</param>
        /// <param name="options">Configuration options for the meshing operation.</param>
        /// <param name="progress">Optional progress reporter for operation tracking.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A ValueTask containing the generated mesh result.</returns>
        ValueTask<Result<ImmutableMesh>> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes multiple structures in parallel with optional progress reporting.
        /// </summary>
        /// <param name="structures">The collection of structures to mesh.</param>
        /// <param name="options">Configuration options for the meshing operations.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of parallel operations (-1 for default).</param>
        /// <param name="progress">Optional progress reporter for batch operation tracking.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A ValueTask containing the collection of generated meshes result.</returns>
        ValueTask<Result<IReadOnlyList<ImmutableMesh>>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Estimates the computational complexity of meshing a given structure.
        /// </summary>
        /// <param name="structureDefinition">The structure to analyze.</param>
        /// <param name="options">Configuration options that will affect complexity.</param>
        /// <returns>A ValueTask containing complexity estimates and performance hints.</returns>
        ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options);

        /// <summary>
        /// Gets current performance statistics for monitoring system health.
        /// </summary>
        /// <returns>A ValueTask containing current performance statistics.</returns>
        ValueTask<Domain.Services.PerformanceStatistics> GetLivePerformanceStatsAsync();
    }
}
