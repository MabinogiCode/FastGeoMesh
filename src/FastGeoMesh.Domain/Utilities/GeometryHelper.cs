namespace FastGeoMesh.Domain {
    /// <summary>Domain-level helper class for geometric calculations and polygon operations.</summary>
    public static class GeometryHelper {
        // Default fallbacks when options not provided
        private static readonly GeometryOptions DefaultOptions = GeometryOptions.Default;

        /// <summary>Compute distance from point <paramref name="p"/> to segment [<paramref name="a"/>, <paramref name="b"/>].</summary>
        /// <returns>Shortest distance from the point to the segment.</returns>
        public static double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b, double tolerance = 0, GeometryOptions? options = null) {
            var opts = options ?? DefaultOptions;
            if (tolerance <= 0) {
                tolerance = opts.DefaultTolerance;
            }

            var ab = b - a;
            var ap = p - a;
            double len2 = ab.Dot(ab);
            if (len2 <= tolerance) {
                return (p - a).Length();
            }

            var t = Math.Max(0.0, Math.Min(1.0, ap.Dot(ab) / len2));
            var projection = new Vec2(a.X + (ab.X * t), a.Y + (ab.Y * t));
            return (p - projection).Length();
        }

        /// <summary>Linearly interpolate between two 2D points.</summary>
        /// <returns>Interpolated 2D point at parameter <paramref name="t"/>.</returns>
        public static Vec2 Lerp(in Vec2 a, in Vec2 b, double t) {
            return new Vec2(a.X + ((b.X - a.X) * t), a.Y + ((b.Y - a.Y) * t));
        }

        /// <summary>Linearly interpolate between two scalar values.</summary>
        /// <returns>Interpolated scalar at parameter <paramref name="t"/>.</returns>
        public static double LerpScalar(double a, double b, double t) {
            return a + ((b - a) * t);
        }

        /// <summary>Check if a quadrilateral is convex.</summary>
        /// <param name="quad">Tuple of four vertices in order.</param>
        /// <param name="options">Optional geometry options controlling tolerances.</param>
        /// <returns>True if convex.</returns>
        public static bool IsConvex((Vec2 a, Vec2 b, Vec2 c, Vec2 d) quad, GeometryOptions? options = null) {
            var opts = options ?? DefaultOptions;
            var cross1 = (quad.b - quad.a).Cross(quad.c - quad.b);
            var cross2 = (quad.c - quad.b).Cross(quad.d - quad.c);
            var cross3 = (quad.d - quad.c).Cross(quad.a - quad.d);
            var cross4 = (quad.a - quad.d).Cross(quad.b - quad.a);

            var tol = opts.ConvexityTolerance;
            return (cross1 >= tol && cross2 >= tol && cross3 >= tol && cross4 >= tol)
                || (cross1 <= -tol && cross2 <= -tol && cross3 <= -tol && cross4 <= -tol);
        }

        /// <summary>Point-in-polygon test using a ReadOnlySpan and a point struct.</summary>
        /// <returns>True if the point is inside or on the polygon boundary.</returns>
        public static bool PointInPolygon(ReadOnlySpan<Vec2> vertices, in Vec2 point, double tolerance = 0, GeometryOptions? options = null) {
            return PointInPolygon(vertices, point.X, point.Y, tolerance, options);
        }

        /// <summary>Point-in-polygon test using ray casting.</summary>
        /// <param name="vertices">Polygon vertices.</param>
        /// <param name="x">X coordinate of the test point.</param>
        /// <param name="y">Y coordinate of the test point.</param>
        /// <param name="tolerance">Tolerance for boundary checks.</param>
        /// <param name="options">Optional geometry options controlling tolerances.</param>
        /// <returns>True if point is inside or on boundary.</returns>
        public static bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0, GeometryOptions? options = null) {
            var opts = options ?? DefaultOptions;
            if (tolerance <= 0) {
                tolerance = opts.PointInPolygonTolerance;
            }

            int n = vertices.Length;
            if (n < 3) {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++) {
                var vi = vertices[i];
                var vj = vertices[j];

                if (IsPointOnSegment(x, y, vi.X, vi.Y, vj.X, vj.Y, tolerance)) {
                    return true;
                }

                if (DoesEdgeCrossHorizontalRay(vi, vj, x, y)) {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>Batch point-in-polygon tests for multiple points against the same polygon.</summary>
        public static void BatchPointInPolygon(ReadOnlySpan<Vec2> vertices, ReadOnlySpan<Vec2> points, Span<bool> results, double tolerance = 0, GeometryOptions? options = null) {
            if (results.Length < points.Length) {
                throw new ArgumentException("Results span too small", nameof(results));
            }

            if (vertices.Length < 3) {
                results.Slice(0, points.Length).Clear();
                return;
            }

            for (int i = 0; i < points.Length; i++) {
                results[i] = PointInPolygon(vertices, points[i], tolerance, options);
            }
        }

        /// <summary>Optimized alias for DistancePointToSegment to match previous Infrastructure API names.</summary>
        /// <returns>Distance from <paramref name="point"/> to the segment defined by <paramref name="segmentStart"/> and <paramref name="segmentEnd"/>.</returns>
        public static double DistanceToSegment(Vec2 point, Vec2 segmentStart, Vec2 segmentEnd, GeometryOptions? options = null) {
            return DistancePointToSegment(point, segmentStart, segmentEnd, 0, options);
        }

        /// <summary>ContainsPoint optimized ray-casting alias using domain helpers.</summary>
        /// <returns>True if the polygon contains the point.</returns>
        public static bool ContainsPoint(ReadOnlySpan<Vec2> polygon, Vec2 point, GeometryOptions? options = null) {
            return PointInPolygon(polygon, point, 0, options);
        }

        /// <summary>Batch contains points alias.</summary>
        public static void ContainsPoints(ReadOnlySpan<Vec2> polygon, ReadOnlySpan<Vec2> points, Span<bool> results, GeometryOptions? options = null) {
            BatchPointInPolygon(polygon, points, results, 0, options);
        }

        /// <summary>Compute polygon area using the Shoelace formula.</summary>
        /// <returns>Unsigned polygon area.</returns>
        public static double PolygonArea(ReadOnlySpan<Vec2> vertices) {
            if (vertices.Length < 3) {
                return 0.0;
            }

            double area = 0.0;
            int n = vertices.Length;
            for (int i = 0; i < n; i++) {
                int j = (i + 1) % n;
                double term1 = vertices[i].X * vertices[j].Y;
                double term2 = vertices[j].X * vertices[i].Y;
                area += term1 - term2;
            }

            return Math.Abs(area) * 0.5;
        }

        /// <summary>Compute signed polygon area (positive for CCW).</summary>
        /// <returns>Signed polygon area (positive if CCW).</returns>
        public static double SignedArea(ReadOnlySpan<Vec2> vertices) {
            if (vertices.Length < 3) {
                return 0.0;
            }

            double area = 0.0;
            int n = vertices.Length;
            for (int i = 0; i < n; i++) {
                int j = (i + 1) % n;
                double term1 = vertices[i].X * vertices[j].Y;
                double term2 = vertices[j].X * vertices[i].Y;
                area += term1 - term2;
            }

            return area * 0.5;
        }

        private static bool DoesEdgeCrossHorizontalRay(in Vec2 edgeStart, in Vec2 edgeEnd, double pointX, double pointY) {
            if ((edgeStart.Y > pointY) == (edgeEnd.Y > pointY)) {
                return false;
            }

            double dx = edgeEnd.X - edgeStart.X;
            double dy = edgeEnd.Y - edgeStart.Y;
            double numerator = dx * (pointY - edgeStart.Y);
            double xAtY = numerator / dy;
            double intersectionX = xAtY + edgeStart.X;
            return pointX < intersectionX;
        }

        private static bool IsPointOnSegment(double px, double py, double ax, double ay, double bx, double by, double tolerance) {
            double apx = px - ax;
            double apy = py - ay;
            double abx = bx - ax;
            double aby = by - ay;
            double prod1 = apx * aby;
            double prod2 = apy * abx;
            double cross = Math.Abs(prod1 - prod2);
            if (cross > tolerance) {
                return false;
            }

            double dot = (apx * abx) + (apy * aby);
            double squaredLength = (abx * abx) + (aby * aby);
            return dot >= -tolerance && dot <= squaredLength + tolerance;
        }
    }
}
