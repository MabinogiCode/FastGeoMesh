using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    public sealed class SvgExporterHandlesHolesAndRefinementTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 6), new Vec2(0, 6) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var st = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            _ = st.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(10, 0)), 1.0);
            var opt = MesherOptions.CreateBuilder().WithTargetEdgeLengthXY(1.5).WithTargetEdgeLengthZ(1.0).WithHoleRefinement(1.0, 0.75).Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_hole_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            File.Exists(path).Should().BeTrue();
            var svg = File.ReadAllText(path);
            svg.Should().Contain("<svg");
            svg.Should().Contain("<line");
            File.Delete(path);
        }
    }
}
