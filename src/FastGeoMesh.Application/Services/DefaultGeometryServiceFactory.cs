using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>
    /// Internal factory for creating default geometry service instances.
    /// </summary>
    /// <remarks>
    /// ⚠️ DEPRECATED: This is a temporary bridge for backward compatibility.
    ///
    /// RECOMMENDED APPROACH: Use Microsoft.Extensions.DependencyInjection instead:
    /// <code>
    /// services.AddFastGeoMesh();  // In Startup.cs or Program.cs
    /// var mesher = serviceProvider.GetRequiredService&lt;IPrismMesher&gt;();
    /// </code>
    ///
    /// This factory uses reflection to avoid direct Application→Infrastructure dependency,
    /// but it's not the proper Clean Architecture solution. Use DI container in production.
    /// </remarks>
    [Obsolete("Use dependency injection with ServiceCollectionExtensions.AddFastGeoMesh() instead. This factory is only for backward compatibility.", false)]
    internal static class DefaultGeometryServiceFactory
    {
        /// <summary>
        /// Creates a default geometry service instance using reflection to avoid direct dependency on Infrastructure.
        /// </summary>
        public static IGeometryService Create()
        {
            // Use reflection to create Infrastructure.Services.GeometryService without direct reference
            var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "FastGeoMesh.Infrastructure");

            if (infrastructureAssembly == null)
            {
                throw new InvalidOperationException(
                    "FastGeoMesh.Infrastructure assembly not found. " +
                    "Please use constructor overload that accepts IGeometryService parameter.");
            }

            var geometryServiceType = infrastructureAssembly.GetType("FastGeoMesh.Infrastructure.Services.GeometryService");
            if (geometryServiceType == null)
            {
                throw new InvalidOperationException("GeometryService type not found in Infrastructure assembly.");
            }

            return (IGeometryService)Activator.CreateInstance(geometryServiceType)!;
        }
    }
}
