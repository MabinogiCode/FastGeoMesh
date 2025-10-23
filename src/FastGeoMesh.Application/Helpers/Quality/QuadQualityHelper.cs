using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using FastGeoMesh.Domain;
using LibTessDotNet;

namespace FastGeoMesh.Application.Helpers.Quality {
    /// <summary>Helper class for quad quality scoring and tessellation operations with SIMD optimizations.</summary>
    internal static class QuadQualityHelper {
        // Test hook: when true, force using SIMD path but run an emulation to avoid AVX requirement.
        internal static bool ForceSimd { get; set; } = false;

        /// <summary>Calculate quality score for a quadrilateral (0-1, higher is better) with SIMD optimizations.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double ScoreQuad((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad) {
            // Use hardware SIMD when available, or honor the test hook to exercise SIMD code paths.
            if ((Vector256.IsHardwareAccelerated && Avx.IsSupported) || ForceSimd) {
                return ScoreQuadSIMD(quad);
            }

            return ScoreQuadScalar(quad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ScoreQuadScalar((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad) {
            double l0 = (quad.v1 - quad.v0).Length();
            double l1 = (quad.v2 - quad.v1).Length();
            double l2 = (quad.v3 - quad.v2).Length();
            double l3 = (quad.v0 - quad.v3).Length();
            double minL = Math.Min(Math.Min(l0, l1), Math.Min(l2, l3));
            double maxL = Math.Max(Math.Max(l0, l1), Math.Max(l2, l3));
            double aspect = minL <= 1e-9 ? 0 : minL / maxL;

            // Calculate orthogonality - how close adjacent edges are to 90 degrees
            double o0 = GeometryCalculationHelper.CalculateOrtho((quad.v1 - quad.v0), (quad.v2 - quad.v1));
            double o1 = GeometryCalculationHelper.CalculateOrtho((quad.v2 - quad.v1), (quad.v3 - quad.v2));
            double o2 = GeometryCalculationHelper.CalculateOrtho((quad.v3 - quad.v2), (quad.v0 - quad.v3));
            double o3 = GeometryCalculationHelper.CalculateOrtho((quad.v0 - quad.v3), (quad.v1 - quad.v0));
            double ortho = 0.25 * (o0 + o1 + o2 + o3); // Average orthogonality

            double area = Math.Abs(Polygon2D.SignedArea(new[] { quad.v0, quad.v1, quad.v2, quad.v3 }));
            double areaScore = area > 1e-12 ? 1.0 : 0.0;

            // Rebalanced weights to ensure perfect square gets > 0.8
            return 0.5 * aspect + 0.4 * ortho + 0.1 * areaScore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ScoreQuadSIMD((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad) {
            // If ForceSimd is set but Avx is not supported, emulate the SIMD path using scalar code
            // so tests can exercise the same logic without requiring AVX hardware.
            if (ForceSimd && !(Vector256.IsHardwareAccelerated && Avx.IsSupported)) {
                // Emulate SIMD computations using scalar operations matching the SIMD implementation.
                double[] dx = new double[4];
                double[] dy = new double[4];

                dx[0] = quad.v1.X - quad.v0.X; dy[0] = quad.v1.Y - quad.v0.Y;
                dx[1] = quad.v2.X - quad.v1.X; dy[1] = quad.v2.Y - quad.v1.Y;
                dx[2] = quad.v3.X - quad.v2.X; dy[2] = quad.v3.Y - quad.v2.Y;
                dx[3] = quad.v0.X - quad.v3.X; dy[3] = quad.v0.Y - quad.v3.Y;

                double[] emuLengths = new double[4];
                for (int i = 0; i < 4; i++) emuLengths[i] = Math.Sqrt(dx[i] * dx[i] + dy[i] * dy[i]);

                double emuMinL = Math.Min(Math.Min(emuLengths[0], emuLengths[1]), Math.Min(emuLengths[2], emuLengths[3]));
                double emuMaxL = Math.Max(Math.Max(emuLengths[0], emuLengths[1]), Math.Max(emuLengths[2], emuLengths[3]));
                double emuAspect = emuMinL <= 1e-9 ? 0 : emuMinL / emuMaxL;

                double emuO0 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dx[0], dy[0]), new Vec2(dx[1], dy[1]));
                double emuO1 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dx[1], dy[1]), new Vec2(dx[2], dy[2]));
                double emuO2 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dx[2], dy[2]), new Vec2(dx[3], dy[3]));
                double emuO3 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dx[3], dy[3]), new Vec2(dx[0], dy[0]));
                double emuOrtho = 0.25 * (emuO0 + emuO1 + emuO2 + emuO3);

                double emuArea = Math.Abs(Polygon2D.SignedArea(new[] { quad.v0, quad.v1, quad.v2, quad.v3 }));
                double emuAreaScore = emuArea > 1e-12 ? 1.0 : 0.0;

                return 0.5 * emuAspect + 0.4 * emuOrtho + 0.1 * emuAreaScore;
            }

            // Hardware-accelerated path using AVX intrinsics
            var x = Vector256.Create(quad.v0.X, quad.v1.X, quad.v2.X, quad.v3.X);
            var y = Vector256.Create(quad.v0.Y, quad.v1.Y, quad.v2.Y, quad.v3.Y);

            var dxv = Vector256.Create(quad.v1.X - quad.v0.X, quad.v2.X - quad.v1.X, quad.v3.X - quad.v2.X, quad.v0.X - quad.v3.X);
            var dyv = Vector256.Create(quad.v1.Y - quad.v0.Y, quad.v2.Y - quad.v1.Y, quad.v3.Y - quad.v2.Y, quad.v0.Y - quad.v3.Y);

            var lengthsSquared = Avx.Add(Avx.Multiply(dxv, dxv), Avx.Multiply(dyv, dyv));

            Span<double> lengths = stackalloc double[4];
            for (int i = 0; i < 4; i++) {
                lengths[i] = Math.Sqrt(lengthsSquared.GetElement(i));
            }

            double minL = Math.Min(Math.Min(lengths[0], lengths[1]), Math.Min(lengths[2], lengths[3]));
            double maxL = Math.Max(Math.Max(lengths[0], lengths[1]), Math.Max(lengths[2], lengths[3]));
            double aspect = minL <= 1e-9 ? 0 : minL / maxL;

            double o0 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dxv.GetElement(0), dyv.GetElement(0)), new Vec2(dxv.GetElement(1), dyv.GetElement(1)));
            double o1 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dxv.GetElement(1), dyv.GetElement(1)), new Vec2(dxv.GetElement(2), dyv.GetElement(2)));
            double o2 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dxv.GetElement(2), dyv.GetElement(2)), new Vec2(dxv.GetElement(3), dyv.GetElement(3)));
            double o3 = GeometryCalculationHelper.CalculateOrtho(new Vec2(dxv.GetElement(3), dyv.GetElement(3)), new Vec2(dxv.GetElement(0), dyv.GetElement(0)));
            double ortho = 0.25 * (o0 + o1 + o2 + o3);

            double area = Math.Abs(Polygon2D.SignedArea(new[] { quad.v0, quad.v1, quad.v2, quad.v3 }));
            double areaScore = area > 1e-12 ? 1.0 : 0.0;

            return 0.5 * aspect + 0.4 * ortho + 0.1 * areaScore;
        }

        /// <summary>Create quad from triangle pair if possible with enhanced validation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3)? MakeQuadFromTrianglePair(
            (int a, int b, int c) t0, (int a, int b, int c) t1, ContourVertex[] vertices) {
            ArgumentNullException.ThrowIfNull(vertices);

            // Validate all indices before proceeding
            if (t0.a < 0 || t0.a >= vertices.Length ||
                t0.b < 0 || t0.b >= vertices.Length ||
                t0.c < 0 || t0.c >= vertices.Length ||
                t1.a < 0 || t1.a >= vertices.Length ||
                t1.b < 0 || t1.b >= vertices.Length ||
                t1.c < 0 || t1.c >= vertices.Length) {
                return null;
            }

            // Find shared vertices manually using stack-allocated arrays for better performance
            int shared0 = -1, shared1 = -1, sharedCount = 0;
            Span<int> tmp0 = stackalloc int[3] { t0.a, t0.b, t0.c };
            Span<int> tmp1 = stackalloc int[3] { t1.a, t1.b, t1.c };

            for (int i = 0; i < 3 && sharedCount < 2; i++) {
                int vi = tmp0[i];
                for (int j = 0; j < 3; j++) {
                    if (vi == tmp1[j]) {
                        if (sharedCount == 0) {
                            shared0 = vi;
                        }
                        else if (shared1 != vi) {
                            shared1 = vi;
                        }
                        sharedCount++;
                        break;
                    }
                }
            }

            if (sharedCount != 2 || shared0 == -1 || shared1 == -1) {
                return null;
            }

            int unique0 = -1, unique1 = -1;
            for (int i = 0; i < 3; i++) {
                int v = tmp0[i];
                if (v != shared0 && v != shared1) {
                    unique0 = v;
                    break;
                }
            }
            for (int i = 0; i < 3; i++) {
                int v = tmp1[i];
                if (v != shared0 && v != shared1) {
                    unique1 = v;
                    break;
                }
            }

            if (unique0 == -1 || unique1 == -1 ||
                unique0 < 0 || unique0 >= vertices.Length ||
                unique1 < 0 || unique1 >= vertices.Length) {
                return null;
            }

            var va = new Vec2(vertices[shared0].Position.X, vertices[shared0].Position.Y);
            var vb = new Vec2(vertices[shared1].Position.X, vertices[shared1].Position.Y);
            var vc = new Vec2(vertices[unique0].Position.X, vertices[unique0].Position.Y);
            var vd = new Vec2(vertices[unique1].Position.X, vertices[unique1].Position.Y);

            var quad = (va, vc, vb, vd);
            if (GeometryCalculationHelper.IsConvex(quad)) {
                return quad;
            }

            quad = (va, vd, vb, vc);
            return GeometryCalculationHelper.IsConvex(quad) ? quad : null;
        }
    }
}
