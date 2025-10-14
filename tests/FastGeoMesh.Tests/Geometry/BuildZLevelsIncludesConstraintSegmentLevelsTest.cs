using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class BuildZLevelsIncludesConstraintSegmentLevelsTest
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 10);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(1, 0)), 3.5);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 1), new Vec2(1, 1)), 7.2);
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(5.0) };

            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);

            levels.Should().Contain(3.5);
            levels.Should().Contain(7.2);
        }
    }
}
