using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class IsInsideAnyHoleWithSpatialIndicesWorksTest
    {
        [Fact]
        public void Test()
        {
            var hole1 = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var hole2 = Polygon2D.FromPoints(new[] { new Vec2(6, 6), new Vec2(8, 6), new Vec2(8, 8), new Vec2(6, 8) });

            var holeIndices = new SpatialPolygonIndex[]
            {
                new(hole1.Vertices),
                new(hole2.Vertices)
            };

            MeshStructureHelper.IsInsideAnyHole(holeIndices, 3, 3).Should().BeTrue();
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 7, 7).Should().BeTrue();
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 1, 1).Should().BeFalse();
            MeshStructureHelper.IsInsideAnyHole(holeIndices, 5, 5).Should().BeFalse();
        }
    }
}
