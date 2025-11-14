namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Interface for spatial acceleration structures that enable fast point-in-polygon queries.
    /// </summary>
    public interface ISpatialPolygonIndex
    {
        /// <summary>
        /// Checks if a point is inside the indexed polygon.
        /// </summary>
        /// <param name="x">X-coordinate of the point.</param>
        /// <param name="y">Y-coordinate of the point.</param>
        /// <returns>True if the point is inside the polygon, false otherwise.</returns>
        bool IsInside(double x, double y);
    }
}
