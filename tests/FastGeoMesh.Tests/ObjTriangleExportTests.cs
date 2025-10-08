using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for OBJ export functionality with triangle faces.</summary>
    public sealed class ObjTriangleExportTests
    {
        /// <summary>Tests that OBJ contains triangle faces when cap triangles are enabled.</summary>
        [Fact]
        public void ObjContainsTriangleFacesWhenCapTrianglesEnabled()
        {
            var outer = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(TestGeometries.SmallSquareSide, 0),
                new Vec2(TestGeometries.SmallSquareSide, TestGeometries.SmallRectangleHeight),
                new Vec2(3, TestGeometries.SmallRectangleHeight),
                new Vec2(3, TestGeometries.SmallRectangleHeight * 2),
                new Vec2(TestGeometries.SmallSquareSide, TestGeometries.SmallRectangleHeight * 2),
                new Vec2(TestGeometries.SmallSquareSide, TestGeometries.SmallSquareSide + 1),
                new Vec2(0, TestGeometries.SmallSquareSide + 1)
            });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(0.8),
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ,
                GenerateBottomCap = true,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.95, // Very high threshold to force triangles
                OutputRejectedCapTriangles = true
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            Assert.True(mesh.Triangles.Count > 0, "Expected triangles emitted");
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}obj_tri_{System.Guid.NewGuid():N}.obj");
            ObjExporter.Write(im, path);
            var lines = File.ReadAllLines(path);
            Assert.Contains(lines, l => l.StartsWith("f ", System.StringComparison.Ordinal) && l.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length == 4);
            File.Delete(path);
        }
    }
}
