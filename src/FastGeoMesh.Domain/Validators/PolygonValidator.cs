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
}
