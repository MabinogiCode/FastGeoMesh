using FastGeoMesh.Structures;

namespace FastGeoMesh.Meshing;

public interface IMesher<TStructure>
{
    Mesh Mesh(TStructure input, MesherOptions options);
}
