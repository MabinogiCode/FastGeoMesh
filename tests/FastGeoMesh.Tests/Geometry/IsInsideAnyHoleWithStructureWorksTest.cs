using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class IsInsideAnyHoleWithStructureWorksTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);

            MeshStructureHelper.IsInsideAnyHole(structure, 5, 5).Should().BeTrue();
            MeshStructureHelper.IsInsideAnyHole(structure, 1, 1).Should().BeFalse();
            MeshStructureHelper.IsInsideAnyHole(structure, 4, 4).Should().BeTrue();
        }
    }
}
