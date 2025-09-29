using System;
using System.IO;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class ObjExporterTests
    {
        [Fact]
        public void ExportsSimpleRectPrismOBJ()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = new PrismMesher().Mesh(st, opt);
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_{Guid.NewGuid():N}.obj");
            ObjExporter.Write(im, path);
            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            Assert.Contains(lines, l => l.StartsWith("v ", StringComparison.Ordinal));
            Assert.Contains(lines, l => l.StartsWith("f ", StringComparison.Ordinal));
            File.Delete(path);
        }
    }
}
