using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Infrastructure
{
    /// <summary>
    /// Optimized constants and lookup tables using .NET 8 frozen collections.
    /// These collections provide faster read operations for immutable data.
    /// </summary>
    public static class OptimizedConstants
    {
        /// <summary>
        /// Frozen dictionary for common edge length validations.
        /// Uses .NET 8 FrozenDictionary for optimal read performance.
        /// </summary>
        public static readonly FrozenDictionary<string, (double Min, double Max)> EdgeLengthLimits =
            new Dictionary<string, (double, double)>
            {
                ["XY"] = (1e-6, 1e6),
                ["Z"] = (1e-6, 1e6),
                ["HoleRefinement"] = (1e-6, 1e4),
                ["SegmentRefinement"] = (1e-6, 1e4)
            }.ToFrozenDictionary();

        /// <summary>
        /// Frozen set of critical analyzer rules for .NET 8 performance.
        /// </summary>
        public static readonly FrozenSet<string> CriticalPerformanceRules =
            new HashSet<string>
            {
                "CA1859", // Use concrete types for better performance
                "CA1860", // Avoid using Enumerable.Any() for collection counts
                "CA1861", // Prefer static readonly arrays
                "CA1825", // Avoid unnecessary zero-length array allocations
                "CA1826", // Use property instead of LINQ method when possible
                "CA1827", // Do not use Count()/LongCount() when Any() can be used
                "CA1828", // Do not use CountAsync()/LongCountAsync() when AnyAsync() can be used
                "CA1829"  // Use Length/Count property instead of Enumerable.Count()
            }.ToFrozenSet();

        /// <summary>
        /// Optimized quality thresholds using frozen dictionary for constant-time lookup.
        /// </summary>
        public static readonly FrozenDictionary<string, double> QualityThresholds =
            new Dictionary<string, double>
            {
                ["MinCapQuad"] = 0.3,
                ["PreferredCapQuad"] = 0.7,
                ["ExcellentCapQuad"] = 0.9,
                ["MinTriangleAspect"] = 0.1,
                ["PreferredTriangleAspect"] = 0.5
            }.ToFrozenDictionary();

        /// <summary>
        /// Fast lookup for geometric tolerance validation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidEdgeLength(string category, double value)
        {
            return EdgeLengthLimits.TryGetValue(category, out var limits) &&
                   value >= limits.Min && value <= limits.Max;
        }

        /// <summary>
        /// Fast lookup for quality threshold validation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MeetsQualityThreshold(string category, double value)
        {
            return QualityThresholds.TryGetValue(category, out var threshold) &&
                   value >= threshold;
        }
    }
}
