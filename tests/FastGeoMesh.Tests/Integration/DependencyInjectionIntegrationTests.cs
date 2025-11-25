using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using FastGeoMesh.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Integration
{
    public class DependencyInjectionIntegrationTests
    {
        [Fact]
        public void AddFastGeoMeshRegistersAllRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();

            // Assert - Verify all services can be resolved
            Assert.NotNull(serviceProvider.GetRequiredService<IGeometryService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IClock>());
            Assert.NotNull(serviceProvider.GetRequiredService<ICapMeshingStrategy>());
            Assert.NotNull(serviceProvider.GetRequiredService<IPerformanceMonitor>());
            Assert.NotNull(serviceProvider.GetRequiredService<IZLevelBuilder>());
            Assert.NotNull(serviceProvider.GetRequiredService<IProximityChecker>());
            Assert.NotNull(serviceProvider.GetRequiredService<IPrismMesher>());
            Assert.NotNull(serviceProvider.GetRequiredService<IAsyncMesher>());
        }

        [Fact]
        public void AddFastGeoMeshUsesSingletonForGeometryService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var service1 = serviceProvider.GetRequiredService<IGeometryService>();
            var service2 = serviceProvider.GetRequiredService<IGeometryService>();

            // Assert - Same instance
            Assert.Same(service1, service2);
        }

        [Fact]
        public void AddFastGeoMeshUsesTransientForPrismMesher()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var mesher1 = serviceProvider.GetRequiredService<IPrismMesher>();
            var mesher2 = serviceProvider.GetRequiredService<IPrismMesher>();

            // Assert - Different instances
            Assert.NotSame(mesher1, mesher2);
        }

        [Fact]
        public void PrismMesherCanMeshWithDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();
            var mesher = serviceProvider.GetRequiredService<IPrismMesher>();

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                0.0,
                10.0
            );

            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build();

            // Act
            var result = mesher.Mesh(structure, optionsResult.Value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.QuadCount > 0);
        }

        [Fact]
        public void AddFastGeoMeshWithMonitoringRegistersPerformanceMonitor()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMeshWithMonitoring();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var monitor = serviceProvider.GetRequiredService<IPerformanceMonitor>();

            // Assert
            Assert.NotNull(monitor);
            Assert.IsType<PerformanceMonitorService>(monitor);
        }

        [Fact]
        public void AddFastGeoMeshWithMonitoringUseSingletonForPerformanceMonitor()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMeshWithMonitoring();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var monitor1 = serviceProvider.GetRequiredService<IPerformanceMonitor>();
            var monitor2 = serviceProvider.GetRequiredService<IPerformanceMonitor>();

            // Assert - Same instance
            Assert.Same(monitor1, monitor2);
        }

        [Fact]
        public void IPrismMesherAndIAsyncMesherResolveToSameImplementation()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var prismMesher = serviceProvider.GetRequiredService<IPrismMesher>();
            var asyncMesher = serviceProvider.GetRequiredService<IAsyncMesher>();

            // Assert - Both resolve to PrismMesher (though different instances due to Transient)
            Assert.IsType<PrismMesher>(prismMesher);
            Assert.IsType<PrismMesher>(asyncMesher);
        }

        [Fact]
        public async Task AsyncMesherCanMeshAsyncWithDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();
            var mesher = serviceProvider.GetRequiredService<IAsyncMesher>();

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                0.0,
                10.0
            );

            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build();

            // Act
            var result = await mesher.MeshAsync(structure, optionsResult.Value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.QuadCount > 0);
        }

        [Fact]
        public void ZLevelBuilderCanBeResolvedAndUsed()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();
            var zLevelBuilder = serviceProvider.GetRequiredService<IZLevelBuilder>();

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                0.0,
                10.0
            );

            var options = MesherOptions.CreateBuilder().Build().Value;

            // Act
            var levels = zLevelBuilder.BuildZLevels(0.0, 10.0, options, structure);

            // Assert
            Assert.NotNull(levels);
            Assert.Contains(0.0, levels);
            Assert.Contains(10.0, levels);
        }

        [Fact]
        public void ProximityCheckerCanBeResolvedAndUsed()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var serviceProvider = services.BuildServiceProvider();
            var proximityChecker = serviceProvider.GetRequiredService<IProximityChecker>();
            var geometryService = serviceProvider.GetRequiredService<IGeometryService>();

            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(2, 2),
                new Vec2(4, 2),
                new Vec2(4, 4),
                new Vec2(2, 4)
            });

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                0.0,
                10.0
            ).AddHole(hole);

            // Act
            var isNear = proximityChecker.IsNearAnyHole(structure, 1.9, 3.0, band: 0.2, geometryService);

            // Assert
            Assert.True(isNear);
        }
    }
}
