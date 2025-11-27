using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain.Services;
using FastGeoMesh.Infrastructure.Services;

namespace FastGeoMesh.Tests.Helpers
{
    internal static class TestMesherFactory
    {
        /// <summary>
        /// Runs test CreatePrismMesher.
        /// </summary>
        public static PrismMesher CreatePrismMesher()
        {
            var geometryService = new GeometryService();
            var zLevelBuilder = new ZLevelBuilder();
            var proximityChecker = new ProximityChecker();
            return new PrismMesher(geometryService, zLevelBuilder, proximityChecker);
        }
        /// <summary>
        /// Runs test CreateGeometryService.
        /// </summary>
        public static IGeometryService CreateGeometryService() => new GeometryService();
    }
}
