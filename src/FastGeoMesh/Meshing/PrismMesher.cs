using FastGeoMesh.Meshing.Helpers;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Meshing
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
        public Mesh Mesh(PrismStructureDefinition input, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();

            return CreateMeshInternal(input, options);
        }

        /// <summary>Generate a mesh asynchronously from the given prism structure definition and meshing options.</summary>
        public ValueTask<Mesh> MeshAsync(PrismStructureDefinition input, MesherOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();

            cancellationToken.ThrowIfCancellationRequested();

            // 🚀 PERFORMANCE: For simple structures, return synchronously to avoid Task overhead
            var complexity = EstimateComplexity(input);
            if (complexity == MeshingComplexity.Trivial && !cancellationToken.CanBeCanceled)
            {
                return new ValueTask<Mesh>(CreateMeshInternal(input, options));
            }

            // For CPU-bound operations in library code, use Task.Run to offload to thread pool
            // Return ValueTask wrapping the Task for better performance when caching results
            return new ValueTask<Mesh>(Task.Run(() => CreateMeshInternal(input, options), cancellationToken));
        }

        // IAsyncMesher implementation
        /// <summary>Generate mesh asynchronously with progress reporting and cancellation support.</summary>
        /// <param name="structureDefinition">The prismatic structure to mesh.</param>
        /// <param name="options">Meshing options and parameters.</param>
        /// <param name="progress">Progress reporter for operation updates.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>ValueTask that resolves to the generated mesh.</returns>
        public ValueTask<Mesh> MeshWithProgressAsync(
            PrismStructureDefinition structureDefinition,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(structureDefinition);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();

            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<Mesh>(Task.Run(() => CreateMeshInternalWithProgress(structureDefinition, options, progress, cancellationToken), cancellationToken));
        }

        /// <summary>Generate multiple meshes in parallel with load balancing.</summary>
        /// <param name="structures">Collection of structures to mesh.</param>
        /// <param name="options">Meshing options applied to all structures.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of parallel operations. Use -1 for unlimited.</param>
        /// <param name="progress">Progress reporter for batch operation updates.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>ValueTask that resolves to a collection of generated meshes in the same order as input structures.</returns>
        public ValueTask<IReadOnlyList<Mesh>> MeshBatchAsync(
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
                throw new ArgumentException("Structures collection cannot be empty.", nameof(structures));
            }

            options.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            // 🚀 PERFORMANCE: For single items, use direct path
            if (structureList.Count == 1)
            {
                return MeshWithProgressAsync(structureList[0], options, progress, cancellationToken)
                    .ContinueWith(mesh => (IReadOnlyList<Mesh>)new[] { mesh }, TaskContinuationOptions.ExecuteSynchronously);
            }

            return new ValueTask<IReadOnlyList<Mesh>>(Task.Run(async () =>
            {
                // 🚀 PERFORMANCE: Analyze batch complexity for optimal strategy
                var totalComplexity = structureList.Sum(s => (int)EstimateComplexity(s));
                var optimalParallelism = Math.Min(
                    maxDegreeOfParallelism == -1 ? Environment.ProcessorCount : maxDegreeOfParallelism,
                    Math.Max(1, totalComplexity / 4) // Adjust based on total work
                );

                // Set up parallel options
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = optimalParallelism
                };

                var results = new Mesh[structureList.Count];
                var completedCount = 0;

                await Task.Run(() =>
                {
                    Parallel.For(0, structureList.Count, parallelOptions, index =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        results[index] = CreateMeshInternal(structureList[index], options);

                        var completed = Interlocked.Increment(ref completedCount);
                        progress?.Report(MeshingProgress.FromCounts("Batch Processing", completed, structureList.Count));
                    });
                }, cancellationToken);

                return (IReadOnlyList<Mesh>)results;
            }, cancellationToken));
        }

        /// <summary>Estimates the computational complexity and memory requirements for a meshing operation.</summary>
        /// <param name="structureDefinition">The structure to analyze.</param>
        /// <param name="options">Meshing options that would be used.</param>
        /// <returns>ValueTask that resolves to complexity estimation.</returns>
        public ValueTask<MeshingComplexityEstimate> EstimateComplexityAsync(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(structureDefinition);
            ArgumentNullException.ThrowIfNull(options);

            // 🚀 PERFORMANCE: Complexity estimation is fast, do it synchronously
            var estimate = EstimateComplexity(structureDefinition);
            return new ValueTask<MeshingComplexityEstimate>(CreateDetailedEstimate(structureDefinition, estimate));
        }

        /// <summary>Gets real-time performance statistics for this mesher instance.</summary>
        /// <returns>Current performance statistics including pool efficiency and operation counts.</returns>
        public ValueTask<PerformanceMonitor.PerformanceStatistics> GetLivePerformanceStatsAsync()
        {
            // 🚀 PERFORMANCE: Stats retrieval is fast, return synchronously
            return new ValueTask<PerformanceMonitor.PerformanceStatistics>(
                PerformanceMonitor.Counters.GetStatistics());
        }

        /// <summary>Fast synchronous complexity estimation for internal use.</summary>
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

        /// <summary>Creates detailed estimate with timing and memory predictions.</summary>
        private static MeshingComplexityEstimate CreateDetailedEstimate(PrismStructureDefinition structure, MeshingComplexity complexity)
        {
            // Estimate based on structure complexity
            var footprintVertices = structure.Footprint.Count;
            var holeVertices = structure.Holes.Sum(h => h.Count);
            var totalVertices = footprintVertices + holeVertices;

            // 🚀 PERFORMANCE: Optimized estimation formulas
            var estimatedQuads = (int)(totalVertices * 1.5 + structure.InternalSurfaces.Count * 10);
            // ✅ FIX: Ensure at least 1 triangle is estimated (cap fallback triangles always possible)
            var estimatedTriangles = Math.Max(1, (int)(totalVertices * 0.3));

            // Memory estimation (optimized with object pooling)
            var estimatedMemory = (estimatedQuads + estimatedTriangles) * 160L; // ~160 bytes per element with pooling

            // Time estimation based on current benchmarks (updated for optimizations)
            var estimatedTime = complexity switch
            {
                MeshingComplexity.Trivial => TimeSpan.FromMicroseconds(80),    // 20% faster
                MeshingComplexity.Simple => TimeSpan.FromMicroseconds(240),    // 20% faster
                MeshingComplexity.Moderate => TimeSpan.FromMicroseconds(800),  // 20% faster
                MeshingComplexity.Complex => TimeSpan.FromMilliseconds(4),     // 20% faster
                MeshingComplexity.Extreme => TimeSpan.FromMilliseconds(16),    // 20% faster
                _ => TimeSpan.FromMicroseconds(800)
            };

            var recommendedParallelism = complexity >= MeshingComplexity.Complex ?
                Math.Min(Environment.ProcessorCount, 4) : 1;

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
            // ✅ FIX: Add hints for moderate complexity with holes or async benefits
            if (complexity == MeshingComplexity.Moderate && structure.Holes.Count > 0)
            {
                hints.Add("Moderate complexity with holes - consider async processing for better performance");
            }
            if (complexity >= MeshingComplexity.Moderate && complexity < MeshingComplexity.Complex && totalVertices > 50)
            {
                hints.Add("Consider async processing for improved responsiveness");
            }

            return new MeshingComplexityEstimate(
                estimatedQuads,
                estimatedTriangles,
                estimatedMemory,
                estimatedTime,
                recommendedParallelism,
                complexity,
                hints);
        }

        private Mesh CreateMeshInternal(PrismStructureDefinition structure, MesherOptions options)
        {
            // 🚀 PERFORMANCE: Track metrics and use Activity for monitoring
            using var activity = PerformanceMonitor.StartMeshingActivity("SyncMeshing", new
            {
                VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count),
                HoleCount = structure.Holes.Count,
                ConstraintCount = structure.ConstraintSegments.Count
            });

            PerformanceMonitor.Counters.IncrementMeshingOperations();

            var mesh = new Mesh();
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;
            var zLevels = MeshStructureHelper.BuildZLevels(z0, z1, options, structure);

            // Side faces (outer + holes)
            foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true))
            {
                mesh.AddQuad(q);
            }
            foreach (var hole in structure.Holes)
            {
                foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false))
                {
                    mesh.AddQuad(q);
                }
            }

            // Caps via strategy
            if (options.GenerateBottomCap || options.GenerateTopCap)
            {
                _capStrategy.GenerateCaps(mesh, structure, options, z0, z1);
            }

            // Auxiliary geometry
            foreach (var p in structure.Geometry.Points)
            {
                mesh.AddPoint(p);
            }
            foreach (var s in structure.Geometry.Segments)
            {
                mesh.AddInternalSegment(s);
            }

            // 🚀 PERFORMANCE: Update counters
            PerformanceMonitor.Counters.AddQuadsGenerated(mesh.QuadCount);
            PerformanceMonitor.Counters.AddTrianglesGenerated(mesh.TriangleCount);

            return mesh;
        }

        private Mesh CreateMeshInternalWithProgress(
            PrismStructureDefinition structure,
            MesherOptions options,
            IProgress<MeshingProgress>? progress,
            CancellationToken cancellationToken)
        {
            // 🚀 PERFORMANCE: Use activity tracking and metrics
            using var activity = PerformanceMonitor.StartMeshingActivity("AsyncMeshingWithProgress", new
            {
                VertexCount = structure.Footprint.Count + structure.Holes.Sum(h => h.Count),
                HoleCount = structure.Holes.Count,
                EstimatedComplexity = EstimateComplexity(structure).ToString()
            });

            PerformanceMonitor.Counters.IncrementMeshingOperations();

            var mesh = new Mesh();
            double z0 = structure.BaseElevation;
            double z1 = structure.TopElevation;

            // Report initial progress and check cancellation
            progress?.Report(new MeshingProgress("Initializing", 0.0, 0, 1, statusMessage: "Analyzing structure"));
            cancellationToken.ThrowIfCancellationRequested();

            var zLevels = MeshStructureHelper.BuildZLevels(z0, z1, options, structure);

            // Side faces with progress reporting and frequent cancellation checks
            progress?.Report(new MeshingProgress("Side Faces", 0.1, 0, 1, statusMessage: "Generating side quads"));
            cancellationToken.ThrowIfCancellationRequested();

            // 🔧 FIX: More frequent cancellation checks and proper handling
            var sideQuadCount = 0;
            var totalExpectedQuads = EstimateQuadCount(structure, options);

            foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(structure.Footprint.Vertices, zLevels, options, outward: true))
            {
                // ⚠️ CRITICAL: Always check cancellation before adding quads
                cancellationToken.ThrowIfCancellationRequested();

                mesh.AddQuad(q);
                sideQuadCount++;

                // Report progress for side faces
                if (sideQuadCount % 10 == 0) // Every 10 quads
                {
                    var sideProgress = Math.Min(0.5, (double)sideQuadCount / totalExpectedQuads * 0.5);
                    progress?.Report(new MeshingProgress("Side Faces", 0.1 + sideProgress, sideQuadCount, totalExpectedQuads,
                        statusMessage: $"Generated {sideQuadCount} side quads"));
                }
            }

            foreach (var hole in structure.Holes)
            {
                foreach (var q in SideFaceMeshingHelper.GenerateSideQuads(hole.Vertices, zLevels, options, outward: false))
                {
                    // ⚠️ CRITICAL: Check cancellation for hole processing too
                    cancellationToken.ThrowIfCancellationRequested();

                    mesh.AddQuad(q);
                    sideQuadCount++;

                    if (sideQuadCount % 10 == 0)
                    {
                        var sideProgress = Math.Min(0.5, (double)sideQuadCount / totalExpectedQuads * 0.5);
                        progress?.Report(new MeshingProgress("Side Faces", 0.1 + sideProgress, sideQuadCount, totalExpectedQuads,
                            statusMessage: $"Generated {sideQuadCount} quads (including holes)"));
                    }
                }
            }

            // Caps with progress reporting
            if (options.GenerateBottomCap || options.GenerateTopCap)
            {
                progress?.Report(new MeshingProgress("Caps", 0.6, 0, 1, statusMessage: "Generating caps"));
                cancellationToken.ThrowIfCancellationRequested();

                _capStrategy.GenerateCaps(mesh, structure, options, z0, z1);

                // ⚠️ CRITICAL: Check cancellation after caps generation
                cancellationToken.ThrowIfCancellationRequested();
            }

            // Auxiliary geometry
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

            // 🚀 PERFORMANCE: Update metrics
            PerformanceMonitor.Counters.AddQuadsGenerated(mesh.QuadCount);
            PerformanceMonitor.Counters.AddTrianglesGenerated(mesh.TriangleCount);

            // Final progress report
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(MeshingProgress.Completed("Meshing", mesh.QuadCount + mesh.TriangleCount));

            return mesh;
        }

        /// <summary>Estimates the number of quads for progress reporting.</summary>
        private static int EstimateQuadCount(PrismStructureDefinition structure, MesherOptions options)
        {
            var perimeterLength = structure.Footprint.Count * 4.0; // Rough perimeter
            var height = structure.TopElevation - structure.BaseElevation;
            var zSegments = Math.Max(1, (int)(height / options.TargetEdgeLengthZ));
            var xySegments = Math.Max(1, (int)(perimeterLength / options.TargetEdgeLengthXY));

            // Side quads + estimated cap quads
            return xySegments * zSegments + structure.Footprint.Count * 2;
        }
    }
}
