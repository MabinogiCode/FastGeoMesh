namespace FastGeoMesh.Domain.Services
{
    /// <summary>
    /// Service for geometric calculations and polygon operations.
    /// Provides fundamental geometry algorithms used throughout the meshing pipeline.
    /// </summary>
    public interface IGeometryService
    {
        /// <summary>
        /// Computes the distance from a point to a line segment.
        /// </summary>
        /// <param name="p">The point to measure from.</param>
        /// <param name="a">First endpoint of the segment.</param>
        /// <param name="b">Second endpoint of the segment.</param>
        /// <param name="tolerance">Tolerance for degenerate segments (default: 0 uses system default).</param>
        /// <returns>The shortest distance from the point to the segment.</returns>
        double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b, double tolerance = 0);

        /// <summary>
        /// Linear interpolation between two 2D points.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">End point.</param>
        /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
        /// <returns>Interpolated point.</returns>
        Vec2 Lerp(in Vec2 a, in Vec2 b, double t);

        /// <summary>
        /// Linear interpolation between two 3D points.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">End point.</param>
        /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
        /// <returns>Interpolated point.</returns>
        Vec3 Lerp(in Vec3 a, in Vec3 b, double t);

        /// <summary>
        /// Linear interpolation between two scalar values.
        /// </summary>
        /// <param name="a">Start value.</param>
        /// <param name="b">End value.</param>
        /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
        /// <returns>Interpolated value.</returns>
        double LerpScalar(double a, double b, double t);

        /// <summary>
        /// Checks if a quadrilateral defined by four 2D points is convex.
        /// </summary>
        /// <param name="quad">The quadrilateral vertices (a, b, c, d).</param>
        /// <returns>True if the quadrilateral is convex; otherwise, false.</returns>
        bool IsConvex((Vec2 a, Vec2 b, Vec2 c, Vec2 d) quad);

        /// <summary>
        /// Checks if a point is inside a polygon using ray casting algorithm.
        /// Points on the boundary are considered inside.
        /// </summary>
        /// <param name="vertices">Polygon vertices.</param>
        /// <param name="point">Point to test.</param>
        /// <param name="tolerance">Tolerance for boundary detection (default: 0 uses system default).</param>
        /// <returns>True if the point is inside or on the polygon; otherwise, false.</returns>
        bool PointInPolygon(ReadOnlySpan<Vec2> vertices, in Vec2 point, double tolerance = 0);

        /// <summary>
        /// Checks if a point is inside a polygon using ray casting algorithm.
        /// Points on the boundary are considered inside.
        /// </summary>
        /// <param name="vertices">Polygon vertices.</param>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point.</param>
        /// <param name="tolerance">Tolerance for boundary detection (default: 0 uses system default).</param>
        /// <returns>True if the point is inside or on the polygon; otherwise, false.</returns>
        bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0);

        /// <summary>
        /// Performs batch point-in-polygon tests for multiple points.
        /// </summary>
        /// <param name="vertices">Polygon vertices.</param>
        /// <param name="points">Points to test.</param>
        /// <param name="results">Span to store results (must match points length).</param>
        /// <param name="tolerance">Tolerance for boundary detection (default: 0 uses system default).</param>
        void BatchPointInPolygon(ReadOnlySpan<Vec2> vertices, ReadOnlySpan<Vec2> points, Span<bool> results, double tolerance = 0);

        /// <summary>
        /// Computes the area of a polygon using the Shoelace formula.
        /// </summary>
        /// <param name="vertices">Polygon vertices.</param>
        /// <returns>The area of the polygon (always positive).</returns>
        double PolygonArea(ReadOnlySpan<Vec2> vertices);

        /// <summary>
        /// Computes the area of a triangle given three 2D points.
        /// </summary>
        /// <param name="a">First vertex.</param>
        /// <param name="b">Second vertex.</param>
        /// <param name="c">Third vertex.</param>
        /// <returns>Triangle area (always positive).</returns>
        double TriangleArea(in Vec2 a, in Vec2 b, in Vec2 c);

        /// <summary>
        /// Computes the area of a quadrilateral given four 2D points.
        /// </summary>
        /// <param name="quad">Quad vertices (v0, v1, v2, v3).</param>
        /// <returns>Quad area (always positive).</returns>
        double QuadArea((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad);

        /// <summary>
        /// Calculates the distance between two 2D points.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>Distance between the points.</returns>
        double Distance(in Vec2 a, in Vec2 b);

        /// <summary>
        /// Calculates the squared distance between two 2D points.
        /// Faster than Distance() as it avoids the square root.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>Squared distance between the points.</returns>
        double DistanceSquared(in Vec2 a, in Vec2 b);

        /// <summary>
        /// Computes the centroid (center of mass) of a set of 2D points.
        /// </summary>
        /// <param name="points">Points to compute centroid for.</param>
        /// <returns>Centroid point, or Vec2.Zero if no points provided.</returns>
        Vec2 Centroid(ReadOnlySpan<Vec2> points);

        /// <summary>
        /// Normalizes a 2D vector to unit length.
        /// </summary>
        /// <param name="vector">Vector to normalize.</param>
        /// <returns>Normalized vector, or Vec2.Zero if input has zero length.</returns>
        Vec2 Normalize(in Vec2 vector);

        /// <summary>
        /// Clamps a value between minimum and maximum bounds.
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Clamped value.</returns>
        double Clamp(double value, double min, double max);
    }
}
