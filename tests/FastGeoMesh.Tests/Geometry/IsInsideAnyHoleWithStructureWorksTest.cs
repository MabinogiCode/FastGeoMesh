using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geo = provider.GetRequiredService<FastGeoMesh.Domain.Services.IGeometryService>();

            MeshStructureHelper.IsInsideAnyHole(structure, 5, 5, geo).Should().BeTrue();
            MeshStructureHelper.IsInsideAnyHole(structure, 1, 1, geo).Should().BeFalse();
            MeshStructureHelper.IsInsideAnyHole(structure, 4, 4, geo).Should().BeTrue();
        }
    }
}
