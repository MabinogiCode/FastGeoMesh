using System;
using System.Reflection;
using FastGeoMesh.Application.Helpers.Quality;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;
using System.Runtime.Intrinsics.X86;

namespace FastGeoMesh.Tests.Application {
    public sealed class QuadQualityHelperSimdTests {
        [Fact]
        public void InvokeScoreQuadSIMDEitherReturnsComparableValueOrThrowsPlatformNotSupported() {
            var quad = (new Vec2(0,0), new Vec2(2,0), new Vec2(2,2), new Vec2(0,2));

            // Get non-public static method ScoreQuadSIMD via reflection
            var t = typeof(QuadQualityHelper);
            var mi = t.GetMethod("ScoreQuadSIMD", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Should().NotBeNull("SIMD scoring implementation should exist for testing purposes");

            try {
                var resultObj = mi!.Invoke(null, new object[] { quad });
                resultObj.Should().NotBeNull();
                var simdScore = Convert.ToDouble(resultObj);

                // Compare with scalar score - they should be reasonably close
                var scalarScore = QuadQualityHelper.ScoreQuad(quad);
                // If SIMD executed, scores should be very close
                Math.Abs(simdScore - scalarScore).Should().BeLessThan(1e-6);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is PlatformNotSupportedException) {
                // On platforms without AVX support, invoking SIMD path may throw
                // Accept this as a valid outcome on CI or non-AVX machines
                Assert.True(!Avx.IsSupported, "PlatformNotSupportedException was thrown but Avx.IsSupported is true");
            }
        }
    }
}
