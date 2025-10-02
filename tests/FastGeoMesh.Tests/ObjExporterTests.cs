using System.IO; // Needed for file operations
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Structures;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests validating OBJ exporter output contains expected vertex and face definitions.
    /// </summary>
    public sealed class ObjExporterTests
    {
        /// <summary>
        /// Exports a simple rectangular prism to OBJ and verifies basic structural lines exist.
        /// </summary>
        [Fact]
        public void ExportsSimpleRectPrismOBJ()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(TestGeometries.SmallRectangleWidth, 0),
                new Vec2(TestGeometries.SmallRectangleWidth, TestGeometries.SmallRectangleHeight),
                new Vec2(0, TestGeometries.SmallRectangleHeight)
            });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = TestMeshOptions.DefaultTargetEdgeLengthXY,
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesh = new PrismMesher().Mesh(st, opt);
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.obj");
            ObjExporter.Write(im, path);
            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            Assert.Contains(lines, l => l.StartsWith("v ", StringComparison.Ordinal));
            Assert.Contains(lines, l => l.StartsWith("f ", StringComparison.Ordinal));
            File.Delete(path);
        }
    }
}
