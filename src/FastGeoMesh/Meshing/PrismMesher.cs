using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastGeoMesh.Geometry;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using FastGeoMesh.Meshing.Helpers;

namespace FastGeoMesh.Meshing
{
    /// <summary>Prism mesher producing quad-dominant meshes (side quads + cap quads, optional cap triangles).</summary>
    public sealed class PrismMesher : IMesher<PrismStructureDefinition>
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
        public Mesh Mesh(PrismStructureDefinition structure, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();
            
            return CreateMeshInternal(structure, options);
        }

        /// <summary>Generate a mesh asynchronously from the given prism structure definition and meshing options.</summary>
        public ValueTask<Mesh> MeshAsync(PrismStructureDefinition structure, MesherOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();
            
            cancellationToken.ThrowIfCancellationRequested();

            // For CPU-bound operations in library code, use Task.Run to offload to thread pool
            // Return ValueTask wrapping the Task for better performance when caching results
            return new ValueTask<Mesh>(Task.Run(() => CreateMeshInternal(structure, options), cancellationToken));
        }

        private Mesh CreateMeshInternal(PrismStructureDefinition structure, MesherOptions options)
        {
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
            
            return mesh;
        }
    }
}
