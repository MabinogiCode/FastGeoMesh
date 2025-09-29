using FastGeoMesh.Structures;

namespace FastGeoMesh.Meshing
{
    /// <summary>Strategy abstraction for generating top/bottom caps.</summary>
    public interface ICapMeshingStrategy
    {
        /// <summary>Generate cap faces (quads + optional triangles) into given mesh.</summary>
        void GenerateCaps(Mesh mesh, PrismStructureDefinition definition, MesherOptions options, double z0, double z1);
    }
}
