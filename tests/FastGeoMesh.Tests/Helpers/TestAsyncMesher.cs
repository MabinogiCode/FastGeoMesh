using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastGeoMesh.Tests.Helpers
{
    internal sealed class TestAsyncMesher : IAsyncMesher
    {
        private readonly IAsyncMesher _actualMesher;
        /// <summary>
        /// Runs test TestAsyncMesher.
        /// </summary>
        public TestAsyncMesher()
            : this(TestServiceProvider.CreateDefaultProvider().GetRequiredService<IAsyncMesher>())
        {
        }
        /// <summary>
        /// Runs test TestAsyncMesher.
        /// </summary>
        public TestAsyncMesher(IAsyncMesher actualMesher)
        {
            _actualMesher = actualMesher;
        }
        /// <summary>
        /// Runs test Mesh.
        /// </summary>
        public Result<ImmutableMesh> Mesh(PrismStructureDefinition structureDefinition, MesherOptions options)
            => _actualMesher.Mesh(structureDefinition, options);
        /// <summary>
        /// Runs test MeshAsync.
        /// </summary>
        public async ValueTask<Result<ImmutableMesh>> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default)
            => await _actualMesher.MeshAsync(structureDefinition, options, cancellationToken).ConfigureAwait(true);
        /// <summary>
        /// Runs test MeshWithProgressAsync.
        /// </summary>
        public ValueTask<Result<ImmutableMesh>> MeshWithProgressAsync(PrismStructureDefinition structureDefinition, MesherOptions options, IProgress<MeshingProgress>? progress, CancellationToken cancellationToken = default)
            => _actualMesher.MeshWithProgressAsync(structureDefinition, options, progress, cancellationToken);
        /// <summary>
        /// Runs test MeshBatchAsync.
        /// </summary>
        public ValueTask<Result<IReadOnlyList<ImmutableMesh>>> MeshBatchAsync(IEnumerable<PrismStructureDefinition> structures, MesherOptions options, int maxDegreeOfParallelism = -1, IProgress<MeshingProgress>? progress = null, CancellationToken cancellationToken = default)
            => _actualMesher.MeshBatchAsync(structures, options, maxDegreeOfParallelism, progress, cancellationToken);
        /// <summary>
        /// Runs test EstimateComplexityAsync.
        /// </summary>
        public ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options)
            => _actualMesher.EstimateComplexityAsync(structureDefinition, options);
        /// <summary>
        /// Runs test GetLivePerformanceStatsAsync.
        /// </summary>
        public ValueTask<PerformanceStatistics> GetLivePerformanceStatsAsync()
            => _actualMesher.GetLivePerformanceStatsAsync();
    }
}

