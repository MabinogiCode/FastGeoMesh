using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Utils
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

            /// <inheritdoc/>
            public static long MeshingOperations => _meshingOperations;
            /// <inheritdoc/>
            public static long QuadsGenerated => _quadsGenerated;
            /// <inheritdoc/>
            public static long TrianglesGenerated => _trianglesGenerated;
            /// <inheritdoc/>
            public static long PoolHits => _poolHits;
            /// <inheritdoc/>
            public static long PoolMisses => _poolMisses;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementMeshingOperations() => Interlocked.Increment(ref _meshingOperations);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddQuadsGenerated(int count) => Interlocked.Add(ref _quadsGenerated, count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddTrianglesGenerated(int count) => Interlocked.Add(ref _trianglesGenerated, count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementPoolHit() => Interlocked.Increment(ref _poolHits);

            /// <inheritdoc/>
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
            /// <inheritdoc/>
            public long MeshingOperations { get; init; }
            /// <inheritdoc/>
            public long QuadsGenerated { get; init; }
            /// <inheritdoc/>
            public long TrianglesGenerated { get; init; }
            /// <inheritdoc/>
            public double PoolHitRate { get; init; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"Operations: {MeshingOperations}, Quads: {QuadsGenerated}, Triangles: {TrianglesGenerated}, Pool Hit Rate: {PoolHitRate:P2}";
            }
        }
    }
}
