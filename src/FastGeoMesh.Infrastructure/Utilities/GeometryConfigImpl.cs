namespace FastGeoMesh.Infrastructure.Utilities
{
    /// <summary>Injectable geometry configuration.</summary>
    public sealed class GeometryConfigImpl : IGeometryConfig
    {
        /// <summary>Default geometric tolerance for calculations.</summary>
        public double DefaultTolerance { get; set; } = 1e-9;
        /// <summary>Tolerance for convexity tests (allows tiny negative due to floating point noise).</summary>
        public double ConvexityTolerance { get; set; } = -1e-9;
        /// <summary>Tolerance specifically for point-in-polygon tests (more lenient for practical use).</summary>
        public double PointInPolygonTolerance { get; set; } = 1e-9;

        // Explicit interface getters so values are read-only through the interface
        double IGeometryConfig.DefaultTolerance => DefaultTolerance;
        double IGeometryConfig.ConvexityTolerance => ConvexityTolerance;
        double IGeometryConfig.PointInPolygonTolerance => PointInPolygonTolerance;
    }
}
