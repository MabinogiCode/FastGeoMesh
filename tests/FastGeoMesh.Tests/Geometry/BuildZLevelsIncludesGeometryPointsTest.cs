using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class BuildZLevelsIncludesGeometryPointsTest
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 10);
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 2.5));
            structure.Geometry.AddPoint(new Vec3(0.5, 0.5, 8.1));
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(5.0) };

            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);

            levels.Should().Contain(2.5);
            levels.Should().Contain(8.1);
        }
    }
}
