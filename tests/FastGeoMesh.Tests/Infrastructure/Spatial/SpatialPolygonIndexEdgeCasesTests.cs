using System.Reflection;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Infrastructure.Spatial
{
    public class SpatialPolygonIndexEdgeCasesTests
    {
        private static TDelegate GetPrivateStatic<TDelegate>(Type type, string name) where TDelegate : Delegate
        {
            var mi = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
            mi.Should().NotBeNull($"Méthode privée {name} introuvable");
            return (TDelegate)mi!.CreateDelegate(typeof(TDelegate));
        }

        [Fact]
        public void SegmentIntersectsRectDetectsEachSide()
        {
            // Arrange
            var fn = GetPrivateStatic<Func<double, double, double, double, double, double, double, double, bool>>(typeof(SpatialPolygonIndex), "SegmentIntersectsRect");

            // Bottom edge intersection
            fn(0.0, -1.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0).Should().BeTrue();
            // Right edge intersection
            fn(2.0, 0.5, 0.5, 0.5, 0.0, 0.0, 1.0, 1.0).Should().BeTrue();
            // Top edge intersection
            fn(0.0, 2.0, 1.0, 0.0, 0.0, 0.0, 1.0, 1.0).Should().BeTrue();
            // Left edge intersection
            fn(-1.0, 0.5, 0.5, 0.5, 0.0, 0.0, 1.0, 1.0).Should().BeTrue();

            // No intersection
            fn(-2.0, -2.0, -1.5, -1.0, 0.0, 0.0, 1.0, 1.0).Should().BeFalse();
        }

        [Fact]
        public void SegmentsIntersectCoversCollinearAndTouching()
        {
            var fn = GetPrivateStatic<Func<double, double, double, double, double, double, double, double, bool>>(typeof(SpatialPolygonIndex), "SegmentsIntersect");

            // Collinear overlapping
            fn(0, 0, 2, 0, 1, 0, 3, 0).Should().BeTrue();
            // Collinear touching at endpoint
            fn(0, 0, 1, 0, 1, 0, 2, 0).Should().BeTrue();
            // Proper intersection (crossing)
            fn(0, 0, 2, 2, 0, 2, 2, 0).Should().BeTrue();
            // Near miss (parallel, separate)
            fn(0, 1, 2, 1, 0, 0, 2, 0).Should().BeFalse();
        }

        [Fact]
        public void DoesPolygonIntersectCellReturnsTrueWhenVertexInsideOrEdgeCrosses()
        {
            var helper = new GeometryService();
            var poly = new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 2), new Vec2(0, 2) };
            var index = new SpatialPolygonIndex(poly, helper, gridResolution: 8);

            // Use reflection to access the private method
            var mi = typeof(SpatialPolygonIndex).GetMethod("DoesPolygonIntersectCell", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Should().NotBeNull();

            // Cell that strictly contains the vertex (2,2)
            var result1 = (bool)mi!.Invoke(index, new object[] { 1.99, 1.99, 2.01, 2.01 })!;
            result1.Should().BeTrue();

            // Cell that the polygon edge crosses (near the right side)
            var result2 = (bool)mi!.Invoke(index, new object[] { 2.0, 0.75, 2.25, 1.25 })!;
            result2.Should().BeTrue();
        }

        [Fact]
        public void IsInsideStillWorksWithSparseGridAndPointsOnBoundary()
        {
            var helper = new GeometryService();
            var square = new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) };
            var index = new SpatialPolygonIndex(square, helper, gridResolution: 2);

            index.IsInside(0.0, 0.5).Should().BeTrue(); // boundary (left edge)
            index.IsInside(0.5, 0.5).Should().BeTrue(); // inside
            index.IsInside(1.5, 0.5).Should().BeFalse(); // outside
        }
    }
}
