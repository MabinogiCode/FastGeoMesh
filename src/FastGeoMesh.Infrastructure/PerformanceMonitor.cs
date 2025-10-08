using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Infrastructure
{
    /// <summary>
    /// Performance monitoring utilities for FastGeoMesh operations.
    /// Provides metrics collection and monitoring capabilities for .NET 8.
    /// </summary>
    public static class PerformanceMonitor
    {
        /// <summary>Activity source for distributed tracing of meshing operations.</summary>
        public static readonly ActivitySource ActivitySource = new("FastGeoMesh");

        private static readonly DiagnosticSource _diagnosticSource = new DiagnosticListener("FastGeoMesh");

        /// <summary>Performance counters for key meshing operations.</summary>
        public static class Counters
        {
            private static long _meshingOperations;
            private static long _quadsGenerated;
            private static long _trianglesGenerated;
            private static long _poolHits;
            private static long _poolMisses;

            /// <summary>Gets the total number of meshing operations performed.</summary>
            public static long MeshingOperations => _meshingOperations;

            /// <summary>Gets the total number of quads generated across all operations.</summary>
            public static long QuadsGenerated => _quadsGenerated;

            /// <summary>Gets the total number of triangles generated across all operations.</summary>
            public static long TrianglesGenerated => _trianglesGenerated;

            /// <summary>Gets the total number of object pool cache hits.</summary>
            public static long PoolHits => _poolHits;

            /// <summary>Gets the total number of object pool cache misses.</summary>
            public static long PoolMisses => _poolMisses;

            /// <summary>Increments the count of meshing operations.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementMeshingOperations() => Interlocked.Increment(ref _meshingOperations);

            /// <summary>Adds to the count of generated quads.</summary>
            /// <param name="count">Number of quads to add to the total.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddQuadsGenerated(int count) => Interlocked.Add(ref _quadsGenerated, count);

            /// <summary>Adds to the count of generated triangles.</summary>
            /// <param name="count">Number of triangles to add to the total.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddTrianglesGenerated(int count) => Interlocked.Add(ref _trianglesGenerated, count);

            /// <summary>Increments the object pool hit counter.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementPoolHit() => Interlocked.Increment(ref _poolHits);

            /// <summary>Increments the object pool miss counter.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementPoolMiss() => Interlocked.Increment(ref _poolMisses);

            /// <summary>Get current performance statistics.</summary>
            public static PerformanceStatistics GetStatistics()
            {
                return new PerformanceStatistics
                {
                    MeshingOperations = _meshingOperations,
                    QuadsGenerated = _quadsGenerated,
                    TrianglesGenerated = _trianglesGenerated,
                    PoolHitRate = _poolHits + _poolMisses > 0 ? (double)_poolHits / (_poolHits + _poolMisses) : 0.0
                };
            }
        }

        /// <summary>Start a new activity for tracking meshing operations.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Activity? StartMeshingActivity(string operationName, object? tags = null)
        {
            // Try to create activity with ActivitySource first
            var activity = ActivitySource.StartActivity(operationName);

            // If no listener is present, create a manual activity for testing scenarios
            if (activity == null)
            {
                activity = new Activity(operationName);
                activity.Start();
            }

            // Set tags if provided
            if (activity != null && tags != null)
            {
                foreach (var prop in tags.GetType().GetProperties())
                {
                    activity.SetTag(prop.Name, prop.GetValue(tags)?.ToString());
                }
            }

            return activity;
        }

        /// <summary>Record diagnostic event for performance analysis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RecordEvent(string eventName, object? data = null)
        {
            if (_diagnosticSource.IsEnabled(eventName))
            {
                _diagnosticSource.Write(eventName, data);
            }
        }

        /// <summary>Performance statistics snapshot.</summary>
        public readonly struct PerformanceStatistics
        {
            /// <summary>Total number of meshing operations performed.</summary>
            public long MeshingOperations { get; init; }

            /// <summary>Total number of quads generated.</summary>
            public long QuadsGenerated { get; init; }

            /// <summary>Total number of triangles generated.</summary>
            public long TrianglesGenerated { get; init; }

            /// <summary>Object pool hit rate as a percentage (0.0 to 1.0).</summary>
            public double PoolHitRate { get; init; }

            /// <summary>Returns a string representation of the performance statistics.</summary>
            public override string ToString()
            {
                return $"Operations: {MeshingOperations}, Quads: {QuadsGenerated}, Triangles: {TrianglesGenerated}, Pool Hit Rate: {PoolHitRate:P2}";
            }
        }
    }
}
