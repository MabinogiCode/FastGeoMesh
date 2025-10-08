using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FastGeoMesh.Meshing.Exporters;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for the SVG exporter ensuring valid SVG output under various scenarios.</summary>
    public sealed class SvgExporterTests
    {
        /// <summary>Exports a simple top-view SVG and verifies file content.</summary>
        [Fact]
        public void ExportsTopViewSvg()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(0.5)
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            File.Exists(path).Should().BeTrue();
            var svg = File.ReadAllText(path);
            svg.Should().Contain("<svg", "SVG root element should exist");
            svg.Should().Contain("<line", "SVG should contain line elements for edges");
            File.Delete(path);
        }

        /// <summary>Exports SVG for a footprint with a hole and refinement settings.</summary>
        [Fact]
        public void SvgExporterHandlesHolesAndRefinement()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 6), new Vec2(0, 6) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var st = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            _ = st.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(10, 0)), 1.0);
            var opt = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithHoleRefinement(1.0, 0.75)
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_hole_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            File.Exists(path).Should().BeTrue();
            var svg = File.ReadAllText(path);
            svg.Should().Contain("<svg", "SVG root element should exist");
            svg.Should().Contain("<line", "SVG should contain line elements for edges");
            File.Delete(path);
        }

        /// <summary>Writes an SVG for a mesh without edges to ensure exporter does not crash.</summary>
        [Fact]
        public void SvgExporterWorksWithoutEdgesProducesEmptyLinesIfNoEdges()
        {
            var mesh = ImmutableMesh.Empty;
            mesh = mesh.AddPoint(new Vec3(0, 0, 0));
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_empty_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            File.Exists(path).Should().BeTrue();
            var svg = File.ReadAllText(path);
            svg.Should().Contain("<svg", "SVG root element should exist");
            File.Delete(path);
        }
    }
}
