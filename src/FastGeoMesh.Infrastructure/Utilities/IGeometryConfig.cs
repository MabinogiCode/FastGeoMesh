namespace FastGeoMesh.Infrastructure.Utilities
{
    /// <summary>Configuration values for geometric algorithms. Use DI to obtain an instance.</summary>
    public interface IGeometryConfig
    {
        /// <summary>Default geometric tolerance for calculations.</summary>
        double DefaultTolerance { get; }
        /// <summary>Tolerance for convexity tests (allows tiny negative due to floating point noise).</summary>
        double ConvexityTolerance { get; }
        /// <summary>Tolerance specifically for point-in-polygon tests.</summary>
        double PointInPolygonTolerance { get; }
    }
}
