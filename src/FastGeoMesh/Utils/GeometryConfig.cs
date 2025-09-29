namespace FastGeoMesh.Utils
{
    /// <summary>Configuration for geometric calculations.</summary>
    public static class GeometryConfig
    {
        /// <summary>Default geometric tolerance for calculations.</summary>
        public static double DefaultTolerance { get; set; } = 1e-12;

        /// <summary>Tolerance for convexity tests (allows tiny negative due to floating point noise).</summary>
        public static double ConvexityTolerance { get; set; } = -1e-12;
    }
}
