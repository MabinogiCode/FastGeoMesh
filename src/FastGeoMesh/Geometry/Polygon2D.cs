using System.Collections.Generic;
using System.Linq;

namespace FastGeoMesh.Geometry;

/// <summary>Simple polygon in CCW order (no self-intersections).</summary>
public sealed class Polygon2D
{
    public IReadOnlyList<Vec2> Vertices { get; }
    public int Count => Vertices.Count;

    public Polygon2D(IEnumerable<Vec2> verts)
    {
        var list = verts.ToList();
        if (list.Count < 3) throw new ArgumentException("Polygon must have at least 3 vertices.");
        // Ensure CCW
        if (SignedArea(list) < 0) list.Reverse();

        // Validate geometry (basic robustness)
        if (!Validate(list, out var error))
            throw new ArgumentException($"Invalid polygon: {error}");

        Vertices = list;
    }

    public static Polygon2D FromPoints(IEnumerable<Vec2> verts) => new(verts);

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

    public double Perimeter()
    {
        double p = 0;
        for (int i = 0; i < Count; i++)
        {
            var a = Vertices[i];
            var b = Vertices[(i + 1) % Count];
            p += (b - a).Length();
        }
        return p;
    }

    public bool IsRectangleAxisAligned(out Vec2 min, out Vec2 max, double eps = 1e-9)
    {
        min = new Vec2(double.PositiveInfinity, double.PositiveInfinity);
        max = new Vec2(double.NegativeInfinity, double.NegativeInfinity);
        if (Count != 4) return false;
        foreach (var v in Vertices)
        {
            if (v.X < min.X) min = new Vec2(v.X, min.Y);
            if (v.Y < min.Y) min = new Vec2(min.X, v.Y);
            if (v.X > max.X) max = new Vec2(v.X, max.Y);
            if (v.Y > max.Y) max = new Vec2(max.X, v.Y);
        }
        // Check each vertex is one of the 4 corners (min/max combos)
        var corners = new HashSet<(double,double)>
        {
            (min.X, min.Y), (min.X, max.Y), (max.X, min.Y), (max.X, max.Y)
        };
        if (Vertices.Any(v => !corners.Contains((v.X, v.Y)))) return false;
        // Edges axis aligned?
        for (int i = 0; i < 4; i++)
        {
            var a = Vertices[i];
            var b = Vertices[(i+1)%4];
            if (Math.Abs(a.X - b.X) > eps && Math.Abs(a.Y - b.Y) > eps) return false;
        }
        return true;
    }

    /// <summary>
    /// Validates that the polygon is non-degenerate and simple (no self-intersections).
    /// Uses a small epsilon for robustness.
    /// </summary>
    public static bool Validate(IReadOnlyList<Vec2> verts, out string? error, double eps = 1e-9)
    {
        ArgumentNullException.ThrowIfNull(verts);
        error = null;
        int n = verts.Count;
        if (n < 3) { error = "Less than 3 vertices"; return false; }

        // Area must be non-zero
        if (Math.Abs(SignedArea(verts)) < eps)
        {
            error = "Degenerate area (collinear vertices)"; return false;
        }

        // No duplicate vertices and no zero-length edges
        for (int i = 0; i < n; i++)
        {
            var a = verts[i];
            var b = verts[(i + 1) % n];
            if ((b - a).Length() < eps)
            {
                error = $"Zero-length edge at index {i}"; return false;
            }
            for (int j = i + 1; j < n; j++)
            {
                var c = verts[j];
                if ((c - a).Length() < eps)
                {
                    error = $"Duplicate/near-coincident vertices at indices {i} and {j}"; return false;
                }
            }
        }

        // Check self-intersections (exclude adjacent edges and shared endpoints)
        for (int i = 0; i < n; i++)
        {
            var a1 = verts[i];
            var a2 = verts[(i + 1) % n];
            for (int j = i + 1; j < n; j++)
            {
                // Skip same edge and adjacent edges
                if (j == i) continue;
                if ((j == i + 1) || (i == 0 && j == n - 1)) continue;
                var b1 = verts[j];
                var b2 = verts[(j + 1) % n];
                if (SegmentsIntersect(a1, a2, b1, b2, eps))
                {
                    error = $"Self-intersection between edges {i}-{(i + 1) % n} and {j}-{(j + 1) % n}"; return false;
                }
            }
        }

        return true;
    }

    private static int Orient(in Vec2 a, in Vec2 b, in Vec2 c, double eps)
    {
        double v = (b - a).Cross(c - a);
        if (Math.Abs(v) <= eps) return 0; // collinear
        return v > 0 ? 1 : -1; // 1 CCW, -1 CW
        }

    private static bool OnSegment(in Vec2 a, in Vec2 b, in Vec2 p, double eps)
    {
        // p collinear with ab and within bounding box
        if (Orient(a, b, p, eps) != 0) return false;
        return p.X <= Math.Max(a.X, b.X) + eps && p.X + eps >= Math.Min(a.X, b.X)
            && p.Y <= Math.Max(a.Y, b.Y) + eps && p.Y + eps >= Math.Min(a.Y, b.Y);
    }

    private static bool SegmentsIntersect(in Vec2 p1, in Vec2 q1, in Vec2 p2, in Vec2 q2, double eps)
    {
        int o1 = Orient(p1, q1, p2, eps);
        int o2 = Orient(p1, q1, q2, eps);
        int o3 = Orient(p2, q2, p1, eps);
        int o4 = Orient(p2, q2, q1, eps);

        if (o1 != o2 && o3 != o4) return true;

        // Collinear cases
        if (o1 == 0 && OnSegment(p1, q1, p2, eps)) return true;
        if (o2 == 0 && OnSegment(p1, q1, q2, eps)) return true;
        if (o3 == 0 && OnSegment(p2, q2, p1, eps)) return true;
        if (o4 == 0 && OnSegment(p2, q2, q1, eps)) return true;

        return false;
    }
}
