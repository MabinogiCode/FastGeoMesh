using System.IO;
using FastGeoMesh.Domain;
using FastGeoMesh.Application;
using FastGeoMesh.Meshing.Exporters;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests validating the GLTF exporter produces a valid glTF file with embedded buffer content.
    /// </summary>
    public sealed class GltfExporterTests
    {
        /// <summary>
        /// Generates a small prism mesh and verifies that the exported glTF file exists
        /// and contains expected JSON sections (asset, buffers, embedded base64 buffer).
        /// </summary>
        [Fact]
        public void ExportsGLTFWithEmbeddedBuffer()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(2, 0),
                new Vec2(2, 1),
                new Vec2(0, 1)
            });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = TestMeshOptions.DefaultTargetEdgeLengthXY,
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ
            };
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.gltf");
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
