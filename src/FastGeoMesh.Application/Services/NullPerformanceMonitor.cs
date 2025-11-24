using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>
    /// Null object implementation for IPerformanceMonitor (no-op).
    /// </summary>
    public class NullPerformanceMonitor : IPerformanceMonitor
    {
        /// <summary>
        /// Starts a no-op meshing activity.
        /// </summary>
        /// <param name="activityName">The name of the activity.</param>
        /// <param name="metadata">Optional metadata for the activity.</param>
        /// <returns>A dummy disposable object.</returns>
        public IDisposable StartMeshingActivity(string activityName, object? metadata = null)
        {
            return System.Threading.Tasks.Task.CompletedTask as IDisposable ?? new DummyDisposable();
        }

        /// <summary>
        /// No-op increment for meshing operations.
        /// </summary>
        public void IncrementMeshingOperations() { }

        /// <summary>
        /// No-op for adding quads generated.
        /// </summary>
        /// <param name="count">The number of quads generated.</param>
        public void AddQuadsGenerated(int count) { }

        /// <summary>
        /// No-op for adding triangles generated.
        /// </summary>
        /// <param name="count">The number of triangles generated.</param>
        public void AddTrianglesGenerated(int count) { }

        /// <summary>
        /// Returns a default performance statistics object.
        /// </summary>
        /// <returns>A default performance statistics object.</returns>
        public PerformanceStatistics GetLiveStatistics() => new PerformanceStatistics();
    }

    /// <summary>
    /// No-op disposable implementation for NullPerformanceMonitor.
    /// </summary>
    internal sealed class DummyDisposable : IDisposable
    {
        /// <summary>
        /// No-op dispose implementation.
        /// </summary>
        public void Dispose() { }
    }
}
