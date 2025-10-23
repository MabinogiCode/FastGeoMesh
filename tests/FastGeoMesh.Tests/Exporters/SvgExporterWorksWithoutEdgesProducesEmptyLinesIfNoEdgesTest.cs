using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters {
    public sealed class SvgExporterWorksWithoutEdgesProducesEmptyLinesIfNoEdgesTest {
        [Fact]
        public void Test() {
            var mesh = ImmutableMesh.Empty;
            mesh = mesh.AddPoint(new Vec3(0, 0, 0));
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_empty_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            File.Exists(path).Should().BeTrue();
            var svg = File.ReadAllText(path);
            svg.Should().Contain("<svg");
            File.Delete(path);
        }
    }
}
