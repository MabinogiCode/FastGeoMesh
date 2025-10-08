using FastGeoMesh.Domain.Services;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Infrastructure.Services
{
    /// <summary>
    /// Implementation of performance monitoring service.
    /// </summary>
    public class PerformanceMonitorService : IPerformanceMonitor
    {
        /// <summary>Gets current performance statistics.</summary>
        public PerformanceStatistics GetLiveStatistics()
        {
            var stats = PerformanceMonitor.Counters.GetStatistics();
            return new PerformanceStatistics
            {
                MeshingOperations = stats.MeshingOperations,
                QuadsGenerated = stats.QuadsGenerated,
                TrianglesGenerated = stats.TrianglesGenerated,
                PoolHitRate = stats.PoolHitRate
            };
        }
    }
}
