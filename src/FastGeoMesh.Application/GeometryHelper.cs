using FastGeoMesh.Domain;

namespace FastGeoMesh.Application
{
    /// <summary>Helper methods for geometric operations.</summary>
    internal static class GeometryHelper
    {
        /// <summary>Linear interpolation between two 2D points.</summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">End point.</param>
        /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
        /// <returns>Interpolated point.</returns>
        internal static Vec2 Lerp(Vec2 a, Vec2 b, double t)
        {
            return new Vec2(
                a.X + t * (b.X - a.X),
                a.Y + t * (b.Y - a.Y)
            );
        }

        /// <summary>Linear interpolation between two 3D points.</summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">End point.</param>
        /// <param name="t">Interpolation parameter (0 = a, 1 = b).</param>
        /// <returns>Interpolated point.</returns>
        internal static Vec3 Lerp(Vec3 a, Vec3 b, double t)
        {
            return new Vec3(
                a.X + t * (b.X - a.X),
                a.Y + t * (b.Y - a.Y),
                a.Z + t * (b.Z - a.Z)
            );
        }

        /// <summary>Checks if a point is inside a polygon using ray casting algorithm.</summary>
        /// <param name="polygon">Array of polygon vertices.</param>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point.</param>
        /// <returns>True if the point is inside the polygon, false otherwise.</returns>
        internal static bool PointInPolygon(Vec2[] polygon, double x, double y)
        {
            int n = polygon.Length;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].Y > y) != (polygon[j].Y > y)) &&
                    (x < (polygon[j].X - polygon[i].X) * (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>Calculates the squared distance between two 2D points.</summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>Squared distance.</returns>
        internal static double DistanceSquared(Vec2 a, Vec2 b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>Calculates the distance between two 2D points.</summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>Distance.</returns>
        internal static double Distance(Vec2 a, Vec2 b)
        {
            return Math.Sqrt(DistanceSquared(a, b));
        }

        /// <summary>Checks if a quad is convex.</summary>
        /// <param name="quad">The quad to check.</param>
        /// <returns>True if the quad is convex, false otherwise.</returns>
        internal static bool IsConvex((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
        {
            // Check if all interior angles are less than 180 degrees
            // by checking if all cross products have the same sign
            var cross1 = CrossProduct(quad.v1 - quad.v0, quad.v2 - quad.v1);
            var cross2 = CrossProduct(quad.v2 - quad.v1, quad.v3 - quad.v2);
            var cross3 = CrossProduct(quad.v3 - quad.v2, quad.v0 - quad.v3);
            var cross4 = CrossProduct(quad.v0 - quad.v3, quad.v1 - quad.v0);

            // All cross products should have the same sign for convexity
            return (cross1 >= 0 && cross2 >= 0 && cross3 >= 0 && cross4 >= 0) ||
                   (cross1 <= 0 && cross2 <= 0 && cross3 <= 0 && cross4 <= 0);
        }

        /// <summary>Computes the cross product of two 2D vectors.</summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>The z-component of the cross product (scalar).</returns>
        private static double CrossProduct(Vec2 a, Vec2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
