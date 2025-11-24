using Microsoft.Extensions.DependencyInjection;
using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Tests.Helpers
{
    public static class TestServiceProvider
    {
        public static IServiceProvider CreateDefaultProvider()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            return services.BuildServiceProvider();
        }

        public static IPrismMesher CreatePrismMesher()
        {
            var provider = CreateDefaultProvider();
            return provider.GetRequiredService<IPrismMesher>();
        }

        public static IPrismMesher CreatePrismMesherWithCustomCapStrategy(ICapMeshingStrategy capStrategy)
        {
            var provider = CreateDefaultProvider();
            var geometryService = provider.GetRequiredService<IGeometryService>();
            var zLevelBuilder = provider.GetRequiredService<IZLevelBuilder>();
            var proximityChecker = provider.GetRequiredService<IProximityChecker>();
            var performanceMonitor = provider.GetRequiredService<IPerformanceMonitor>();

            return new PrismMesher(capStrategy, performanceMonitor, geometryService, zLevelBuilder, proximityChecker);
        }

        public static IServiceProvider CreateProvider(Action<IServiceCollection>? customize)
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            customize?.Invoke(services);
            return services.BuildServiceProvider();
        }
    }
}
