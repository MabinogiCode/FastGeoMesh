using FastGeoMesh.Domain;

namespace FastGeoMesh.Meshing
{
    public interface IAsyncMesher : IMesher<PrismStructureDefinition>
    {
        ValueTask<Mesh> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken = default);

        ValueTask<IReadOnlyList<Mesh>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress>? progress = null,
            CancellationToken cancellationToken = default);

        ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options);

        ValueTask<PerformanceMonitor.PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            return new ValueTask<PerformanceMonitor.PerformanceStatistics>(
                PerformanceMonitor.Counters.GetStatistics());
        }
    }
}
