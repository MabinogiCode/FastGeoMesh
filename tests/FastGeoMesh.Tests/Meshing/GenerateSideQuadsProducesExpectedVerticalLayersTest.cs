using FastGeoMesh.Application.Helpers.Meshing;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class GenerateSideQuadsProducesExpectedVerticalLayersTest
    {
        [Fact]
        public void Test()
        {
            var loop = new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) };
            var zLevels = new double[] { 0, 1, 2 };
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(2.0), TargetEdgeLengthZ = EdgeLength.From(1.0) };

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geometryService = provider.GetRequiredService<IGeometryService>();

            var quads = SideFaceMeshingHelper.GenerateSideQuads(loop, zLevels, options, outward: true, geometryService);
            quads.Should().HaveCount(12);
            quads.All(q => q.V0.Z is 0 or 1 or 2).Should().BeTrue();
        }
    }
}
