using System;
using System.IO;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Structures;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class SvgExporterTests
    {
        [Fact]
        public void ExportsTopViewSvg()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new PrismMesher().Mesh(st, opt);
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            Assert.True(File.Exists(path));
            var svg = File.ReadAllText(path);
            Assert.Contains("<svg", svg, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<line", svg, StringComparison.OrdinalIgnoreCase);
            File.Delete(path);
        }

        [Fact]
        public void SvgExporterHandlesHolesAndRefinement()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 6), new Vec2(0, 6) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4) });
            var st = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            _ = st.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(10, 0)), 1.0);
            var opt = new MesherOptions { TargetEdgeLengthXY = 1.5, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true, HoleRefineBand = 1.0, TargetEdgeLengthXYNearHoles = 0.75 };
            var mesh = new PrismMesher().Mesh(st, opt);
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_hole_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            Assert.True(File.Exists(path));
            var svg = File.ReadAllText(path);
            Assert.Contains("<svg", svg, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<line", svg, StringComparison.OrdinalIgnoreCase);
            File.Delete(path);
        }

        [Fact]
        public void SvgExporterWorksWithoutEdgesProducesEmptyLinesIfNoEdges()
        {
            var mesh = new Mesh();
            mesh.AddPoint(new Vec3(0, 0, 0));
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_empty_{Guid.NewGuid():N}.svg");
            SvgExporter.Write(im, path);
            Assert.True(File.Exists(path));
            var svg = File.ReadAllText(path);
            Assert.Contains("<svg", svg, StringComparison.OrdinalIgnoreCase);
            File.Delete(path);
        }
    }
}
