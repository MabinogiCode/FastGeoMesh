namespace FastGeoMesh.Domain.Validators;
/// <summary>
/// Static helper class for polygon validation operations.
/// </summary>
public static class PolygonValidator
{
    /// <summary>
    /// Calculates the signed area of a polygon (positive if CCW).
    /// </summary>
    /// <param name="verts">The polygon vertices.</param>
    /// <returns>The signed area.</returns>
    public static double SignedArea(IReadOnlyList<Vec2> verts)
    {
        ArgumentNullException.ThrowIfNull(verts);
        double a = 0;
        for (int i = 0, j = verts.Count - 1; i < verts.Count; j = i++)
        {
            a += (verts[j].X * verts[i].Y) - (verts[i].X * verts[j].Y);
        }
        return 0.5 * a;
    }

    /// <summary>
    /// Validates a polygon (non-degenerate, simple).
    /// </summary>
    /// <param name="verts">The polygon vertices to validate.</param>
    /// <param name="error">The error message if validation fails.</param>
    /// <param name="eps">The epsilon tolerance for comparisons.</param>
    /// <returns>True if the polygon is valid; otherwise, false.</returns>
    public static bool Validate(IReadOnlyList<Vec2> verts, out string? error, double eps = 1e-9)
    {
        ArgumentNullException.ThrowIfNull(verts);
        error = null;
        int n = verts.Count;
        if (n < 3)
        {
            error = "Less than 3 vertices";
            return false;
        }
        if (Math.Abs(SignedArea(verts)) < eps)
        {
            error = "Degenerate area (collinear vertices)";
            return false;
        }
        for (int i = 0; i < n; i++)
        {
            var a = verts[i];
            var b = verts[(i + 1) % n];
            if ((b - a).Length() < eps)
            {
                error = $"Zero-length edge at index {i}";
                return false;
            }
            for (int j = i + 1; j < n; j++)
            {
                var c = verts[j];
                if ((c - a).Length() < eps)
                {
                    error = $"Duplicate/near-coincident vertices at indices {i} and {j}";
                    return false;
                }
            }
        }
        for (int i = 0; i < n; i++)
        {
            var a1 = verts[i];
            var a2 = verts[(i + 1) % n];
            for (int j = i + 1; j < n; j++)
            {
                if (j == i)
                {
                    continue;
                }
                if ((j == i + 1) || (i == 0 && j == n - 1))
                {
                    continue;
                }
                var b1 = verts[j];
                var b2 = verts[(j + 1) % n];
                if (SegmentsIntersect(a1, a2, b1, b2, eps))
                {
                    error = $"Self-intersection between edges {i}-{(i + 1) % n} and {j}-{(j + 1) % n}";
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Determines the orientation of three points.
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <param name="c">Third point.</param>
    /// <param name="eps">Epsilon tolerance.</param>
    /// <returns>0 if collinear, 1 if clockwise, -1 if counter-clockwise.</returns>
    public static int Orient(in Vec2 a, in Vec2 b, in Vec2 c, double eps)
    {
        double v = (b - a).Cross(c - a);
        if (Math.Abs(v) <= eps)
        {
            return 0;
        }
        return v > 0 ? 1 : -1;
    }

    /// <summary>
    /// Checks if point p lies on the segment ab.
    /// </summary>
    /// <param name="a">First endpoint of segment.</param>
    /// <param name="b">Second endpoint of segment.</param>
    /// <param name="p">Point to check.</param>
    /// <param name="eps">Epsilon tolerance.</param>
    /// <returns>True if p is on segment ab; otherwise, false.</returns>
    internal static bool OnSegment(in Vec2 a, in Vec2 b, in Vec2 p, double eps)
    {
        if (Orient(a, b, p, eps) != 0)
        {
            return false;
        }
        return p.X <= Math.Max(a.X, b.X) + eps && p.X + eps >= Math.Min(a.X, b.X) && p.Y <= Math.Max(a.Y, b.Y) + eps && p.Y + eps >= Math.Min(a.Y, b.Y);
    }

    /// <summary>
    /// Checks if two segments intersect.
    /// </summary>
    /// <param name="p1">First endpoint of first segment.</param>
    /// <param name="q1">Second endpoint of first segment.</param>
    /// <param name="p2">First endpoint of second segment.</param>
    /// <param name="q2">Second endpoint of second segment.</param>
    /// <param name="eps">Epsilon tolerance.</param>
    /// <returns>True if segments intersect; otherwise, false.</returns>
    internal static bool SegmentsIntersect(in Vec2 p1, in Vec2 q1, in Vec2 p2, in Vec2 q2, double eps)
    {
        int o1 = Orient(p1, q1, p2, eps);
        int o2 = Orient(p1, q1, q2, eps);
        int o3 = Orient(p2, q2, p1, eps);
        int o4 = Orient(p2, q2, q1, eps);
        if (o1 != o2 && o3 != o4)
        {
            return true;
        }
        if (o1 == 0 && OnSegment(p1, q1, p2, eps))
        {
            return true;
        }
        if (o2 == 0 && OnSegment(p1, q1, q2, eps))
        {
            return true;
        }
        if (o3 == 0 && OnSegment(p2, q2, p1, eps))
        {
            return true;
        }
        if (o4 == 0 && OnSegment(p2, q2, q1, eps))
        {
            return true;
        }
        return false;
    }
}
