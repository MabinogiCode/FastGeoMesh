using System;
using System.Collections.Generic;

namespace FastGeoMesh.Meshing
{
    public readonly struct MeshingComplexityEstimate
    {
        public int EstimatedQuadCount { get; }
        public int EstimatedTriangleCount { get; }
        public long EstimatedPeakMemoryBytes { get; }
        public TimeSpan EstimatedComputationTime { get; }
        public int RecommendedParallelism { get; }
        public MeshingComplexity Complexity { get; }
        public IReadOnlyList<string> PerformanceHints { get; }
        public MeshingComplexityEstimate(
            int estimatedQuadCount,
            int estimatedTriangleCount,
            long estimatedPeakMemoryBytes,
            TimeSpan estimatedComputationTime,
            int recommendedParallelism,
            MeshingComplexity complexity,
            IReadOnlyList<string>? performanceHints = null)
        {
            EstimatedQuadCount = estimatedQuadCount;
            EstimatedTriangleCount = estimatedTriangleCount;
            EstimatedPeakMemoryBytes = estimatedPeakMemoryBytes;
            EstimatedComputationTime = estimatedComputationTime;
            RecommendedParallelism = recommendedParallelism;
            Complexity = complexity;
            PerformanceHints = performanceHints ?? Array.Empty<string>();
        }
        public override string ToString()
        {
            var memoryMB = EstimatedPeakMemoryBytes / (1024.0 * 1024.0);
            return $"{Complexity} complexity: ~{EstimatedQuadCount + EstimatedTriangleCount} elements, " +
                   $"~{memoryMB:F1} MB peak, ~{EstimatedComputationTime.TotalMilliseconds:F0}ms";
        }
    }
}
