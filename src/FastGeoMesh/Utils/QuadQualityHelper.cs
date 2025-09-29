using System;
using System.Collections.Generic;
using FastGeoMesh.Geometry;
using LibTessDotNet;

namespace FastGeoMesh.Utils
{
    /// <summary>Helper class for quad quality scoring and tessellation operations.</summary>
    public static class QuadQualityHelper
    {
        /// <summary>Calculate quality score for a quadrilateral (0-1, higher is better).</summary>
        public static double ScoreQuad((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
        {
            double l0 = (quad.v1 - quad.v0).Length();
            double l1 = (quad.v2 - quad.v1).Length();
            double l2 = (quad.v3 - quad.v2).Length();
            double l3 = (quad.v0 - quad.v3).Length();
            double minL = Math.Min(Math.Min(l0, l1), Math.Min(l2, l3));
            double maxL = Math.Max(Math.Max(l0, l1), Math.Max(l2, l3));
            double aspect = minL <= 1e-9 ? 0 : minL / maxL;
            
            // Calculate orthogonality - how close adjacent edges are to 90 degrees
            double o0 = CalculateOrtho((quad.v1 - quad.v0), (quad.v2 - quad.v1));
            double o1 = CalculateOrtho((quad.v2 - quad.v1), (quad.v3 - quad.v2));
            double o2 = CalculateOrtho((quad.v3 - quad.v2), (quad.v0 - quad.v3));
            double o3 = CalculateOrtho((quad.v0 - quad.v3), (quad.v1 - quad.v0));
            double ortho = 0.25 * (o0 + o1 + o2 + o3); // Average orthogonality
            
            double area = Math.Abs(Polygon2D.SignedArea(new[] { quad.v0, quad.v1, quad.v2, quad.v3 }));
            double areaScore = area > 1e-12 ? 1.0 : 0.0;
            
            // Rebalanced weights to ensure perfect square gets > 0.8
            // Perfect square: aspect=1.0, ortho=1.0, area=1.0 ? 0.5*1.0 + 0.4*1.0 + 0.1*1.0 = 1.0
            return 0.5 * aspect + 0.4 * ortho + 0.1 * areaScore;
        }

        /// <summary>Create quad from triangle pair if possible.</summary>
        public static (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3)? MakeQuadFromTrianglePair(
            (int a, int b, int c) t0, (int a, int b, int c) t1, ContourVertex[] vertices)
        {
            ArgumentNullException.ThrowIfNull(vertices);
            
            // Validate all indices before proceeding
            if (t0.a < 0 || t0.a >= vertices.Length ||
                t0.b < 0 || t0.b >= vertices.Length ||
                t0.c < 0 || t0.c >= vertices.Length ||
                t1.a < 0 || t1.a >= vertices.Length ||
                t1.b < 0 || t1.b >= vertices.Length ||
                t1.c < 0 || t1.c >= vertices.Length)
            {
                return null;
            }
            
            // Find shared vertices manually (no LINQ / avoid allocations except the two small fixed arrays)
            int shared0 = -1, shared1 = -1, sharedCount = 0;
            int[] tmp0 = { t0.a, t0.b, t0.c }; // small local array
            int[] tmp1 = { t1.a, t1.b, t1.c };
            for (int i = 0; i < 3 && sharedCount < 2; i++)
            {
                int vi = tmp0[i];
                for (int j = 0; j < 3; j++)
                {
                    if (vi == tmp1[j])
                    {
                        if (sharedCount == 0)
                        {
                            shared0 = vi;
                        }
                        else if (shared1 != vi)
                        {
                            shared1 = vi;
                        }
                        sharedCount++;
                        break;
                    }
                }
            }
            if (sharedCount != 2 || shared0 == -1 || shared1 == -1)
            {
                return null;
            }
            int unique0 = -1, unique1 = -1;
            for (int i = 0; i < 3; i++)
            {
                int v = tmp0[i];
                if (v != shared0 && v != shared1)
                {
                    unique0 = v;
                    break;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                int v = tmp1[i];
                if (v != shared0 && v != shared1)
                {
                    unique1 = v;
                    break;
                }
            }
            if (unique0 == -1 || unique1 == -1)
            {
                return null;
            }
            
            // Final validation of unique indices
            if (unique0 < 0 || unique0 >= vertices.Length ||
                unique1 < 0 || unique1 >= vertices.Length)
            {
                return null;
            }
            
            var va = new Vec2(vertices[shared0].Position.X, vertices[shared0].Position.Y);
            var vb = new Vec2(vertices[shared1].Position.X, vertices[shared1].Position.Y);
            var vc = new Vec2(vertices[unique0].Position.X, vertices[unique0].Position.Y);
            var vd = new Vec2(vertices[unique1].Position.X, vertices[unique1].Position.Y);
            var quad = (va, vc, vb, vd);
            if (GeometryHelper.IsConvex(quad))
            {
                return quad;
            }
            quad = (va, vd, vb, vc);
            return GeometryHelper.IsConvex(quad) ? quad : null;
        }

        /// <summary>Calculate orthogonality measure between two vectors (0-1, 1 is perpendicular).</summary>
        private static double CalculateOrtho(Vec2 a, Vec2 b)
        {
            double na = Math.Sqrt(a.Dot(a));
            double nb = Math.Sqrt(b.Dot(b));
            if (na <= 1e-12 || nb <= 1e-12)
            {
                return 0;
            }
            // Return 1.0 when vectors are perpendicular, 0.0 when parallel
            return 1.0 - Math.Abs(a.Dot(b) / (na * nb));
        }
    }
}