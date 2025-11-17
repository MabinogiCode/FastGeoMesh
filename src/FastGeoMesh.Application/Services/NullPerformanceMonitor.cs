using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>
    /// Null object implementation of <see cref="IPerformanceMonitor"/> for use when no monitoring is required.
    /// Kept in Application layer as a lightweight default implementation for samples/tests.
    /// </summary>
    public sealed class NullPerformanceMonitor : IPerformanceMonitor
    {
        /// <summary>Returns empty performance statistics.</summary>
        public PerformanceStatistics GetLiveStatistics() => new();

        /// <summary>Returns a no-op disposable.</summary>
        public IDisposable StartMeshingActivity(string activityName, object? metadata = null) => NullDisposable.Instance;

        /// <summary>No-op implementation.</summary>
        public void IncrementMeshingOperations() { }

        /// <summary>No-op implementation.</summary>
        public void AddQuadsGenerated(int count) { }

        /// <summary>No-op implementation.</summary>
        public void AddTrianglesGenerated(int count) { }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
