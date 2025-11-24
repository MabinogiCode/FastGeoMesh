using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure.Utilities
{
    /// <summary>Injectable implementation of geometric helper functions.</summary>
    public sealed class GeometryHelperImpl : IGeometryHelper
    {
        private readonly IGeometryConfig _config;

        /// <summary>Create a new GeometryHelperImpl.</summary>
        public GeometryHelperImpl(IGeometryConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <inheritdoc/>
        public double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b, double tolerance = 0)
        {
            tolerance = tolerance <= 0 ? _config.DefaultTolerance : tolerance;

            var ab = b - a;
            var ap = p - a;
            double len2 = ab.Dot(ab);
            if (len2 <= tolerance)
            {
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

        /// <inheritdoc/>
        public Vec2 Lerp(in Vec2 a, in Vec2 b, double t)
        {
            return new Vec2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        /// <inheritdoc/>
        public double LerpScalar(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        /// <inheritdoc/>
        public bool IsConvex((Vec2 a, Vec2 b, Vec2 c, Vec2 d) quad)
        {
            var cross1 = (quad.b - quad.a).Cross(quad.c - quad.b);
            var cross2 = (quad.c - quad.b).Cross(quad.d - quad.c);
            var cross3 = (quad.d - quad.c).Cross(quad.a - quad.d);
            var cross4 = (quad.a - quad.d).Cross(quad.b - quad.a);

            var tol = _config.ConvexityTolerance;
            return (cross1 >= tol && cross2 >= tol && cross3 >= tol && cross4 >= tol) ||
                   (cross1 <= -tol && cross2 <= -tol && cross3 <= -tol && cross4 <= -tol);
        }

        /// <inheritdoc/>
        public bool PointInPolygon(ReadOnlySpan<Vec2> vertices, in Vec2 point, double tolerance = 0)
        {
            return PointInPolygon(vertices, point.X, point.Y, tolerance);
        }

        /// <inheritdoc/>
        public bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0)
        {
            tolerance = tolerance <= 0 ? _config.PointInPolygonTolerance : tolerance;

            int n = vertices.Length;
            if (n < 3)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var vi = vertices[i];
                var vj = vertices[j];
                if (IsPointOnSegment(x, y, vi.X, vi.Y, vj.X, vj.Y, tolerance))
                {
                    return true;
                }

                if (DoesEdgeCrossHorizontalRay(vi, vj, x, y))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <inheritdoc/>
        public void BatchPointInPolygon(ReadOnlySpan<Vec2> vertices, ReadOnlySpan<Vec2> points, Span<bool> results, double tolerance = 0)
        {
            if (points.Length != results.Length)
            {
                throw new ArgumentException("Points and results spans must have the same length");
            }

            for (int i = 0; i < points.Length; i++)
            {
                results[i] = PointInPolygon(vertices, points[i], tolerance);
            }
        }

        /// <inheritdoc/>
        public double PolygonArea(ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.Length < 3)
            {
                return 0.0;
            }

            double area = 0.0;
            int n = vertices.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += vertices[i].X * vertices[j].Y;
                area -= vertices[j].X * vertices[i].Y;
            }

            return Math.Abs(area) * 0.5;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822:Mark members as static", Justification = "Coding guideline prohibits mixing static and instance methods")]
        private bool DoesEdgeCrossHorizontalRay(in Vec2 a, in Vec2 b, double x, double y)
        {
            if ((a.Y > y) == (b.Y > y))
            {
                return false;
            }

            double intersectionX = (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X;
            return x < intersectionX;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822:Mark members as static", Justification = "Coding guideline prohibits mixing static and instance methods")]
        private bool IsPointOnSegment(double px, double py, double ax, double ay, double bx, double by, double tolerance)
        {
            double apx = px - ax;
            double apy = py - ay;
            double abx = bx - ax;
            double aby = by - ay;

            double cross = Math.Abs(apx * aby - apy * abx);
            if (cross > tolerance)
            {
                return false;
            }

            double dot = apx * abx + apy * aby;
            double squaredLength = abx * abx + aby * aby;

            return dot >= -tolerance && dot <= squaredLength + tolerance;
        }
    }
}
