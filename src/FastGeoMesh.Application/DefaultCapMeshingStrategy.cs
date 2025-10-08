using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;

namespace FastGeoMesh.Application
{
    /// <summary>Default cap meshing strategy delegating to CapMeshingHelper.</summary>
    internal sealed class DefaultCapMeshingStrategy : ICapMeshingStrategy
    {
        public CapGeometry GenerateCaps(PrismStructureDefinition definition, MesherOptions options, double z0, double z1)
        {
            // Create a temporary empty mesh and generate caps
            var tempMesh = CapMeshingHelper.GenerateCaps(ImmutableMesh.Empty, definition, options, z0, z1);

            // Extract the generated quads and triangles
            return new CapGeometry(tempMesh.Quads, tempMesh.Triangles);
        }
    }
}
