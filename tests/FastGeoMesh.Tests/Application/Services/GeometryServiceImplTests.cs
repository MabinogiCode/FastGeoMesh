using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Application.Services
{
    public class GeometryServiceImplTests
    {
        private readonly GeometryServiceImpl _sut = new();

        [Fact]
        public void DistancePointToSegmentReturnsDistanceForDegenerateSegment()
        {
            var p = new Vec2(3, 4);
            var a = new Vec2(0, 0);
            var d = _sut.DistancePointToSegment(p, a, a);
            d.Should().BeApproximately(5.0, 1e-12);
        }

        [Fact]
        public void DistancePointToSegmentProjectsBeforeAReturnsDistanceToA()
        {
            var p = new Vec2(-1, 1);
            var a = new Vec2(0, 0);
            var b = new Vec2(2, 0);
            var d = _sut.DistancePointToSegment(p, a, b);
            d.Should().BeApproximately(Math.Sqrt(2), 1e-12);
        }

        [Fact]
        public void DistancePointToSegmentProjectsAfterBReturnsDistanceToB()
        {
            var p = new Vec2(3, 4);
            var a = new Vec2(0, 0);
            var b = new Vec2(2, 0);
            var d = _sut.DistancePointToSegment(p, a, b);
            d.Should().BeApproximately(Math.Sqrt((3 - 2) * (3 - 2) + 4 * 4), 1e-12);
        }

        [Fact]
        public void DistancePointToSegmentProjectsInsideReturnsPerpendicularDistance()
        {
            var p = new Vec2(1, 2);
            var a = new Vec2(0, 0);
            var b = new Vec2(2, 0);
            var d = _sut.DistancePointToSegment(p, a, b);
            d.Should().BeApproximately(2.0, 1e-12);
        }

        [Fact]
        public void PointInPolygonReturnsExpectedForSimpleSquare()
        {
            var square = new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) };
            _sut.PointInPolygon(square, 1, 1).Should().BeTrue();
            _sut.PointInPolygon(square, -1, -1).Should().BeFalse();
        }

        [Fact]
        public void BatchPointInPolygonMismatchedLengthsThrows()
        {
            var square = new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) };
            var points = new[] { new Vec2(0.5, 0.5), new Vec2(2, 2) };
            var results = new bool[1];
            Action act = () => _sut.BatchPointInPolygon(square, points, results);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AreasAreCorrect()
        {
            var tri = _sut.TriangleArea(new Vec2(0, 0), new Vec2(2, 0), new Vec2(0, 2));
            tri.Should().BeApproximately(2.0, 1e-12);

            var quad = _sut.QuadArea((new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 1), new Vec2(0, 1)));
            quad.Should().BeApproximately(2.0, 1e-12);

            var poly = _sut.PolygonArea(new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 1), new Vec2(0, 1) });
            poly.Should().BeApproximately(2.0, 1e-12);
        }

        [Fact]
        public void DistanceAndSquaredAreConsistent()
        {
            var a = new Vec2(0, 0);
            var b = new Vec2(3, 4);
            _sut.Distance(a, b).Should().BeApproximately(5.0, 1e-12);
            _sut.DistanceSquared(a, b).Should().Be(25.0);
        }

        [Fact]
        public void CentroidAndNormalizeAndClampWork()
        {
            _sut.Centroid(Array.Empty<Vec2>()).Should().Be(Vec2.Zero);
            _sut.Centroid(new[] { new Vec2(0, 0), new Vec2(2, 2) })
                .Should().Be(new Vec2(1, 1));

            _sut.Normalize(new Vec2(0, 0)).Should().Be(Vec2.Zero);
            var n = _sut.Normalize(new Vec2(3, 4));
            n.X.Should().BeApproximately(0.6, 1e-12);
            n.Y.Should().BeApproximately(0.8, 1e-12);

            _sut.Clamp(5, 0, 10).Should().Be(5);
            _sut.Clamp(-1, 0, 10).Should().Be(0);
            _sut.Clamp(11, 0, 10).Should().Be(10);
        }

        [Fact]
        public void IsConvexTrueAndFalseCases()
        {
            var convex = (new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2));
            _sut.IsConvex(convex).Should().BeTrue();

            // Simple concave quad (arrow shape)
            var concave = (new Vec2(0, 0), new Vec2(2, 0), new Vec2(1, 0.2), new Vec2(0, 2));
            _sut.IsConvex(concave).Should().BeFalse();
        }

        [Fact]
        public void LerpVariantsAndVec2OverloadAreCovered()
        {
            // Lerp(Vec2)
            var v = _sut.Lerp(new Vec2(0, 0), new Vec2(2, 2), 0.25);
            v.Should().Be(new Vec2(0.5, 0.5));

            // Lerp(Vec3)
            var v3 = _sut.Lerp(new Vec3(0, 0, 0), new Vec3(2, 4, 6), 0.5);
            v3.Should().Be(new Vec3(1, 2, 3));

            // LerpScalar
            _sut.LerpScalar(2, 10, 0.25).Should().Be(4);

            // PointInPolygon(Vec2 overload)
            var square = new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) };
            _sut.PointInPolygon(square, new Vec2(1, 1)).Should().BeTrue();
        }
    }
}
