// <copyright file="ServiceCollectionExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FastGeoMesh.Application.Services;
using FastGeoMesh.Application.Strategies;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Domain.Services;
using FastGeoMesh.Infrastructure.Services;
using FastGeoMesh.Infrastructure.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FastGeoMesh
{
    /// <summary>
    /// Extension methods for configuring FastGeoMesh services in the DI container.
    /// This is the Composition Root - the only place where concrete implementations are wired to interfaces.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all FastGeoMesh services to the DI container.
        /// This is the proper Clean Architecture approach for dependency injection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddFastGeoMesh(this IServiceCollection services)
        {
            // Configuration
            services.AddSingleton<IGeometryConfig, GeometryConfigImpl>();

            // Geometry helpers / service - consolidate to single implementation
            services.AddSingleton<IGeometryService, GeometryService>();

            // Map the domain-level IGeometryHelper to the same GeometryService instance
            services.AddSingleton<IGeometryHelper>(sp => (IGeometryHelper)sp.GetRequiredService<IGeometryService>());

            // Domain Services (implemented in Infrastructure)
            services.AddSingleton<IClock, SystemClock>();

            // Application Services
            services.AddTransient<ICapMeshingStrategy, DefaultCapMeshingStrategy>();
            services.AddTransient<IPerformanceMonitor, NullPerformanceMonitor>(); // Default: no monitoring
            services.AddTransient<IZLevelBuilder, ZLevelBuilder>();
            services.AddTransient<IProximityChecker, ProximityChecker>();

            // Main mesher - uses all injected dependencies
            services.AddTransient<IAsyncMesher, PrismMesher>();
            services.AddTransient<IPrismMesher, PrismMesher>();

            return services;
        }

        /// <summary>
        /// Adds FastGeoMesh services with performance monitoring enabled.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddFastGeoMeshWithMonitoring(this IServiceCollection services)
        {
            // Configuration
            services.AddSingleton<IGeometryConfig, GeometryConfigImpl>();

            // Geometry helpers / service - consolidate to single implementation
            services.AddSingleton<IGeometryService, GeometryService>();
            services.AddSingleton<IGeometryHelper>(sp => (IGeometryHelper)sp.GetRequiredService<IGeometryService>());

            // Domain Services
            services.AddSingleton<IClock, SystemClock>();

            // Application Services with monitoring
            services.AddTransient<ICapMeshingStrategy, DefaultCapMeshingStrategy>();
            services.AddSingleton<IPerformanceMonitor, PerformanceMonitorService>(); // Real monitoring
            services.AddTransient<IZLevelBuilder, ZLevelBuilder>();
            services.AddTransient<IProximityChecker, ProximityChecker>();

            // Main mesher
            services.AddTransient<IAsyncMesher, PrismMesher>();
            services.AddTransient<IPrismMesher, PrismMesher>();

            return services;
        }
    }
}
