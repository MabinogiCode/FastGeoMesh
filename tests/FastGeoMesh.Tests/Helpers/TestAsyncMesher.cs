using System.Threading;
using System.Threading.Tasks;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Tests
{
    /// <summary>Test implementation of async mesher interface for performance optimization tests.</summary>
    internal sealed class TestAsyncMesher : IPrismMesher
    {
        /// <summary>
        /// Synchronously meshes the provided structure definition with the supplied options.
        /// </summary>
        public Mesh Mesh(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            return new PrismMesher().Mesh(structureDefinition, options);
        }

        /// <summary>
        /// Asynchronously meshes the provided structure definition simulating async workload.
        /// </summary>
        public async ValueTask<Mesh> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default)
        {
            // Simulate async work
            await Task.Delay(1, cancellationToken);
            return await Task.Run(() => new PrismMesher().Mesh(structureDefinition, options), cancellationToken);
        }
    }
}
