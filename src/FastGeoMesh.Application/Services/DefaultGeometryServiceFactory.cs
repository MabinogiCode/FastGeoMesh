using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>
    /// Internal factory for creating default geometry service instances.
    /// NOTE: This is a temporary bridge to maintain backward compatibility while migrating to Clean Architecture.
    /// In production, IGeometryService should be injected via a DI container, not created here.
    /// </summary>
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
