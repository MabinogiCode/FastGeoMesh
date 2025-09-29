using System.Threading;
using System.Threading.Tasks;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Meshing
{
    /// <summary>Mesher interface for converting a structure definition into a populated mesh.</summary>
    /// <typeparam name="TStructure">Structure definition type.</typeparam>
    public interface IMesher<TStructure>
    {
        /// <summary>Create mesh from the provided structure and options.</summary>
        Mesh Mesh(TStructure input, MesherOptions options);

        /// <summary>Create mesh from the provided structure and options asynchronously with cancellation support.</summary>
        /// <param name="input">Structure definition to mesh.</param>
        /// <param name="options">Meshing options.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>ValueTask representing the meshing operation for better performance.</returns>
        ValueTask<Mesh> MeshAsync(TStructure input, MesherOptions options, CancellationToken cancellationToken = default);
    }

    /// <summary>Interface for meshing algorithms that generate geometric meshes from structure definitions.</summary>
    public interface IMesher
    {
        /// <summary>Generate a mesh from the given structure and options.</summary>
        /// <param name="structureDefinition">The structure definition to mesh.</param>
        /// <param name="options">Meshing parameters and options.</param>
        /// <returns>Generated mesh containing quads and triangles.</returns>
        Mesh Mesh(PrismStructureDefinition structureDefinition, MesherOptions options);

        /// <summary>Asynchronously generate a mesh from the given structure and options.</summary>
        /// <param name="structureDefinition">The structure definition to mesh.</param>
        /// <param name="options">Meshing parameters and options.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Generated mesh containing quads and triangles.</returns>
        Task<Mesh> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default);
    }
}
