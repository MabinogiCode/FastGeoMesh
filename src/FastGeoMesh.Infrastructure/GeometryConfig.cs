namespace FastGeoMesh.Utils
{
    /// <summary>Configuration for geometric calculations.</summary>
    public static class GeometryConfig
    {
        /// <summary>Default geometric tolerance for calculations.</summary>
        public static double DefaultTolerance { get; set; } = 1e-9;

        /// <summary>Tolerance for convexity tests (allows tiny negative due to floating point noise).</summary>
        public static double ConvexityTolerance { get; set; } = -1e-9;

        /// <summary>Tolerance specifically for point-in-polygon tests (more lenient for practical use).</summary>
        public static double PointInPolygonTolerance { get; set; } = 1e-9;
    }
}
