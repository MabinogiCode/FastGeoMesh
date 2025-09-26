using FastGeoMesh.Structures;

namespace FastGeoMesh.Meshing;

/// <summary>Generic mesher interface.</summary>
public interface IMesher<TStructure>
{
    /// <summary>Generate a mesh from a structure and mesher options.</summary>
    Mesh Mesh(TStructure input, MesherOptions options);
}
