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
}
