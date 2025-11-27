using FastGeoMesh.Application.Helpers.Meshing;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Strategies
{
    /// <summary>Default cap meshing strategy with optimized path for axis-aligned rectangles.</summary>
    public sealed class DefaultCapMeshingStrategy : ICapMeshingStrategy
    {
        private readonly IGeometryService _geometryService;

        /// <summary>Create a new default cap meshing strategy.</summary>
        public DefaultCapMeshingStrategy(IGeometryService geometryService)
        {
            _geometryService = geometryService ?? throw new ArgumentNullException(nameof(geometryService));
        }

        /// <summary>Generate cap geometry for the given structure and options.</summary>
        public CapGeometry GenerateCaps(PrismStructureDefinition structure, MesherOptions options, double z0, double z1)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(options);

            // Use the helper to generate caps
            var mesh = ImmutableMesh.Empty;
            mesh = CapMeshingHelper.GenerateCaps(mesh, structure, options, z0, z1, _geometryService);

            // Convert mesh to CapGeometry
            return new CapGeometry(mesh.Quads.ToList(), mesh.Triangles.ToList());
        }
    }
}
