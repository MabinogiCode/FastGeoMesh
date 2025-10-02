using FastGeoMesh.Geometry;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for spatial helper functions determining proximity to holes and segments in prism structures.
    /// </summary>
    public sealed class MeshStructureHelperDistanceTests
    {
        private static PrismStructureDefinition BuildStructure()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            return new PrismStructureDefinition(outer, 0, 5).AddHole(hole);
        }

        /// <summary>
        /// Validates near-hole detection with varying influence bands and points near hole boundaries.
        /// </summary>
        [Theory]
        [InlineData(4, 4, 0.5, true)]
        [InlineData(5, 4, 0.1, true)]
        [InlineData(5, 5, 1.2, true)]
        [InlineData(3.9, 5, 0.05, false)]
        [InlineData(3.9, 5, 0.2, true)]
        public void IsNearAnyHoleThreshold(double x, double y, double band, bool expected)
        {
            var st = BuildStructure();
            MeshStructureHelper.IsNearAnyHole(st, x, y, band).Should().Be(expected);
        }

        /// <summary>
        /// Ensures near-segment detection works for internal geometry segments and respects proximity thresholds.
        /// </summary>
        [Fact]
        public void IsNearAnySegmentDetectsProximity()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 8), new Vec2(0, 8) });
            var st = new PrismStructureDefinition(outer, 0, 3);
            st.Geometry.AddSegment(new Segment3D(new Vec3(0, 0, 0), new Vec3(8, 8, 0)));
            st.Geometry.AddSegment(new Segment3D(new Vec3(0, 8, 0), new Vec3(8, 0, 0)));
            MeshStructureHelper.IsNearAnySegment(st, 4.1, 4.0, 0.3).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(st, 4.1, 4.0, 0.01).Should().BeFalse();
        }

        /// <summary>
        /// Compares indexed vs non-indexed hole containment checks across a sample grid to ensure parity.
        /// </summary>
        [Fact]
        public void IsInsideAnyHoleIndexedMatchesNonIndexed()
        {
            var st = BuildStructure();
            var holeIdx = st.Holes.Select(h => new SpatialPolygonIndex(h.Vertices)).ToArray();
            for (double x = 0; x <= 10; x += 0.5)
            {
                for (double y = 0; y <= 10; y += 0.5)
                {
                    bool a = MeshStructureHelper.IsInsideAnyHole(st, x, y);
                    bool b = MeshStructureHelper.IsInsideAnyHole(holeIdx, x, y);
                    a.Should().Be(b);
                }
            }
        }
    }
}
