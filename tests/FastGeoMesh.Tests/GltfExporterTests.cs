using System;
using System.IO;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Geometry;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class GltfExporterTests
    {
        [Fact]
        public void ExportsGLTFWithEmbeddedBuffer()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 1), new Vec2(0, 1) });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5 };
            var mesh = new PrismMesher().Mesh(st, opt);
            var im = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"fgm_test_{Guid.NewGuid():N}.gltf");
            GltfExporter.Write(im, path);
            Assert.True(File.Exists(path));

            var json = File.ReadAllText(path);
            Assert.Contains("\"asset\"", json, StringComparison.Ordinal);
            Assert.Contains("\"buffers\"", json, StringComparison.Ordinal);
            Assert.Contains("data:application/octet-stream;base64,", json, StringComparison.Ordinal);
            File.Delete(path);
        }
    }
}
