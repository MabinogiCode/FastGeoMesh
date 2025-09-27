using FastGeoMesh.Structures;

namespace FastGeoMesh.Meshing
{
    /// <summary>Mesher interface for converting a structure definition into a populated mesh.</summary>
    /// <typeparam name="TStructure">Structure definition type.</typeparam>
    public interface IMesher<TStructure>
    {
        /// <summary>Create mesh from the provided structure and options.</summary>
        Mesh Mesh(TStructure input, MesherOptions options);
    }
}
