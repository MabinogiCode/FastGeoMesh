using System.Threading;
using System.Threading.Tasks;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Tests
{
    /// <summary>Test implementation of async mesher interface for performance optimization tests.</summary>
    internal sealed class TestAsyncMesher : IPrismMesher
    {
        public Mesh Mesh(PrismStructureDefinition structureDefinition, MesherOptions options)
        {
            return new PrismMesher().Mesh(structureDefinition, options);
        }

        public async ValueTask<Mesh> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default)
        {
            // Simulate async work
            await Task.Delay(1, cancellationToken);
            return await Task.Run(() => new PrismMesher().Mesh(structureDefinition, options), cancellationToken);
        }
    }
}
