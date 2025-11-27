using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class IsNearAnyHoleDetectsProximityCorrectlyTest.
    /// </summary>
    public sealed class IsNearAnyHoleDetectsProximityCorrectlyTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(4, 4), new Vec2(6, 4), new Vec2(6, 6), new Vec2(4, 6) });
            var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<FastGeoMesh.Domain.Services.IGeometryService>();

            MeshStructureHelper.IsNearAnyHole(structure, 5, 5, 0.5, geo).Should().BeFalse();
            MeshStructureHelper.IsNearAnyHole(structure, 3.5, 5, 0.6, geo).Should().BeTrue();
            MeshStructureHelper.IsNearAnyHole(structure, 1, 1, 0.5, geo).Should().BeFalse();
        }
    }
}
