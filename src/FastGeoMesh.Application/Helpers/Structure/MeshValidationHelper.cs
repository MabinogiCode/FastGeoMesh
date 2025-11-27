using FastGeoMesh.Domain;

namespace FastGeoMesh.Application.Helpers.Structure
{
    /// <summary>Helper class for mesh structure validation operations.</summary>
    internal static class MeshValidationHelper
    {
        /// <summary>Validates that the given polygon is simple and non-self-intersecting.</summary>
        internal static bool ValidatePolygon(Polygon2D polygon)
        {
            if (polygon == null)
            {
                return false;
            }

            var vertices = polygon.Vertices;
            if (vertices == null || vertices.Count < 3)
            {
                return false;
            }

            // Check for duplicate consecutive vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextIndex = (i + 1) % vertices.Count;
                if (ArePointsEqual(vertices[i], vertices[nextIndex]))
                {
                    return false;
                }
            }

            // Check for self-intersections (basic check)
            // This is a simplified check - for production, consider more robust algorithms
            for (int i = 0; i < vertices.Count; i++)
            {
                int j = (i + 1) % vertices.Count;
                var seg1Start = vertices[i];
                var seg1End = vertices[j];

                for (int k = i + 2; k < vertices.Count; k++)
                {
                    int l = (k + 1) % vertices.Count;
                    if (l == i)
                    {
                        continue; // Skip adjacent segments
                    }

                    var seg2Start = vertices[k];
                    var seg2End = vertices[l];

                    if (SegmentsIntersect(seg1Start, seg1End, seg2Start, seg2End))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ArePointsEqual(Vec2 p1, Vec2 p2, double tolerance = 1e-10)
        {
            return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance;
        }

        private static bool SegmentsIntersect(Vec2 a1, Vec2 a2, Vec2 b1, Vec2 b2)
        {
            double d = (a2.X - a1.X) * (b2.Y - b1.Y) - (a2.Y - a1.Y) * (b2.X - b1.X);
            if (Math.Abs(d) < 1e-10)
            {
                return false; // Parallel or collinear
            }

            double t = ((b1.X - a1.X) * (b2.Y - b1.Y) - (b1.Y - a1.Y) * (b2.X - b1.X)) / d;
            double u = ((b1.X - a1.X) * (a2.Y - a1.Y) - (b1.Y - a1.Y) * (a2.X - a1.X)) / d;

            return t > 0 && t < 1 && u > 0 && u < 1;
        }
    }
}
