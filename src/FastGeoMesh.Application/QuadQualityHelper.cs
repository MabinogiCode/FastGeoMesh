using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using FastGeoMesh.Domain;
using LibTessDotNet;

namespace FastGeoMesh.Application
{
    /// <summary>Helper class for quad quality scoring and tessellation operations with SIMD optimizations.</summary>
    public static class QuadQualityHelper
    {
        /// <summary>Calculate quality score for a quadrilateral (0-1, higher is better) with SIMD optimizations.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ScoreQuad((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
        {
            // Use SIMD when available for distance calculations
            if (Vector256.IsHardwareAccelerated && Avx.IsSupported)
            {
                return ScoreQuadSIMD(quad);
            }

            return ScoreQuadScalar(quad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ScoreQuadScalar((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
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
            return 0.5 * aspect + 0.4 * ortho + 0.1 * areaScore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ScoreQuadSIMD((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)
        {
            // Pack coordinates into SIMD vectors for parallel computation
            var x = Vector256.Create(quad.v0.X, quad.v1.X, quad.v2.X, quad.v3.X);
            var y = Vector256.Create(quad.v0.Y, quad.v1.Y, quad.v2.Y, quad.v3.Y);

            // Calculate edge vectors using SIMD
            var dx = Vector256.Create(quad.v1.X - quad.v0.X, quad.v2.X - quad.v1.X, quad.v3.X - quad.v2.X, quad.v0.X - quad.v3.X);
            var dy = Vector256.Create(quad.v1.Y - quad.v0.Y, quad.v2.Y - quad.v1.Y, quad.v3.Y - quad.v2.Y, quad.v0.Y - quad.v3.Y);

            // Calculate edge lengths using SIMD
            var lengthsSquared = Avx.Add(Avx.Multiply(dx, dx), Avx.Multiply(dy, dy));

            // Extract individual lengths for min/max calculation
            Span<double> lengths = stackalloc double[4];
            for (int i = 0; i < 4; i++)
            {
                lengths[i] = Math.Sqrt(lengthsSquared.GetElement(i));
            }

            double minL = Math.Min(Math.Min(lengths[0], lengths[1]), Math.Min(lengths[2], lengths[3]));
            double maxL = Math.Max(Math.Max(lengths[0], lengths[1]), Math.Max(lengths[2], lengths[3]));
            double aspect = minL <= 1e-9 ? 0 : minL / maxL;

            // Calculate orthogonality (fallback to scalar for complex operations)
            double o0 = CalculateOrtho(new Vec2(dx.GetElement(0), dy.GetElement(0)), new Vec2(dx.GetElement(1), dy.GetElement(1)));
            double o1 = CalculateOrtho(new Vec2(dx.GetElement(1), dy.GetElement(1)), new Vec2(dx.GetElement(2), dy.GetElement(2)));
            double o2 = CalculateOrtho(new Vec2(dx.GetElement(2), dy.GetElement(2)), new Vec2(dx.GetElement(3), dy.GetElement(3)));
            double o3 = CalculateOrtho(new Vec2(dx.GetElement(3), dy.GetElement(3)), new Vec2(dx.GetElement(0), dy.GetElement(0)));
            double ortho = 0.25 * (o0 + o1 + o2 + o3);

            double area = Math.Abs(Polygon2D.SignedArea(new[] { quad.v0, quad.v1, quad.v2, quad.v3 }));
            double areaScore = area > 1e-12 ? 1.0 : 0.0;

            return 0.5 * aspect + 0.4 * ortho + 0.1 * areaScore;
        }

        /// <summary>Create quad from triangle pair if possible with enhanced validation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            // Find shared vertices manually using stack-allocated arrays for better performance
            int shared0 = -1, shared1 = -1, sharedCount = 0;
            Span<int> tmp0 = stackalloc int[3] { t0.a, t0.b, t0.c };
            Span<int> tmp1 = stackalloc int[3] { t1.a, t1.b, t1.c };

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

            if (unique0 == -1 || unique1 == -1 ||
                unique0 < 0 || unique0 >= vertices.Length ||
                unique1 < 0 || unique1 >= vertices.Length)
            {
                return null;
            }

            var va = new Vec2(vertices[shared0].Position.X, vertices[shared0].Position.Y);
            var vb = new Vec2(vertices[shared1].Position.X, vertices[shared1].Position.Y);
            var vc = new Vec2(vertices[unique0].Position.X, vertices[unique0].Position.Y);
            var vd = new Vec2(vertices[unique1].Position.X, vertices[unique1].Position.Y);

            var quad = (va, vc, vb, vd);
            if (Infrastructure.GeometryHelper.IsConvex(quad))
            {
                return quad;
            }

            quad = (va, vd, vb, vc);
            return Infrastructure.GeometryHelper.IsConvex(quad) ? quad : null;
        }

        /// <summary>Calculate orthogonality measure between two vectors (0-1, 1 is perpendicular).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
