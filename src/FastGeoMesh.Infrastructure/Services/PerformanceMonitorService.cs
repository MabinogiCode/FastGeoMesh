using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Infrastructure.Services
{
    /// <summary>
    /// Infrastructure implementation of performance monitoring.
    /// Wraps the static PerformanceMonitor to provide dependency injection support.
    /// </summary>
    public sealed class PerformanceMonitorService : IPerformanceMonitor
    {
        /// <summary>Gets current performance statistics.</summary>
        public FastGeoMesh.Domain.Services.PerformanceStatistics GetLiveStatistics()
        {
            var infraStats = PerformanceMonitorCounters.GetStatistics();
            return new FastGeoMesh.Domain.Services.PerformanceStatistics
            {
                MeshingOperations = infraStats.MeshingOperations,
                QuadsGenerated = infraStats.QuadsGenerated,
                TrianglesGenerated = infraStats.TrianglesGenerated,
                PoolHitRate = infraStats.PoolHitRate
            };
        }

        /// <summary>
        /// Starts a new meshing activity for performance tracking.
        /// </summary>
        public IDisposable StartMeshingActivity(string activityName, object? metadata = null)
        {
            var activity = PerformanceMonitor.StartMeshingActivity(activityName, metadata);
            return new ActivityDisposable(activity);
        }

        /// <summary>
        /// Increments the meshing operations counter.
        /// </summary>
        public void IncrementMeshingOperations()
        {
            PerformanceMonitorCounters.IncrementMeshingOperations();
        }

        /// <summary>
        /// Adds to the generated quads counter.
        /// </summary>
        public void AddQuadsGenerated(int count)
        {
            PerformanceMonitorCounters.AddQuadsGenerated(count);
        }

        /// <summary>
        /// Adds to the generated triangles counter.
        /// </summary>
        public void AddTrianglesGenerated(int count)
        {
            PerformanceMonitorCounters.AddTrianglesGenerated(count);
        }

        /// <summary>
        /// Wrapper to make Activity disposable for proper resource management.
        /// </summary>
        private sealed class ActivityDisposable : IDisposable
        {
            private readonly System.Diagnostics.Activity? _activity;

            public ActivityDisposable(System.Diagnostics.Activity? activity)
            {
                _activity = activity;
            }

            public void Dispose()
            {
                _activity?.Dispose();
            }
        }
    }
}
