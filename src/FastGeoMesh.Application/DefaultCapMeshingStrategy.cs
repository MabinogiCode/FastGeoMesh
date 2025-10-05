using FastGeoMesh.Domain;
using FastGeoMesh.Meshing;
using FastGeoMesh.Infrastructure;

namespace FastGeoMesh.Meshing
{
    /// <summary>Default cap meshing strategy delegating to CapMeshingHelper.</summary>
    internal sealed class DefaultCapMeshingStrategy : ICapMeshingStrategy
    {
        public void GenerateCaps(Mesh mesh, PrismStructureDefinition definition, MesherOptions options, double z0, double z1)
            => CapMeshingHelper.GenerateCaps(mesh, definition, options, z0, z1);
    }
}
