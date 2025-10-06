using System.Threading;
using System.Threading.Tasks;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Tests
{
    /// <summary>Test implementation of async mesher interface for performance optimization tests.</summary>
    internal sealed class TestAsyncMesher : IAsyncMesher
    {
        private readonly PrismMesher _actualMesher = new();

        /// <summary>
        /// Synchronously meshes the provided structure definition with the supplied options.
        /// </summary>
        public Mesh Mesh(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            return _actualMesher.Mesh(structureDefinition, options);
        }

        /// <summary>
        /// Asynchronously meshes the provided structure definition simulating async workload.
        /// </summary>
        public async ValueTask<Mesh> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default)
        {
            // Simulate async work
            await Task.Delay(1, cancellationToken);
            return await Task.Run(() => _actualMesher.Mesh(structureDefinition, options), cancellationToken);
        }

        /// <summary>Generate mesh asynchronously with progress reporting and cancellation support.</summary>
        public ValueTask<Mesh> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress> progress,
            CancellationToken cancellationToken = default)
        {
            return _actualMesher.MeshWithProgressAsync(structureDefinition, options, progress, cancellationToken);
        }

        /// <summary>Generate multiple meshes in parallel with load balancing.</summary>
        public ValueTask<IReadOnlyList<Mesh>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            return _actualMesher.MeshBatchAsync(structures, options, maxDegreeOfParallelism, progress, cancellationToken);
        }

        /// <summary>Estimates the computational complexity and memory requirements for a meshing operation.</summary>
        public ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            return _actualMesher.EstimateComplexityAsync(structureDefinition, options);
        }

        /// <summary>Gets real-time performance statistics for monitoring and optimization.</summary>
        public ValueTask<PerformanceMonitor.PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            return _actualMesher.GetLivePerformanceStatsAsync();
        }
    }
}
