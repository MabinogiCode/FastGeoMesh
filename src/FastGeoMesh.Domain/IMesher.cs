using FastGeoMesh.Domain;

namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Core meshing interface for converting structure definitions into geometric meshes.
    /// Follows .NET 8 async best practices with ValueTask for better performance.
    /// </summary>
    /// <typeparam name="TStructure">The type of structure definition to mesh.</typeparam>
    public interface IMesher<in TStructure>
        where TStructure : notnull
    {
        Mesh Mesh(TStructure input, MesherOptions options);
        ValueTask<Mesh> MeshAsync(TStructure input, MesherOptions options, CancellationToken cancellationToken = default);
    }

    public interface IPrismMesher : IMesher<PrismStructureDefinition>
    {
    }
}
