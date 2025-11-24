using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class IsNearAnySegmentDetectsProximityTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 8), new Vec2(0, 8) });
            var st = new PrismStructureDefinition(outer, 0, 3);
            st.Geometry.AddSegment(new Segment3D(new Vec3(0, 0, 0), new Vec3(8, 8, 0)));
            st.Geometry.AddSegment(new Segment3D(new Vec3(0, 8, 0), new Vec3(8, 0, 0)));

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<FastGeoMesh.Domain.Services.IGeometryService>();

            MeshStructureHelper.IsNearAnySegment(st, 4.1, 4.0, 0.3, geo).Should().BeTrue();
            MeshStructureHelper.IsNearAnySegment(st, 4.1, 4.0, 0.01, geo).Should().BeFalse();
        }
    }
}
