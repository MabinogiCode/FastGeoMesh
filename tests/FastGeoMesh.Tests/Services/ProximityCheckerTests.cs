using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Services
{
    public class ProximityCheckerTests
    {
        private readonly ProximityChecker _checker = new ProximityChecker();
        private readonly GeometryService _geometryService = new GeometryService();
        private readonly PrismStructureDefinition _structure;

        public ProximityCheckerTests()
        {
            var hole = new Polygon2D(new[]
            {
                new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4)
            });
            var segment = new Segment3D(new Vec3(2, 2, 0), new Vec3(8, 2, 10));
            _structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            ).AddHole(hole);
            _structure.Geometry.AddSegment(segment);
        }

        [Fact]
        public void IsNearAnyHoleWithNullStructureThrowsArgumentNullException()
        {
            Action act = () => _checker.IsNearAnyHole((PrismStructureDefinition)null!, 0, 0, 1, _geometryService);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsNearAnyHoleWithNullGeometryServiceThrowsArgumentNullException()
        {
            Action act = () => _checker.IsNearAnyHole(_structure, 0, 0, 1, (GeometryService)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsNearAnySegmentWithNullStructureThrowsArgumentNullException()
        {
            Action act = () => _checker.IsNearAnySegment((PrismStructureDefinition)null!, 0, 0, 1, _geometryService);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsNearAnySegmentWithNullGeometryServiceThrowsArgumentNullException()
        {
            Action act = () => _checker.IsNearAnySegment(_structure, 0, 0, 1, (GeometryService)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsInsideAnyHoleWithNullStructureThrowsArgumentNullException()
        {
            Action act = () => _checker.IsInsideAnyHole((PrismStructureDefinition)null!, 0, 0, _geometryService);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsInsideAnyHoleWithNullGeometryServiceThrowsArgumentNullException()
        {
            Action act = () => _checker.IsInsideAnyHole(_structure, 0, 0, (GeometryService)null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsNearAnyHoleReturnsTrueWhenPointIsNearHoleBoundary()
        {
            var result = _checker.IsNearAnyHole(_structure, 1.9, 3.0, band: 0.2, _geometryService);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsNearAnyHoleReturnsFalseWhenPointIsFarFromHoles()
        {
            var result = _checker.IsNearAnyHole(_structure, 8.0, 8.0, band: 0.2, _geometryService);
            result.Should().BeFalse();
        }

        [Fact]
        public void IsNearAnySegmentReturnsTrueWhenPointIsNearSegment()
        {
            var result = _checker.IsNearAnySegment(_structure, 5.0, 2.1, band: 0.2, _geometryService);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsNearAnySegmentReturnsFalseWhenPointIsFarFromSegments()
        {
            var result = _checker.IsNearAnySegment(_structure, 5.0, 8.0, band: 0.2, _geometryService);
            result.Should().BeFalse();
        }

        [Fact]
        public void IsInsideAnyHoleReturnsTrueWhenPointIsInsideHole()
        {
            var result = _checker.IsInsideAnyHole(_structure, 3.0, 3.0, _geometryService);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInsideAnyHoleReturnsFalseWhenPointIsOutsideAllHoles()
        {
            var result = _checker.IsInsideAnyHole(_structure, 8.0, 8.0, _geometryService);
            result.Should().BeFalse();
        }
    }
}
