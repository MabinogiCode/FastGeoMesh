using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Tests.Services;

internal sealed class StubProximityChecker : IProximityChecker
{
    public bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService) => false;
    public bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService) => false;
    public bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y, IGeometryService geometryService) => false;
}
