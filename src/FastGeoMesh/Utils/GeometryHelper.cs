using System;
using System.Collections.Generic;
using System.Linq;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Utils
{
    /// <summary>Helper class for geometric calculations and polygon operations.</summary>
    public static class GeometryHelper
    {
        /// <summary>Compute distance from a point to a line segment.</summary>
        public static double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b, double tolerance = 0)
        {
            tolerance = tolerance <= 0 ? GeometryConfig.DefaultTolerance : tolerance;

            var ab = b - a;
            var ap = p - a;
            double len2 = ab.Dot(ab);
            if (len2 <= tolerance)
            {
                // Degenerate segment (treat as point)
                return (p - a).Length();
            }
            double t = ap.Dot(ab) / len2;
            if (t <= 0)
            {
                return (p - a).Length();
            }
            if (t >= 1)
            {
                return (p - b).Length();
            }
            var c = new Vec2(a.X + ab.X * t, a.Y + ab.Y * t);
            return (p - c).Length();
        }

        /// <summary>Point-in-polygon using ray casting (points on boundary considered inside).</summary>
        /// <param name="vertices">Polygon vertices as ReadOnlySpan for optimal performance.</param>
        /// <param name="x">Point X coordinate.</param>
        /// <param name="y">Point Y coordinate.</param>
        /// <param name="tolerance">Geometric tolerance (uses default if not specified).</param>
        /// <returns>True if point is inside or on boundary of polygon.</returns>
        public static bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0)
        {
            tolerance = tolerance <= 0 ? GeometryConfig.DefaultTolerance : tolerance;

            int n = vertices.Length;
            if (n < 3)
            {
                return false;
            }
            var p = new Vec2(x, y);
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var a = vertices[j];
                var b = vertices[i];
                // Fast bounding box rejection
                if (x + tolerance < Math.Min(a.X, b.X) || x - tolerance > Math.Max(a.X, b.X) ||
                    y + tolerance < Math.Min(a.Y, b.Y) || y - tolerance > Math.Max(a.Y, b.Y))
                {
                    continue;
                }
                double area2 = (b - a).Cross(p - a);
                if (Math.Abs(area2) <= tolerance)
                {
                    double dot = (p - a).Dot(b - a);
                    if (dot >= -tolerance && dot <= (b - a).Dot(b - a) + tolerance)
                    {
                        return true; // on segment
                    }
                }
            }
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var vi = vertices[i];
                var vj = vertices[j];
                bool intersect = ((vi.Y > y) != (vj.Y > y)) &&
                                  (x < (vj.X - vi.X) * (y - vi.Y) / (vj.Y - vi.Y) + vi.X);
                if (intersect)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        /// <summary>Test if a quadrilateral is convex (with small tolerance for near-collinear cases).</summary>
        public static bool IsConvex((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
        {
            double c1 = (quad.v1 - quad.v0).Cross(quad.v2 - quad.v1);
            double c2 = (quad.v2 - quad.v1).Cross(quad.v3 - quad.v2);
            double c3 = (quad.v3 - quad.v2).Cross(quad.v0 - quad.v3);
            double c4 = (quad.v0 - quad.v3).Cross(quad.v1 - quad.v0);

            return c1 >= GeometryConfig.ConvexityTolerance && c2 >= GeometryConfig.ConvexityTolerance &&
                   c3 >= GeometryConfig.ConvexityTolerance && c4 >= GeometryConfig.ConvexityTolerance;
        }

        /// <summary>Linear interpolation between two 2D points.</summary>
        public static Vec2 Lerp(in Vec2 a, in Vec2 b, double t)
            => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        /// <summary>Linear interpolation between two scalars.</summary>
        public static double LerpScalar(double a, double b, double t) => a + (b - a) * t;
    }
}
