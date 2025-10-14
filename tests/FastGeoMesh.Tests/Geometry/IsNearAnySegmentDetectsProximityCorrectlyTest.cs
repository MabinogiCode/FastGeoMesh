using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class IsNearAnySegmentDetectsProximityCorrectlyTest
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(polygon, 0, 5);
            structure.Geometry.AddSegment(new Segment3D(new Vec3(2, 2, 1), new Vec3(8, 2, 1)));

            MeshStructureHelper.IsNearAnySegment(structure, 5, 2, 0.1).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(structure, 5, 2.5, 0.6).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(structure, 5, 8, 1.0).Should().BeFalse();
        }
    }
}
