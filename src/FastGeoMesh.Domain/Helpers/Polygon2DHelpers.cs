namespace FastGeoMesh.Domain
{
    internal static class Polygon2DHelpers
    {
        public static int Orient(in Vec2 a, in Vec2 b, in Vec2 c, double eps)
        {
            double v = (b - a).Cross(c - a);
            if (Math.Abs(v) <= eps)
            {
                return 0;
            }
            return v > 0 ? 1 : -1;
        }

        public static bool OnSegment(in Vec2 a, in Vec2 b, in Vec2 p, double eps)
        {
            if (Orient(a, b, p, eps) != 0)
            {
                return false;
            }

            return p.X <= Math.Max(a.X, b.X) + eps
                   && p.X + eps >= Math.Min(a.X, b.X)
                   && p.Y <= Math.Max(a.Y, b.Y) + eps
                   && p.Y + eps >= Math.Min(a.Y, b.Y);
        }

        public static bool SegmentsIntersect(in Vec2 p1, in Vec2 q1, in Vec2 p2, in Vec2 q2, double eps)
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
}