using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class BuildZLevelsCreatesCorrectNumberOfLevelsTest.
    /// </summary>
    public sealed class BuildZLevelsCreatesCorrectNumberOfLevelsTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            var structure = new PrismStructureDefinition(polygon, 0, 10);
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(2.0) };

            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);

            levels.Should().NotBeEmpty();
            levels[0].Should().Be(0);
            levels[^1].Should().Be(10);
            levels.Should().HaveCountGreaterThan(2);
            levels.Should().BeInAscendingOrder();
        }
    }
}
