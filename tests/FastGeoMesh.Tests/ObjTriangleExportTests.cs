using System.IO;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
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
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 2), new Vec2(3, 2), new Vec2(3, 4), new Vec2(5, 4), new Vec2(5, 6), new Vec2(0, 6) });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 0.8,
                TargetEdgeLengthZ = 0.5,
                GenerateBottomCap = true,
                GenerateTopCap = false,
                MinCapQuadQuality = 0.95,
                OutputRejectedCapTriangles = true
            };
            var mesh = new PrismMesher().Mesh(structure, options);
            Assert.True(mesh.Triangles.Count > 0, "Expected triangles emitted");
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            string path = Path.Combine(Path.GetTempPath(), $"fgm_obj_tri_{System.Guid.NewGuid():N}.obj");
            ObjExporter.Write(im, path);
            var lines = File.ReadAllLines(path);
            Assert.Contains(lines, l => l.StartsWith("f ", System.StringComparison.Ordinal) && l.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length == 4);
            File.Delete(path);
        }
    }
}
