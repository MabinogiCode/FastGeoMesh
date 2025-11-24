using FastGeoMesh.Application.Helpers;
using FastGeoMesh.Application.Helpers.Meshing;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>Prism mesher producing quad-dominant meshes (side quads + cap quads, optional cap triangles).</summary>
    public sealed class PrismMesher : IAsyncMesher, IPrismMesher
    {
        private readonly ICapMeshingStrategy _capStrategy;
        private readonly IPerformanceMonitor _performanceMonitor;
        private readonly IGeometryService _geometryService;
        private readonly IZLevelBuilder _zLevelBuilder;

        /// <summary>Create a mesher with required services.</summary>
        public PrismMesher(
            IGeometryService geometryService,
            IZLevelBuilder zLevelBuilder,
            IProximityChecker proximityChecker)
            : this(
                new Strategies.DefaultCapMeshingStrategy(geometryService),
                new NullPerformanceMonitor(),
                geometryService,
                zLevelBuilder,
                proximityChecker)
        {
        }

        /// <summary>Create a mesher with custom cap strategy.</summary>
        public PrismMesher(
            ICapMeshingStrategy capStrategy,
            IGeometryService geometryService,
            IZLevelBuilder zLevelBuilder,
            IProximityChecker proximityChecker)
            : this(capStrategy, new NullPerformanceMonitor(), geometryService, zLevelBuilder, proximityChecker)
        {
        }

        /// <summary>Create a mesher with all dependencies.</summary>
        public PrismMesher(
            ICapMeshingStrategy capStrategy,
            IPerformanceMonitor performanceMonitor,
            IGeometryService geometryService,
            IZLevelBuilder zLevelBuilder,
            IProximityChecker proximityChecker)
        {
            _capStrategy = capStrategy ?? throw new System.ArgumentNullException(nameof(capStrategy));
            _performanceMonitor = performanceMonitor ?? throw new System.ArgumentNullException(nameof(performanceMonitor));
            _geometryService = geometryService ?? throw new System.ArgumentNullException(nameof(geometryService));
            _zLevelBuilder = zLevelBuilder ?? throw new System.ArgumentNullException(nameof(zLevelBuilder));
        }

        /// <summary>
        /// Generates a mesh from the given prism structure definition and meshing options.
        /// </summary>
        /// <param name="input">The prism structure definition.</param>
        /// <param name="options">Meshing options.</param>
        /// <returns>Result containing the generated immutable mesh or an error.</returns>
        public Result<ImmutableMesh> Mesh(PrismStructureDefinition input, MesherOptions options)
        {
            System.ArgumentNullException.ThrowIfNull(input);
            System.ArgumentNullException.ThrowIfNull(options);

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
            catch (System.ArgumentException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArgumentError", ex.Message));
            }
            catch (System.InvalidOperationException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.OperationError", ex.Message));
            }
            catch (System.ArithmeticException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArithmeticError",
                    $"Arithmetic error during meshing (overflow, division by zero, etc.): {ex.Message}"));
            }
            catch (System.IndexOutOfRangeException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.IndexError",
                    $"Index out of range during meshing: {ex.Message}"));
            }
            catch (System.Exception ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during meshing: {ex.Message}"));
            }
        }

        /// <summary>
        /// Asynchronously generates a mesh from the given prism structure definition and meshing options.
        /// </summary>
        /// <param name="input">The prism structure definition.</param>
        /// <param name="options">Meshing options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A ValueTask containing the meshing result.</returns>
        public async ValueTask<Result<ImmutableMesh>> MeshAsync(PrismStructureDefinition input, MesherOptions options, CancellationToken cancellationToken = default)
        {
            System.ArgumentNullException.ThrowIfNull(input);
            System.ArgumentNullException.ThrowIfNull(options);

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

                var complexity = PrismMesherHelper.EstimateComplexity(input);
                if (complexity == MeshingComplexity.Trivial && !cancellationToken.CanBeCanceled)
                {
                    var mesh = CreateMeshInternal(input, options);
                    return Result<ImmutableMesh>.Success(mesh);
                }

                var asyncMesh = await Task.Run(() => CreateMeshInternal(input, options), cancellationToken).ConfigureAwait(false);
                return Result<ImmutableMesh>.Success(asyncMesh);
            }
            catch (System.OperationCanceledException)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.Cancelled", "Meshing operation was cancelled"));
            }
            catch (System.ArgumentException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArgumentError", ex.Message));
            }
            catch (System.InvalidOperationException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.OperationError", ex.Message));
            }
            catch (System.ArithmeticException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArithmeticError",
                    $"Arithmetic error during async meshing: {ex.Message}"));
            }
            catch (System.IndexOutOfRangeException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.IndexError",
                    $"Index out of range during async meshing: {ex.Message}"));
            }
            catch (System.Exception ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during async meshing: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generates a mesh asynchronously with progress reporting and cancellation support.
        /// </summary>
        public async ValueTask<Result<ImmutableMesh>> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken = default)
        {
            System.ArgumentNullException.ThrowIfNull(structureDefinition);
            System.ArgumentNullException.ThrowIfNull(options);

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
            catch (System.OperationCanceledException)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.Cancelled", "Meshing operation was cancelled"));
            }
            catch (System.ArgumentException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArgumentError", ex.Message));
            }
            catch (System.InvalidOperationException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.OperationError", ex.Message));
            }
            catch (System.ArithmeticException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.ArithmeticError",
                    $"Arithmetic error during meshing with progress: {ex.Message}"));
            }
            catch (System.IndexOutOfRangeException ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.IndexError",
                    $"Index out of range during meshing with progress: {ex.Message}"));
            }
            catch (System.Exception ex)
            {
                return Result<ImmutableMesh>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during meshing with progress: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generates multiple meshes in parallel with load balancing.
        /// </summary>
        public async ValueTask<Result<IReadOnlyList<ImmutableMesh>>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            System.ArgumentNullException.ThrowIfNull(structures);
            System.ArgumentNullException.ThrowIfNull(options);

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

                // Use Parallel.ForEachAsync to provide a cooperative async parallel loop that respects cancellation
                var totalComplexity = structureList.Sum(s => (int)PrismMesherHelper.EstimateComplexity(s));
                var optimalParallelism = Math.Min(
                    maxDegreeOfParallelism == -1 ? System.Environment.ProcessorCount : maxDegreeOfParallelism,
                    Math.Max(1, totalComplexity / 4)
                );

                await Parallel.ForEachAsync(Enumerable.Range(0, structureList.Count), new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = optimalParallelism
                }, (index, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    results[index] = CreateMeshInternal(structureList[index], options);
                    var completed = System.Threading.Interlocked.Increment(ref completedCount);
                    progress?.Report(MeshingProgress.FromCounts("Batch Processing", completed, structureList.Count));
                    return ValueTask.CompletedTask;
                }).ConfigureAwait(false);

                return Result<IReadOnlyList<ImmutableMesh>>.Success(results);
            }
            catch (System.OperationCanceledException)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.Cancelled",
                    "Batch meshing operation was cancelled"));
            }
            catch (System.ArgumentException ex)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.ArgumentError", ex.Message));
            }
            catch (System.InvalidOperationException ex)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.OperationError", ex.Message));
            }
            catch (System.ArithmeticException ex)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.ArithmeticError",
                    $"Arithmetic error during batch meshing: {ex.Message}"));
            }
            catch (System.IndexOutOfRangeException ex)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.IndexError",
                    $"Index out of range during batch meshing: {ex.Message}"));
            }
            catch (System.Exception ex)
            {
                return Result<IReadOnlyList<ImmutableMesh>>.Failure(new Error("Meshing.UnexpectedError",
                    $"Unexpected error during batch meshing: {ex.Message}"));
            }
        }

        /// <summary>Estimates the computational complexity and memory requirements for a meshing operation.</summary>
        public ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            System.ArgumentNullException.ThrowIfNull(structureDefinition);
            System.ArgumentNullException.ThrowIfNull(options);

            var estimate = PrismMesherHelper.EstimateComplexity(structureDefinition);
            return new ValueTask<MeshingComplexityEstimate>(PrismMesherHelper.CreateDetailedEstimate(structureDefinition, estimate));
        }

        /// <summary>Gets real-time performance statistics for this mesher instance.</summary>
        public ValueTask<PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            var stats = _performanceMonitor.GetLiveStatistics();
            return new ValueTask<PerformanceStatistics>(stats);
        }

        private ImmutableMesh CreateMeshInternal(PrismStructureDefinition structure, MesherOptions options)
        {
            using var activity = _performanceMonitor.StartMeshingActivity("SyncMeshing", new { VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count), HoleCount = structure.Holes.Count, ConstraintCount = structure.ConstraintSegments.Count });
            _performanceMonitor.IncrementMeshingOperations();

            var mesh = ImmutableMesh.Empty;
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;

            var zLevels = _zLevelBuilder.BuildZLevels(z0, z1, options, structure);

            // Generate side faces
            var sideQuads = SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true, _geometryService);
            mesh = mesh.AddQuads(sideQuads);

            foreach (var hole in structure.Holes)
            {
                var holeQuads = SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false, _geometryService);
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

            _performanceMonitor.AddQuadsGenerated(mesh.QuadCount);
            _performanceMonitor.AddTrianglesGenerated(mesh.TriangleCount);
            return mesh;
        }

        private ImmutableMesh CreateMeshInternalWithProgress(PrismStructureDefinition structure, MesherOptions options, IProgress<MeshingProgress>? progress, CancellationToken cancellationToken)
        {
            using var activity = _performanceMonitor.StartMeshingActivity("AsyncMeshingWithProgress", new { VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count), HoleCount = structure.Holes.Count, EstimatedComplexity = PrismMesherHelper.EstimateComplexity(structure).ToString() });
            _performanceMonitor.IncrementMeshingOperations();

            var mesh = ImmutableMesh.Empty;
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;

            progress?.Report(new MeshingProgress("Initializing", 0.0, 0, 1, statusMessage: "Analyzing structure"));
            cancellationToken.ThrowIfCancellationRequested();

            var zLevels = _zLevelBuilder.BuildZLevels(z0, z1, options, structure);

            progress?.Report(new MeshingProgress("Side Faces", 0.1, 0, 1, statusMessage: "Generating side quads"));
            cancellationToken.ThrowIfCancellationRequested();

            // Generate side faces with progress tracking
            var sideQuads = SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true, _geometryService);
            mesh = mesh.AddQuads(sideQuads);

            foreach (var hole in structure.Holes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var holeQuads = SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false, _geometryService);
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

            _performanceMonitor.AddQuadsGenerated(mesh.QuadCount);
            _performanceMonitor.AddTrianglesGenerated(mesh.TriangleCount);

            progress?.Report(MeshingProgress.Completed("Meshing", mesh.QuadCount + mesh.TriangleCount));
            return mesh;
        }
    }
}
