using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class BuildZLevelsExcludesPointsOutsideRangeTest
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 5, 15);
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 2));
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 18));
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 10));
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(5.0), Epsilon = Tolerance.From(1e-6) };
            var levels = MeshStructureHelper.BuildZLevels(5, 15, options, structure);
            levels.Should().Contain(10);
            levels.Should().NotContain(2);
            levels.Should().NotContain(18);
        }
    }
}
