using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class IsNearAnySegmentDetectsProximityCorrectlyTest.
    /// </summary>
    public sealed class IsNearAnySegmentDetectsProximityCorrectlyTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(polygon, 0, 5);
            structure.Geometry.AddSegment(new Segment3D(new Vec3(2, 2, 1), new Vec3(8, 2, 1)));

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<FastGeoMesh.Domain.Services.IGeometryService>();

            MeshStructureHelper.IsNearAnySegment(structure, 5, 2, 0.1, geo).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(structure, 5, 2.5, 0.6, geo).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(structure, 5, 8, 1.0, geo).Should().BeFalse();
        }
    }
}
