using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Tests.Helpers {
    /// <summary>Test implementation of async mesher interface for performance optimization tests.</summary>
    internal sealed class TestAsyncMesher : IAsyncMesher {
        private readonly PrismMesher _actualMesher = new();

        /// <summary>
        /// Synchronously meshes the provided structure definition with the supplied options.
        /// </summary>
        public Result<ImmutableMesh> Mesh(PrismStructureDefinition structureDefinition, MesherOptions options)
            => _actualMesher.Mesh(structureDefinition, options);

        /// <summary>
        /// Asynchronously meshes the provided structure definition simulating async workload.
        /// </summary>
        public async ValueTask<Result<ImmutableMesh>> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default)
            => await _actualMesher.MeshAsync(structureDefinition, options, cancellationToken);

        /// <summary>Generate mesh asynchronously with progress reporting and cancellation support.</summary>
        public ValueTask<Result<ImmutableMesh>> MeshWithProgressAsync(PrismStructureDefinition structureDefinition, MesherOptions options, IProgress<MeshingProgress>? progress, CancellationToken cancellationToken = default)
            => _actualMesher.MeshWithProgressAsync(structureDefinition, options, progress, cancellationToken);

        /// <summary>Generate multiple meshes in parallel with load balancing.</summary>
        public ValueTask<Result<IReadOnlyList<ImmutableMesh>>> MeshBatchAsync(IEnumerable<PrismStructureDefinition> structures, MesherOptions options, int maxDegreeOfParallelism = -1, IProgress<MeshingProgress>? progress = null, CancellationToken cancellationToken = default)
            => _actualMesher.MeshBatchAsync(structures, options, maxDegreeOfParallelism, progress, cancellationToken);

        /// <summary>Estimates the computational complexity and memory requirements for a meshing operation.</summary>
        public ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options)
            => _actualMesher.EstimateComplexityAsync(structureDefinition, options);

        /// <summary>Gets real-time performance statistics for monitoring and optimization.</summary>
        public ValueTask<PerformanceStatistics> GetLivePerformanceStatsAsync()
            => _actualMesher.GetLivePerformanceStatsAsync();
    }
}

