using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Exporters;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests validating Legacy exporter output contains expected format structure.
    /// </summary>
    public sealed class LegacyExporterTests
    {
        /// <summary>
        /// Exports a simple rectangular prism to Legacy format and verifies basic structural format.
        /// </summary>
        [Fact]
        public void ExportsSimpleRectPrismLegacy()
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
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.txt");
            
            LegacyExporter.Write(im, path);
            
            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            
            // Verify legacy format structure
            Assert.True(lines.Length >= 3, "Should have at least vertex count, edge count, and quad count lines");
            
            // First line should be vertex count (number)
            Assert.True(int.TryParse(lines[0], out int vertexCount), "First line should be vertex count");
            Assert.True(vertexCount > 0, "Should have vertices");
            
            // Should contain lines with vertex data (index x y z)
            bool hasVertexLine = false;
            bool hasEdgeLine = false;
            bool hasQuadLine = false;
            
            for (int i = 1; i < lines.Length && i <= vertexCount; i++)
            {
                var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4 && int.TryParse(parts[0], out _) && 
                    double.TryParse(parts[1], out _) && 
                    double.TryParse(parts[2], out _) && 
                    double.TryParse(parts[3], out _))
                {
                    hasVertexLine = true;
                    break;
                }
            }
            
            // Look for edge and quad lines pattern
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 && int.TryParse(parts[0], out _) && 
                    int.TryParse(parts[1], out _) && int.TryParse(parts[2], out _))
                {
                    hasEdgeLine = true;
                }
                if (parts.Length == 5 && int.TryParse(parts[0], out _) && 
                    int.TryParse(parts[1], out _) && int.TryParse(parts[2], out _) && 
                    int.TryParse(parts[3], out _) && int.TryParse(parts[4], out _))
                {
                    hasQuadLine = true;
                }
            }
            
            Assert.True(hasVertexLine, "Should contain vertex lines in format 'index x y z'");
            Assert.True(hasEdgeLine, "Should contain edge lines in format 'index v0 v1'");
            Assert.True(hasQuadLine, "Should contain quad lines in format 'index v0 v1 v2 v3'");
            
            File.Delete(path);
        }

        /// <summary>
        /// Exports using legacy naming convention.
        /// </summary>
        [Fact]
        public void ExportsWithLegacyNamingConvention()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(2, 0),
                new Vec2(2, 2),
                new Vec2(0, 2)
            });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = TestMeshOptions.DefaultTargetEdgeLengthXY,
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);
            
            string tempDir = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}");
            
            LegacyExporter.WriteWithLegacyName(im, tempDir);
            
            string expectedPath = Path.Combine(tempDir, "0_maill.txt");
            Assert.True(File.Exists(expectedPath), "Should create 0_maill.txt in specified directory");
            
            // Cleanup
            Directory.Delete(tempDir, true);
        }

        /// <summary>
        /// Tests round-trip: export then import Legacy format.
        /// </summary>
        [Fact]
        public void LegacyFormatRoundTripPreservesData()
        {
            // Arrange - Create a simple mesh
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(1, 0),
                new Vec2(1, 1),
                new Vec2(0, 1)
            });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesh = new PrismMesher().Mesh(st, opt).UnwrapForTests();
            var originalIndexed = IndexedMesh.FromMesh(mesh);
            
            string tempFile = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.txt");
            
            // Act - Export then import
            LegacyExporter.Write(originalIndexed, tempFile);
            var importedIndexed = IndexedMeshFileHelper.ReadCustomTxt(tempFile);
            
            // Assert - Data should be preserved
            Assert.Equal(originalIndexed.Vertices.Count, importedIndexed.Vertices.Count);
            Assert.Equal(originalIndexed.Edges.Count, importedIndexed.Edges.Count);
            Assert.Equal(originalIndexed.Quads.Count, importedIndexed.Quads.Count);
            
            // Cleanup
            File.Delete(tempFile);
        }
    }
}
