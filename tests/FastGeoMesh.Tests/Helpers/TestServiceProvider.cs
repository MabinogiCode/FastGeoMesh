using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Tests for class TestServiceProvider.
    /// </summary>
    public static class TestServiceProvider
    {
        /// <summary>
        /// Runs test CreateDefaultProvider.
        /// </summary>
        public static IServiceProvider CreateDefaultProvider()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            return services.BuildServiceProvider();
        }
        /// <summary>
        /// Runs test CreatePrismMesher.
        /// </summary>
        public static IPrismMesher CreatePrismMesher()
        {
            var provider = CreateDefaultProvider();
            return provider.GetRequiredService<IPrismMesher>();
        }
        /// <summary>
        /// Runs test CreatePrismMesherWithCustomCapStrategy.
        /// </summary>
        public static IPrismMesher CreatePrismMesherWithCustomCapStrategy(ICapMeshingStrategy capStrategy)
        {
            var provider = CreateDefaultProvider();
            var geometryService = provider.GetRequiredService<IGeometryService>();
            var zLevelBuilder = provider.GetRequiredService<IZLevelBuilder>();
            var proximityChecker = provider.GetRequiredService<IProximityChecker>();
            var performanceMonitor = provider.GetRequiredService<IPerformanceMonitor>();

            return new PrismMesher(capStrategy, performanceMonitor, geometryService, zLevelBuilder, proximityChecker);
        }
        /// <summary>
        /// Runs test CreateProvider.
        /// </summary>
        public static IServiceProvider CreateProvider(Action<IServiceCollection>? customize)
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            customize?.Invoke(services);
            return services.BuildServiceProvider();
        }
    }
}
