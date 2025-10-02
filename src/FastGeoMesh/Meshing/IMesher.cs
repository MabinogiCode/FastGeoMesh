using FastGeoMesh.Structures;

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
        /// <summary>Creates a mesh synchronously from the provided structure and options.</summary>
        /// <param name="input">Structure definition to mesh.</param>
        /// <param name="options">Meshing configuration options.</param>
        /// <returns>Generated mesh containing quads and triangles.</returns>
        Mesh Mesh(TStructure input, MesherOptions options);

        /// <summary>Creates a mesh asynchronously with cancellation support.</summary>
        /// <param name="input">Structure definition to mesh.</param>
        /// <param name="options">Meshing configuration options.</param>
        /// <param name="cancellationToken">Token to cancel the meshing operation.</param>
        /// <returns>ValueTask for better performance in synchronous completion scenarios.</returns>
        ValueTask<Mesh> MeshAsync(TStructure input, MesherOptions options, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Specialized mesher interface for prism structure definitions.
    /// Inherits from the generic interface for consistency.
    /// </summary>
    public interface IPrismMesher : IMesher<PrismStructureDefinition>
    {
        // Interface marker - all methods inherited from IMesher<PrismStructureDefinition>
    }
}
