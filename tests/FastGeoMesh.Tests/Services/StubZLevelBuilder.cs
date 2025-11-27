using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Tests.Services;

internal sealed class StubZLevelBuilder : IZLevelBuilder
{
    public Func<double, double, MesherOptions, PrismStructureDefinition, IReadOnlyList<double>> BuildZLevelsFunc { get; set; }
        = (z0, z1, opt, struc) => new List<double> { z0, z1 };

    public IReadOnlyList<double> BuildZLevels(double z0, double z1, MesherOptions options, PrismStructureDefinition structure)
        => BuildZLevelsFunc(z0, z1, options, structure);
}
