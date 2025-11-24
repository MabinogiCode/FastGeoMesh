using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    public sealed class ExportsTopViewSvgTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = MesherOptions.CreateBuilder().WithTargetEdgeLengthXY(1.0).WithTargetEdgeLengthZ(0.5).Build().UnwrapForTests();
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            File.Exists(path).Should().BeTrue();
            var svg = File.ReadAllText(path);
            svg.Should().Contain("<svg");
            svg.Should().Contain("<line");
            File.Delete(path);
        }
    }
}
