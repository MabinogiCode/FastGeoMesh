namespace FastGeoMesh.Domain.Utilities;
/// <summary>
/// Canonical static utility class for fundamental geometric calculations.
/// All geometric operations should use these methods to ensure consistency across the codebase.
/// </summary>
public static class GeometryUtilities
{
    /// <summary>
    /// Calculates the minimum distance from a point to a line segment.
    /// Uses projection onto the segment, clamped to the endpoints.
    /// </summary>
    /// <param name="point">The point to measure distance from.</param>
    /// <param name="segmentStart">Start point of the segment.</param>
    /// <param name="segmentEnd">End point of the segment.</param>
    /// <param name="tolerance">Tolerance for degenerate segment detection (default: 1e-12).</param>
    /// <returns>The minimum distance from the point to the segment.</returns>
    public static double DistancePointToSegment(in Vec2 point, in Vec2 segmentStart, in Vec2 segmentEnd, double tolerance = 1e-12)
    {
        var segmentVector = segmentEnd - segmentStart;
        var pointVector = point - segmentStart;

        // Check for degenerate segment (start and end are essentially the same point)
        double segmentLengthSquared = segmentVector.Dot(segmentVector);
        if (segmentLengthSquared <= tolerance)
        {
            // Degenerate segment - treat as a point
            return (point - segmentStart).Length();
        }

        // Project point onto the line containing the segment
        // t represents the position along the segment (0 = start, 1 = end)
        double t = pointVector.Dot(segmentVector) / segmentLengthSquared;

        // Clamp t to [0, 1] to stay within the segment
        t = Math.Clamp(t, 0.0, 1.0);

        // Calculate the closest point on the segment
        var closestPoint = new Vec2(
            segmentStart.X + segmentVector.X * t,
            segmentStart.Y + segmentVector.Y * t
        );

        // Return distance from point to closest point on segment
        return (point - closestPoint).Length();
    }

    /// <summary>
    /// Calculates the signed area of a triangle given three vertices.
    /// Positive if vertices are in counter-clockwise order, negative if clockwise.
    /// </summary>
    /// <param name="a">First vertex.</param>
    /// <param name="b">Second vertex.</param>
    /// <param name="c">Third vertex.</param>
    /// <returns>The signed area of the triangle.</returns>
    public static double TriangleSignedArea(in Vec2 a, in Vec2 b, in Vec2 c)
    {
        // Using the cross product formula: 0.5 * ((b-a) × (c-a))
        return 0.5 * ((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y));
    }

    /// <summary>
    /// Calculates the absolute area of a triangle given three vertices.
    /// </summary>
    /// <param name="a">First vertex.</param>
    /// <param name="b">Second vertex.</param>
    /// <param name="c">Third vertex.</param>
    /// <returns>The absolute area of the triangle (always positive).</returns>
    public static double TriangleArea(in Vec2 a, in Vec2 b, in Vec2 c)
    {
        return Math.Abs(TriangleSignedArea(a, b, c));
    }

    /// <summary>
    /// Calculates the area of a quadrilateral by splitting it into two triangles.
    /// </summary>
    /// <param name="quad">The quad vertices (v0, v1, v2, v3) in order.</param>
    /// <returns>The area of the quadrilateral.</returns>
    public static double QuadArea((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
    {
        // Split quad into two triangles: (v0, v1, v2) and (v0, v2, v3)
        return TriangleArea(quad.v0, quad.v1, quad.v2) + TriangleArea(quad.v0, quad.v2, quad.v3);
    }

    /// <summary>
    /// Linear interpolation between two 2D points.
    /// </summary>
    /// <param name="a">Start point.</param>
    /// <param name="b">End point.</param>
    /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
    /// <returns>Interpolated point.</returns>
    public static Vec2 Lerp(in Vec2 a, in Vec2 b, double t)
    {
        return new Vec2(
            a.X + t * (b.X - a.X),
            a.Y + t * (b.Y - a.Y)
        );
    }

    /// <summary>
    /// Linear interpolation between two 3D points.
    /// </summary>
    /// <param name="a">Start point.</param>
    /// <param name="b">End point.</param>
    /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
    /// <returns>Interpolated point.</returns>
    public static Vec3 Lerp(in Vec3 a, in Vec3 b, double t)
    {
        return new Vec3(
            a.X + t * (b.X - a.X),
            a.Y + t * (b.Y - a.Y),
            a.Z + t * (b.Z - a.Z)
        );
    }

    /// <summary>
    /// Linear interpolation between two scalar values.
    /// </summary>
    /// <param name="a">Start value.</param>
    /// <param name="b">End value.</param>
    /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
    /// <returns>Interpolated value.</returns>
    public static double LerpScalar(double a, double b, double t)
    {
        return a + (b - a) * t;
    }
}
