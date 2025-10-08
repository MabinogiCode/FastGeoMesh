using FastGeoMesh.Domain;
using FastGeoMesh.Meshing.Helpers;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Application
{
    /// <summary>Prism mesher producing quad-dominant meshes (side quads + cap quads, optional cap triangles).</summary>
    public sealed class PrismMesher : IAsyncMesher
    {
        private readonly ICapMeshingStrategy _capStrategy;

        /// <summary>Create a mesher with default cap strategy.</summary>
        public PrismMesher() : this(new DefaultCapMeshingStrategy()) { }

        /// <summary>Create a mesher with a custom cap strategy.</summary>
        public PrismMesher(ICapMeshingStrategy capStrategy)
        {
            _capStrategy = capStrategy ?? throw new ArgumentNullException(nameof(capStrategy));
        }

        /// <summary>Generate a mesh from the given prism structure definition and meshing options (thread-safe – no shared state).</summary>
        public Result<ImmutableMesh> Mesh(PrismStructureDefinition input, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(options);

            // Validate options first
            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ValidationError",
                    $"Invalid meshing options: {validationResult.Error.Description}"));
            }

            try
            {
                var mesh = CreateMeshInternal(input, options);
                return Result<ImmutableMesh>.Success(mesh);
            }
            catch (ArgumentException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArgumentError", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.OperationError", ex.Message));
            }
            catch (Exception ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during meshing: {ex.Message}"));
            }
        }

        /// <summary>Generate a mesh asynchronously from the given prism structure definition and meshing options.</summary>
        public async ValueTask<Result<ImmutableMesh>> MeshAsync(PrismStructureDefinition input, MesherOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(options);

            // Validate options first
            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ValidationError",
                    $"Invalid meshing options: {validationResult.Error.Description}"));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var complexity = EstimateComplexity(input);
                if (complexity == MeshingComplexity.Trivial && !cancellationToken.CanBeCanceled)
                {
                    var mesh = CreateMeshInternal(input, options);
                    return Result<ImmutableMesh>.Success(mesh);
                }

                var asyncMesh = await Task.Run(() => CreateMeshInternal(input, options), cancellationToken).ConfigureAwait(false);
                return Result<ImmutableMesh>.Success(asyncMesh);
            }
            catch (OperationCanceledException)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.Cancelled", "Meshing operation was cancelled"));
            }
            catch (ArgumentException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArgumentError", ex.Message));
            }
            catch (Exception ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during async meshing: {ex.Message}"));
            }
        }

        // IAsyncMesher implementation
        /// <summary>Generate mesh asynchronously with progress reporting and cancellation support.</summary>
        public async ValueTask<Result<ImmutableMesh>> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(structureDefinition);
            ArgumentNullException.ThrowIfNull(options);

            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ValidationError",
                    $"Invalid meshing options: {validationResult.Error.Description}"));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mesh = await Task.Run(() => CreateMeshInternalWithProgress(structureDefinition, options, progress, cancellationToken), cancellationToken).ConfigureAwait(false);
                return Result<ImmutableMesh>.Success(mesh);
            }
            catch (OperationCanceledException)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.Cancelled", "Meshing operation was cancelled"));
            }
            catch (Exception ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during meshing with progress: {ex.Message}"));
            }
        }

        /// <summary>Generate multiple meshes in parallel with load balancing.</summary>
        public async ValueTask<Result<IReadOnlyList<ImmutableMesh>>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(structures);
            ArgumentNullException.ThrowIfNull(options);

            var structureList = structures.ToList();
            if (structureList.Count == 0)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.EmptyBatch",
                    "Structures collection cannot be empty"));
            }

            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.ValidationError",
                    $"Invalid meshing options: {validationResult.Error.Description}"));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (structureList.Count == 1)
                {
                    var singleResult = await MeshWithProgressAsync(structureList[0], options, progress, cancellationToken).ConfigureAwait(false);
                    if (singleResult.IsFailure)
                    {
                        return Result<IReadOnlyList<ImmutableMesh>>.Failure(singleResult.Error);
                    }
                    return Result<IReadOnlyList<ImmutableMesh>>.Success(new[] { singleResult.Value });
                }

                var results = new ImmutableMesh[structureList.Count];
                var completedCount = 0;

                await Task.Run(() =>
                {
                    var totalComplexity = structureList.Sum(s => (int)EstimateComplexity(s));
                    var optimalParallelism = Math.Min(
                        maxDegreeOfParallelism == -1 ? Environment.ProcessorCount : maxDegreeOfParallelism,
                        Math.Max(1, totalComplexity / 4)
                    );

                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = optimalParallelism
                    };

                    Parallel.For(0, structureList.Count, parallelOptions, index =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        results[index] = CreateMeshInternal(structureList[index], options);
                        var completed = Interlocked.Increment(ref completedCount);
                        progress?.Report(MeshingProgress.FromCounts("Batch Processing", completed, structureList.Count));
                    });
                }, cancellationToken).ConfigureAwait(false);

                return Result<IReadOnlyList<ImmutableMesh>>.Success(results);
            }
            catch (OperationCanceledException)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.Cancelled",
                    "Batch meshing operation was cancelled"));
            }
            catch (Exception ex)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during batch meshing: {ex.Message}"));
            }
        }

        /// <summary>Estimates the computational complexity and memory requirements for a meshing operation.</summary>
        public ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(structureDefinition);
            ArgumentNullException.ThrowIfNull(options);

            var estimate = EstimateComplexity(structureDefinition);
            return new ValueTask<MeshingComplexityEstimate>(CreateDetailedEstimate(structureDefinition, estimate));
        }

        /// <summary>Gets real-time performance statistics for this mesher instance.</summary>
        public ValueTask<Domain.Services.PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            var infraStats = PerformanceMonitor.Counters.GetStatistics();
            var domainStats = new Domain.Services.PerformanceStatistics
            {
                MeshingOperations = infraStats.MeshingOperations,
                QuadsGenerated = infraStats.QuadsGenerated,
                TrianglesGenerated = infraStats.TrianglesGenerated,
                PoolHitRate = infraStats.PoolHitRate
            };
            return new ValueTask<Domain.Services.PerformanceStatistics>(domainStats);
        }

        private static MeshingComplexity EstimateComplexity(PrismStructureDefinition structure)
        {
            var totalVertices = structure.Footprint.Count + structure.Holes.Sum(h => h.Count);
            return totalVertices switch
            {
                < 10 => MeshingComplexity.Trivial,
                < 50 => MeshingComplexity.Simple,
                < 200 => MeshingComplexity.Moderate,
                < 1000 => MeshingComplexity.Complex,
                _ => MeshingComplexity.Extreme
            };
        }

        private static MeshingComplexityEstimate CreateDetailedEstimate(PrismStructureDefinition structure, MeshingComplexity complexity)
        {
            var footprintVertices = structure.Footprint.Count;
            var holeVertices = structure.Holes.Sum(h => h.Count);
            var totalVertices = footprintVertices + holeVertices;
            var estimatedQuads = (int)(totalVertices * 1.5 + structure.InternalSurfaces.Count * 10);
            var estimatedTriangles = Math.Max(1, (int)(totalVertices * 0.3));
            var estimatedMemory = (estimatedQuads + estimatedTriangles) * 160L;
            var estimatedTime = complexity switch
            {
                MeshingComplexity.Trivial => TimeSpan.FromMicroseconds(80),
                MeshingComplexity.Simple => TimeSpan.FromMicroseconds(240),
                MeshingComplexity.Moderate => TimeSpan.FromMicroseconds(800),
                MeshingComplexity.Complex => TimeSpan.FromMilliseconds(4),
                MeshingComplexity.Extreme => TimeSpan.FromMilliseconds(16),
                _ => TimeSpan.FromMicroseconds(800)
            };
            var recommendedParallelism = complexity >= MeshingComplexity.Complex ? Math.Min(Environment.ProcessorCount, 4) : 1;
            var hints = new List<string>();
            if (complexity >= MeshingComplexity.Complex)
            {
                hints.Add("Consider using parallel batch processing for multiple structures");
            }

            if (structure.Holes.Count > 5)
            {
                hints.Add("Large number of holes detected - consider hole refinement options");
            }

            if (totalVertices > 500)
            {
                hints.Add("Large geometry detected - async processing recommended");
            }

            if (complexity == MeshingComplexity.Trivial)
            {
                hints.Add("Simple geometry - synchronous processing is optimal");
            }

            if (complexity == MeshingComplexity.Moderate && structure.Holes.Count > 0)
            {
                hints.Add("Moderate complexity with holes - consider async processing for better performance");
            }

            if (complexity >= MeshingComplexity.Moderate && complexity < MeshingComplexity.Complex && totalVertices > 50)
            {
                hints.Add("Consider async processing for improved responsiveness");
            }

            return new MeshingComplexityEstimate(estimatedQuads, estimatedTriangles, estimatedMemory, estimatedTime, recommendedParallelism, complexity, hints);
        }

        private ImmutableMesh CreateMeshInternal(PrismStructureDefinition structure, MesherOptions options)
        {
            using var activity = PerformanceMonitor.StartMeshingActivity("SyncMeshing", new { VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count), HoleCount = structure.Holes.Count, ConstraintCount = structure.ConstraintSegments.Count });
            PerformanceMonitor.Counters.IncrementMeshingOperations();

            var mesh = ImmutableMesh.Empty;
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;

            var zLevels = MeshStructureHelper.BuildZLevels(z0, z1, options, structure);

            // Generate side faces
            var sideQuads = SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true);
            mesh = mesh.AddQuads(sideQuads);

            foreach (var hole in structure.Holes)
            {
                var holeQuads = SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false);
                mesh = mesh.AddQuads(holeQuads);
            }

            // Generate caps
            if (options.GenerateBottomCap || options.GenerateTopCap)
            {
                var capGeometry = _capStrategy.GenerateCaps(structure, options, z0, z1);
                mesh = mesh.AddQuads(capGeometry.Quads);
                mesh = mesh.AddTriangles(capGeometry.Triangles);
            }

            // Add auxiliary geometry
            mesh = mesh.AddPoints(structure.Geometry.Points);
            foreach (var segment in structure.Geometry.Segments)
            {
                mesh = mesh.AddInternalSegment(segment);
            }

            PerformanceMonitor.Counters.AddQuadsGenerated(mesh.QuadCount);
            PerformanceMonitor.Counters.AddTrianglesGenerated(mesh.TriangleCount);
            return mesh;
        }

        private ImmutableMesh CreateMeshInternalWithProgress(PrismStructureDefinition structure, MesherOptions options, IProgress<MeshingProgress>? progress, CancellationToken cancellationToken)
        {
            using var activity = PerformanceMonitor.StartMeshingActivity("AsyncMeshingWithProgress", new { VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count), HoleCount = structure.Holes.Count, EstimatedComplexity = EstimateComplexity(structure).ToString() });
            PerformanceMonitor.Counters.IncrementMeshingOperations();

            var mesh = ImmutableMesh.Empty;
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;

            progress?.Report(new MeshingProgress("Initializing", 0.0, 0, 1, statusMessage: "Analyzing structure"));
            cancellationToken.ThrowIfCancellationRequested();

            var zLevels = MeshStructureHelper.BuildZLevels(z0, z1, options, structure);

            progress?.Report(new MeshingProgress("Side Faces", 0.1, 0, 1, statusMessage: "Generating side quads"));
            cancellationToken.ThrowIfCancellationRequested();

            // Generate side faces with progress tracking
            var sideQuads = SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true);
            mesh = mesh.AddQuads(sideQuads);

            foreach (var hole in structure.Holes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var holeQuads = SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false);
                mesh = mesh.AddQuads(holeQuads);
            }

            if (options.GenerateBottomCap || options.GenerateTopCap)
            {
                progress?.Report(new MeshingProgress("Caps", 0.6, 0, 1, statusMessage: "Generating caps"));
                cancellationToken.ThrowIfCancellationRequested();

                var capGeometry = _capStrategy.GenerateCaps(structure, options, z0, z1);
                mesh = mesh.AddQuads(capGeometry.Quads);
                mesh = mesh.AddTriangles(capGeometry.Triangles);
            }

            progress?.Report(new MeshingProgress("Auxiliary", 0.9, 0, 1, statusMessage: "Adding auxiliary geometry"));
            cancellationToken.ThrowIfCancellationRequested();

            mesh = mesh.AddPoints(structure.Geometry.Points);
            foreach (var segment in structure.Geometry.Segments)
            {
                mesh = mesh.AddInternalSegment(segment);
                cancellationToken.ThrowIfCancellationRequested();
            }

            PerformanceMonitor.Counters.AddQuadsGenerated(mesh.QuadCount);
            PerformanceMonitor.Counters.AddTrianglesGenerated(mesh.TriangleCount);

            progress?.Report(MeshingProgress.Completed("Meshing", mesh.QuadCount + mesh.TriangleCount));
            return mesh;
        }
    }
}
