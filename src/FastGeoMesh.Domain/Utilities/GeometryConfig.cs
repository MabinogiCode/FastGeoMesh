namespace FastGeoMesh.Domain {
    /// <summary>
    /// Immutable options for geometry algorithms. Use the <see cref="Default"/> instance
    /// or create a custom instance and pass explicit tolerances to API methods that accept them.
    /// </summary>
    public sealed record GeometryOptions(double DefaultTolerance = 1e-9, double ConvexityTolerance = -1e-9, double PointInPolygonTolerance = 1e-9) {
        /// <summary>
        /// Default options instance with recommended tolerances.
        /// </summary>
        public static GeometryOptions Default { get; } = new GeometryOptions();
    }
}
