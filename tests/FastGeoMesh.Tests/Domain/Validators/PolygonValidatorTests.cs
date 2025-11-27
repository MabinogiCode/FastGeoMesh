using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Validators;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Domain.Validators
{
    public class PolygonValidatorTests
    {
        [Fact]
        public void SignedAreaIsPositiveForCCWAndNegativeForCW()
        {
            var ccw = new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 1), new Vec2(0, 1) };
            var cw = new[] { new Vec2(0, 0), new Vec2(0, 1), new Vec2(2, 1), new Vec2(2, 0) };

            PolygonValidator.SignedArea(ccw).Should().BeGreaterThan(0);
            PolygonValidator.SignedArea(cw).Should().BeLessThan(0);
        }

        [Fact]
        public void OrientReturnsZeroForCollinearWithTolerance()
        {
            var a = new Vec2(0, 0);
            var b = new Vec2(1, 1);
            var c = new Vec2(2, 2 + 1e-12);
            PolygonValidator.Orient(a, b, c, 1e-6).Should().Be(0);
        }

        [Fact]
        public void OrientReturnsCorrectSignForCWAndCCW()
        {
            var a = new Vec2(0, 0);
            var b = new Vec2(1, 0);
            var cCcw = new Vec2(1, 1);
            var cCw = new Vec2(1, -1);

            PolygonValidator.Orient(a, b, cCcw, 0).Should().Be(1);
            PolygonValidator.Orient(a, b, cCw, 0).Should().Be(-1);
        }
    }
}
