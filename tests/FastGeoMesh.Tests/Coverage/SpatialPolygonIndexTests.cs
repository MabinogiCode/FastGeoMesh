using System.Collections.ObjectModel;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage {
    public sealed class SpatialPolygonIndexTests {
        [Fact]
        public void ConstructorThrowsOnEmptyVertices() {
            var empty = new List<Vec2>();
            Assert.Throws<ArgumentException>(() => new SpatialPolygonIndex(empty));
        }

        [Fact]
        public void ConstructorThrowsOnInvalidGridResolution() {
            var square = new Vec2[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) };
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpatialPolygonIndex(square, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpatialPolygonIndex(square, -5));
        }

        [Fact]
        public void IsInsideDetectsInsideOutsideAndBoundary_ForArraySource() {
            var square = new Vec2[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 4), new Vec2(0, 4) };
            var idx = new SpatialPolygonIndex(square, gridResolution: 8);

            // clearly inside
            idx.IsInside(2.0, 2.0).Should().BeTrue();
            // outside
            idx.IsInside(-1.0, 2.0).Should().BeFalse();
            idx.IsInside(5.0, 2.0).Should().BeFalse();
            // on edge should be considered inside (PointInPolygon returns true for on-segment)
            idx.IsInside(0.0, 2.0).Should().BeTrue();
            idx.IsInside(4.0, 2.0).Should().BeTrue();
            idx.IsInside(2.0, 0.0).Should().BeTrue();
            idx.IsInside(2.0, 4.0).Should().BeTrue();
        }

        [Fact]
        public void IsInsideWorksForListSource() {
            var list = new List<Vec2> { new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 3), new Vec2(0, 3) };
            var idx = new SpatialPolygonIndex(list, gridResolution: 4);
            idx.IsInside(1.5, 1.5).Should().BeTrue();
            idx.IsInside(10, 10).Should().BeFalse();
        }

        [Fact]
        public void IsInsideForIReadOnlyListFallbacksUseToArrayPath() {
            // Create an IReadOnlyList implementation that is neither List<T> nor T[]
            var backing = new Vec2[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) };
            IReadOnlyList<Vec2> custom = new ReadOnlyCollection<Vec2>(backing);
            var idx = new SpatialPolygonIndex(custom, gridResolution: 4);
            idx.IsInside(1.0, 1.0).Should().BeTrue();
            idx.IsInside(-1.0, 1.0).Should().BeFalse();
        }
    }
}
