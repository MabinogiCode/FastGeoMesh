using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Utils
{
    /// <summary>Helper class for geometric calculations and polygon operations optimized for .NET 8.</summary>
    public static class GeometryHelper
    {
        /// <summary>Compute distance from a point to a line segment using optimized arithmetic.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>Linear interpolation between two 2D points.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Lerp(in Vec2 a, in Vec2 b, double t)
        {
            return new Vec2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        /// <summary>Linear interpolation between two scalar values.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpScalar(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        /// <summary>Check if a quadrilateral defined by four 2D points is convex.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsConvex((Vec2 a, Vec2 b, Vec2 c, Vec2 d) quad)
        {
            // Check if all cross products have the same sign (indicating consistent winding)
            var cross1 = (quad.b - quad.a).Cross(quad.c - quad.b);
            var cross2 = (quad.c - quad.b).Cross(quad.d - quad.c);
            var cross3 = (quad.d - quad.c).Cross(quad.a - quad.d);
            var cross4 = (quad.a - quad.d).Cross(quad.b - quad.a);

            // All should have the same sign for convexity
            var tolerance = GeometryConfig.ConvexityTolerance;
            return (cross1 >= tolerance && cross2 >= tolerance && cross3 >= tolerance && cross4 >= tolerance) ||
                   (cross1 <= -tolerance && cross2 <= -tolerance && cross3 <= -tolerance && cross4 <= -tolerance);
        }

        /// <summary>
        /// High-performance point-in-polygon test using ray casting optimized for .NET 8.
        /// Uses ReadOnlySpan for zero-copy vertex access and aggressive inlining.
        /// </summary>
        /// <param name="vertices">Polygon vertices as ReadOnlySpan for optimal performance.</param>
        /// <param name="point">Point to test.</param>
        /// <param name="tolerance">Geometric tolerance (uses default if not specified).</param>
        /// <returns>True if point is inside or on boundary of polygon.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PointInPolygon(ReadOnlySpan<Vec2> vertices, in Vec2 point, double tolerance = 0)
        {
            return PointInPolygon(vertices, point.X, point.Y, tolerance);
        }

        /// <summary>
        /// Point-in-polygon using corrected ray casting algorithm.
        /// Points on boundary considered inside. Fixed algorithm that actually works.
        /// </summary>
        /// <param name="vertices">Polygon vertices as ReadOnlySpan for optimal performance.</param>
        /// <param name="x">Point X coordinate.</param>
        /// <param name="y">Point Y coordinate.</param>
        /// <param name="tolerance">Geometric tolerance (uses default if not specified).</param>
        /// <returns>True if point is inside or on boundary of polygon.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0)
        {
            tolerance = tolerance <= 0 ? GeometryConfig.PointInPolygonTolerance : tolerance;

            int n = vertices.Length;
            if (n < 3)
            {
                return false;
            }

            bool inside = false;

            // Standard ray casting algorithm with proper edge case handling
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var vi = vertices[i];
                var vj = vertices[j];

                // Check if point is on edge
                if (IsPointOnSegment(x, y, vi.X, vi.Y, vj.X, vj.Y, tolerance))
                {
                    return true; // Point on boundary counts as inside
                }

                // Ray casting test: cast ray from point to the right (+X direction)
                // Check if edge crosses the ray
                if (((vi.Y > y) != (vj.Y > y)) &&
                    (x < (vj.X - vi.X) * (y - vi.Y) / (vj.Y - vi.Y) + vi.X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Fast batch point-in-polygon test for multiple points using .NET 8 Span optimizations.
        /// </summary>
        /// <param name="vertices">Polygon vertices.</param>
        /// <param name="points">Points to test.</param>
        /// <param name="results">Results span to write to (must be same length as points).</param>
        /// <param name="tolerance">Geometric tolerance.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void BatchPointInPolygon(ReadOnlySpan<Vec2> vertices, ReadOnlySpan<Vec2> points, 
            Span<bool> results, double tolerance = 0)
        {
            if (points.Length != results.Length)
            {
                throw new ArgumentException("Points and results spans must have the same length");
            }

            // Optimize for small batches with direct iteration
            for (int i = 0; i < points.Length; i++)
            {
                results[i] = PointInPolygon(vertices, points[i], tolerance);
            }
        }

        /// <summary>
        /// Check if a point lies on a line segment within tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPointOnSegment(double px, double py, double ax, double ay, double bx, double by, double tolerance)
        {
            // Vector from A to P
            double apx = px - ax;
            double apy = py - ay;

            // Vector from A to B  
            double abx = bx - ax;
            double aby = by - ay;

            // Cross product to check if point is on line
            double cross = Math.Abs(apx * aby - apy * abx);
            
            // If not on line (within tolerance), return false
            if (cross > tolerance)
            {
                return false;
            }

            // Check if point is within segment bounds
            double dot = apx * abx + apy * aby;
            double squaredLength = abx * abx + aby * aby;

            // Point is on segment if 0 <= dot <= squaredLength (with tolerance)
            return dot >= -tolerance && dot <= squaredLength + tolerance;
        }

        /// <summary>
        /// Optimized polygon area calculation using Shoelace formula with Span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double PolygonArea(ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.Length < 3) 
            {
                return 0.0;
            }
            
            double area = 0.0;
            int n = vertices.Length;
            
            // Standard shoelace formula
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += vertices[i].X * vertices[j].Y;
                area -= vertices[j].X * vertices[i].Y;
            }
            
            return Math.Abs(area) * 0.5;
        }
    }
}
