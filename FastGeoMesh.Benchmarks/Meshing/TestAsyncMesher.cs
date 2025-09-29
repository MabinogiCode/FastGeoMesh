using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Benchmarks.Meshing;

/// <summary>Test mesher implementation using ValueTask for async benchmarking.</summary>
internal sealed class TestAsyncMesher : IPrismMesher
{
    private readonly PrismMesher _mesher = new();

    public Mesh Mesh(PrismStructureDefinition structureDefinition, MesherOptions options)
    {
        return _mesher.Mesh(structureDefinition, options);
    }

    public async ValueTask<Mesh> MeshAsync(PrismStructureDefinition structureDefinition, MesherOptions options, CancellationToken cancellationToken = default)
    {
        // Simulate small async delay to test async patterns
        await Task.Delay(1, cancellationToken);
        return await Task.Run(() => _mesher.Mesh(structureDefinition, options), cancellationToken);
    }
}
