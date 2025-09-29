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
            
            public static long MeshingOperations => _meshingOperations;
            public static long QuadsGenerated => _quadsGenerated;
            public static long TrianglesGenerated => _trianglesGenerated;
            public static long PoolHits => _poolHits;
            public static long PoolMisses => _poolMisses;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementMeshingOperations() => Interlocked.Increment(ref _meshingOperations);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddQuadsGenerated(int count) => Interlocked.Add(ref _quadsGenerated, count);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddTrianglesGenerated(int count) => Interlocked.Add(ref _trianglesGenerated, count);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementPoolHit() => Interlocked.Increment(ref _poolHits);
            
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
            var activity = ActivitySource.StartActivity(operationName);
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
            public long MeshingOperations { get; init; }
            public long QuadsGenerated { get; init; }
            public long TrianglesGenerated { get; init; }
            public double PoolHitRate { get; init; }
            
            public override string ToString()
            {
                return $"Operations: {MeshingOperations}, Quads: {QuadsGenerated}, Triangles: {TrianglesGenerated}, Pool Hit Rate: {PoolHitRate:P2}";
            }
        }
    }
}