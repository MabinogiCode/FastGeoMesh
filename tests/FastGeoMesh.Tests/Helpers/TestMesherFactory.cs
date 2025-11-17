using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain.Services;
using FastGeoMesh.Infrastructure.Services;

namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Factory helper for creating PrismMesher instances in tests.
    /// Provides easy access to properly configured meshers without DI container setup.
    /// </summary>
    internal static class TestMesherFactory
    {
        /// <summary>Creates a PrismMesher with all default dependencies.</summary>
        public static PrismMesher CreatePrismMesher()
        {
            var geometryService = new GeometryService();
            var zLevelBuilder = new ZLevelBuilder();
            var proximityChecker = new ProximityChecker();
            return new PrismMesher(geometryService, zLevelBuilder, proximityChecker);
        }

        /// <summary>Creates a GeometryService for tests.</summary>
        public static IGeometryService CreateGeometryService() => new GeometryService();
    }
}
