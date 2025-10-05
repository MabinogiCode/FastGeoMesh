using FastGeoMesh.Domain;
using FastGeoMesh.Meshing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastGeoMesh.Application
{
    /// <summary>Prism mesher producing quad-dominant meshes (side quads + cap quads, optional cap triangles).</summary>
    public sealed class PrismMesher : IMesher<PrismStructureDefinition>, IAsyncMesher
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
        public Result<Mesh> Mesh(PrismStructureDefinition input, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(options);

            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<Mesh>.Failure(validationResult.Error);
            }

            return Result<Mesh>.Success(CreateMeshInternal(input, options));
        }

        /// <summary>Generate a mesh asynchronously from the given prism structure definition and meshing options.</summary>
        public async ValueTask<Result<Mesh>> MeshAsync(PrismStructureDefinition input, MesherOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(options);

            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<Mesh>.Failure(validationResult.Error);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var complexity = EstimateComplexity(input);
            if (complexity == MeshingComplexity.Trivial && !cancellationToken.CanBeCanceled)
            {
                return Result<Mesh>.Success(CreateMeshInternal(input, options));
            }

            var mesh = await Task.Run(() => CreateMeshInternal(input, options), cancellationToken);
            return Result<Mesh>.Success(mesh);
        }

        // IAsyncMesher implementation
        /// <summary>Generate mesh asynchronously with progress reporting and cancellation support.</summary>
        public async ValueTask<Result<Mesh>> MeshWithProgressAsync(
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
                return Result<Mesh>.Failure(validationResult.Error);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var mesh = await Task.Run(() => CreateMeshInternalWithProgress(structureDefinition, options, progress, cancellationToken), cancellationToken);
            return Result<Mesh>.Success(mesh);
        }

        /// <summary>Generate multiple meshes in parallel with load balancing.</summary>
        public async ValueTask<Result<IReadOnlyList<Mesh>>> MeshBatchAsync(
            IEnumerable<PrismStructureDefinition> structures,
            MesherOptions options,
            int maxDegreeOfParallelism = -1,
            IProgress<MeshingProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(structures);
            ArgumentNullException.ThrowIfNull(options);

            var structureList = structures.ToList();
            if (!structureList.Any())
            {
                return Result<IReadOnlyList<Mesh>>.Failure(new Error("Validation.Input", "Structures collection cannot be empty."));
            }

            var validationResult = options.Validate();
            if (validationResult.IsFailure)
            {
                return Result<IReadOnlyList<Mesh>>.Failure(validationResult.Error);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (structureList.Count == 1)
            {
                var singleResult = await MeshWithProgressAsync(structureList[0], options, progress, cancellationToken);
                return singleResult.IsSuccess
                    ? Result<IReadOnlyList<Mesh>>.Success(new[] { singleResult.Value })
                    : Result<IReadOnlyList<Mesh>>.Failure(singleResult.Error);
            }

            var results = new Mesh[structureList.Count];
            var completedCount = 0;

            try
            {
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
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Result<IReadOnlyList<Mesh>>.Failure(new Error("Operation.Cancelled", "The batch meshing operation was cancelled."));
            }

            return Result<IReadOnlyList<Mesh>>.Success(results);
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
        public ValueTask<PerformanceMonitor.PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            return new ValueTask<PerformanceMonitor.PerformanceStatistics>(
                PerformanceMonitor.Counters.GetStatistics());
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
            if (complexity >= MeshingComplexity.Complex) hints.Add("Consider using parallel batch processing for multiple structures");
            if (structure.Holes.Count > 5) hints.Add("Large number of holes detected - consider hole refinement options");
            if (totalVertices > 500) hints.Add("Large geometry detected - async processing recommended");
            if (complexity == MeshingComplexity.Trivial) hints.Add("Simple geometry - synchronous processing is optimal");
            if (complexity == MeshingComplexity.Moderate && structure.Holes.Count > 0) hints.Add("Moderate complexity with holes - consider async processing for better performance");
            if (complexity >= MeshingComplexity.Moderate && complexity < MeshingComplexity.Complex && totalVertices > 50) hints.Add("Consider async processing for improved responsiveness");

            return new MeshingComplexityEstimate(estimatedQuads, estimatedTriangles, estimatedMemory, estimatedTime, recommendedParallelism, complexity, hints);
        }

        private Mesh CreateMeshInternal(PrismStructureDefinition structure, MesherOptions options)
        {
            using var activity = PerformanceMonitor.StartMeshingActivity("SyncMeshing", new { VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count), HoleCount = structure.Holes.Count, ConstraintCount = structure.ConstraintSegments.Count });
            PerformanceMonitor.Counters.IncrementMeshingOperations();
            var mesh = new Mesh();
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;
            var zLevels = MeshStructureHelper.BuildZLevels(z0, z1, options, structure);
            foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true)) mesh.AddQuad(q);
            foreach (var hole in structure.Holes) foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false)) mesh.AddQuad(q);
            if (options.GenerateBottomCap || options.GenerateTopCap) _capStrategy.GenerateCaps(mesh, structure, options, z0, z1);
            foreach (var p in structure.Geometry.Points) mesh.AddPoint(p);
            foreach (var s in structure.Geometry.Segments) mesh.AddInternalSegment(s);
            PerformanceMonitor.Counters.AddQuadsGenerated(mesh.QuadCount);
            PerformanceMonitor.Counters.AddTrianglesGenerated(mesh.TriangleCount);
            return mesh;
        }

        private Mesh CreateMeshInternalWithProgress(PrismStructureDefinition structure, MesherOptions options, IProgress<MeshingProgress>? progress, CancellationToken cancellationToken)
        {
            using var activity = PerformanceMonitor.StartMeshingActivity("AsyncMeshingWithProgress", new { VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count), HoleCount = structure.Holes.Count, EstimatedComplexity = EstimateComplexity(structure).ToString() });
            PerformanceMonitor.Counters.IncrementMeshingOperations();
            var mesh = new Mesh();
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;
            progress?.Report(new MeshingProgress("Initializing", 0.0, 0, 1, statusMessage: "Analyzing structure"));
            cancellationToken.ThrowIfCancellationRequested();
            var zLevels = MeshStructureHelper.BuildZLevels(z0, z1, options, structure);
            progress?.Report(new MeshingProgress("Side Faces", 0.1, 0, 1, statusMessage: "Generating side quads"));
            cancellationToken.ThrowIfCancellationRequested();
            var sideQuadCount = 0;
            var totalExpectedQuads = EstimateQuadCount(structure, options);
            foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true))
            {
                cancellationToken.ThrowIfCancellationRequested();
                mesh.AddQuad(q);
                sideQuadCount++;
                if (sideQuadCount % 10 == 0)
                {
                    var sideProgress = Math.Min(0.5, (double)sideQuadCount / totalExpectedQuads * 0.5);
                    progress?.Report(new MeshingProgress("Side Faces", 0.1 + sideProgress, sideQuadCount, totalExpectedQuads, statusMessage: $"Generated {sideQuadCount} side quads"));
                }
            }
            foreach (var hole in structure.Holes)
            {
                foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    mesh.AddQuad(q);
                    sideQuadCount++;
                    if (sideQuadCount % 10 == 0)
                    {
                        var sideProgress = Math.Min(0.5, (double)sideQuadCount / totalExpectedQuads * 0.5);
                        progress?.Report(new MeshingProgress("Side Faces", 0.1 + sideProgress, sideQuadCount, totalExpectedQuads, statusMessage: $"Generated {sideQuadCount} quads (including holes)"));
                    }
                }
            }
            if (options.GenerateBottomCap || options.GenerateTopCap)
            {
                progress?.Report(new MeshingProgress("Caps", 0.6, 0, 1, statusMessage: "Generating caps"));
                cancellationToken.ThrowIfCancellationRequested();
                _capStrategy.GenerateCaps(mesh, structure, options, z0, z1);
                cancellationToken.ThrowIfCancellationRequested();
            }
            progress?.Report(new MeshingProgress("Auxiliary", 0.9, 0, 1, statusMessage: "Adding auxiliary geometry"));
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var p in structure.Geometry.Points)
            {
                mesh.AddPoint(p);
                cancellationToken.ThrowIfCancellationRequested();
            }
            foreach (var s in structure.Geometry.Segments)
            {
                mesh.AddInternalSegment(s);
                cancellationToken.ThrowIfCancellationRequested();
            }
            PerformanceMonitor.Counters.AddQuadsGenerated(mesh.QuadCount);
            PerformanceMonitor.Counters.AddTrianglesGenerated(mesh.TriangleCount);
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(MeshingProgress.Completed("Meshing", mesh.QuadCount + mesh.TriangleCount));
            return mesh;
        }

        private static int EstimateQuadCount(PrismStructureDefinition structure, MesherOptions options)
        {
            var perimeterLength = structure.Footprint.Count * 4.0;
            var height = structure.TopElevation - structure.BaseElevation;
            var zSegments = Math.Max(1, (int)(height / options.TargetEdgeLengthZ.Value));
            var xySegments = Math.Max(1, (int)(perimeterLength / options.TargetEdgeLengthXY.Value));
            return xySegments * zSegments + structure.Footprint.Count * 2;
        }
    }
}
