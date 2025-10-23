using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Infrastructure {
    /// <summary>
    /// Performance monitoring utilities for FastGeoMesh operations.
    /// Provides metrics collection and monitoring capabilities for .NET 8.
    /// </summary>
    public static partial class PerformanceMonitor {
        /// <summary>Activity source for distributed tracing of meshing operations.</summary>
        public static readonly ActivitySource ActivitySource = new("FastGeoMesh");

        private static readonly DiagnosticSource _diagnosticSource = new DiagnosticListener("FastGeoMesh");

        /// <summary>Start a new activity for tracking meshing operations.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Activity? StartMeshingActivity(string operationName, object? tags = null) {
            // Try to create activity with ActivitySource first
            var activity = ActivitySource.StartActivity(operationName);

            // If no listener is present, create a manual activity for testing scenarios
            if (activity == null) {
                activity = new Activity(operationName);
                activity.Start();
            }

            // Set tags if provided
            if (activity != null && tags != null) {
                foreach (var prop in tags.GetType().GetProperties()) {
                    activity.SetTag(prop.Name, prop.GetValue(tags)?.ToString());
                }
            }

            return activity;
        }

        /// <summary>Record diagnostic event for performance analysis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RecordEvent(string eventName, object? data = null) {
            if (_diagnosticSource.IsEnabled(eventName)) {
                _diagnosticSource.Write(eventName, data);
            }
        }
    }
}
