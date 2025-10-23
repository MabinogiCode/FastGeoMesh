using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using Xunit;

namespace FastGeoMesh.Tests.Exporters {
    public sealed class ExportsSimpleRectPrismOBJTest {
        [Fact]
        public void Test() {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(TestGeometries.SmallRectangleWidth, 0),
                new Vec2(TestGeometries.SmallRectangleWidth, TestGeometries.SmallRectangleHeight),
                new Vec2(0, TestGeometries.SmallRectangleHeight)
            });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions {
                TargetEdgeLengthXY = TestMeshOptions.DefaultTargetEdgeLengthXY,
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.obj");
            ObjExporter.Write(im, path);
            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            Assert.Contains(lines, l => l.StartsWith("v ", System.StringComparison.Ordinal));
            Assert.Contains(lines, l => l.StartsWith("f ", System.StringComparison.Ordinal));
            File.Delete(path);
        }
    }
}
