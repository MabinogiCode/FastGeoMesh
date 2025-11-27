using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    // Minimal geometry service implementation used as a sensible default for legacy call sites/tests.
    internal sealed class GeometryServiceImpl : IGeometryService
    {
        public double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b, double tolerance = 0)
        {
            // Project p onto segment ab clamped
            var vx = b.X - a.X;
            var vy = b.Y - a.Y;
            var wx = p.X - a.X;
            var wy = p.Y - a.Y;
            var denom = vx * vx + vy * vy;
            if (denom <= double.Epsilon)
            {
                return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
            }

            var t = (vx * wx + vy * wy) / denom;
            t = Clamp(t, 0.0, 1.0);
            var projx = a.X + t * vx;
            var projy = a.Y + t * vy;
            var dx = p.X - projx;
            var dy = p.Y - projy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public Vec2 Lerp(in Vec2 a, in Vec2 b, double t)
            => new Vec2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        public Vec3 Lerp(in Vec3 a, in Vec3 b, double t)
            => new Vec3(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);

        public double LerpScalar(double a, double b, double t) => a + (b - a) * t;

        public bool IsConvex((Vec2 a, Vec2 b, Vec2 c, Vec2 d) quad)
        {
            // Check sign of cross products
            static double cross(Vec2 p0, Vec2 p1, Vec2 p2) => (p1.X - p0.X) * (p2.Y - p0.Y) - (p1.Y - p0.Y) * (p2.X - p0.X);
            var c1 = cross(quad.a, quad.b, quad.c);
            var c2 = cross(quad.b, quad.c, quad.d);
            var c3 = cross(quad.c, quad.d, quad.a);
            var c4 = cross(quad.d, quad.a, quad.b);
            return (c1 >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0) || (c1 <= 0 && c2 <= 0 && c3 <= 0 && c4 <= 0);
        }

        public bool PointInPolygon(ReadOnlySpan<Vec2> vertices, in Vec2 point, double tolerance = 0)
            => PointInPolygon(vertices, point.X, point.Y, tolerance);

        public bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0)
        {
            // Ray casting algorithm
            var inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                var xi = vertices[i].X; var yi = vertices[i].Y;
                var xj = vertices[j].X; var yj = vertices[j].Y;
                var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi + double.Epsilon) + xi);
                if (intersect)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public void BatchPointInPolygon(ReadOnlySpan<Vec2> vertices, ReadOnlySpan<Vec2> points, Span<bool> results, double tolerance = 0)
        {
            if (results.Length != points.Length)
            {
                throw new ArgumentException("Results span length must match points length");
            }

            for (int i = 0; i < points.Length; i++)
            {
                results[i] = PointInPolygon(vertices, points[i], tolerance);
            }
        }

        public double PolygonArea(ReadOnlySpan<Vec2> vertices)
        {
            double area = 0;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                area += (vertices[j].X - vertices[i].X) * (vertices[j].Y + vertices[i].Y);
            }
            return Math.Abs(area) * 0.5; // standard shoelace but simplified
        }

        public double TriangleArea(in Vec2 a, in Vec2 b, in Vec2 c)
        {
            return Math.Abs((a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) * 0.5);
        }

        public double QuadArea((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
        {
            // Split into two triangles
            return TriangleArea(quad.v0, quad.v1, quad.v2) + TriangleArea(quad.v0, quad.v2, quad.v3);
        }

        public double Distance(in Vec2 a, in Vec2 b) => Math.Sqrt(DistanceSquared(a, b));

        public double DistanceSquared(in Vec2 a, in Vec2 b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y; return dx * dx + dy * dy;
        }

        public Vec2 Centroid(ReadOnlySpan<Vec2> points)
        {
            if (points.Length == 0)
            {
                return Vec2.Zero;
            }

            double sx = 0, sy = 0;
            for (int i = 0; i < points.Length; i++) { sx += points[i].X; sy += points[i].Y; }
            return new Vec2(sx / points.Length, sy / points.Length);
        }

        public Vec2 Normalize(in Vec2 vector)
        {
            var len = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (len <= double.Epsilon)
            {
                return Vec2.Zero;
            }
            return new Vec2(vector.X / len, vector.Y / len);
        }

        public double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
    }
}
