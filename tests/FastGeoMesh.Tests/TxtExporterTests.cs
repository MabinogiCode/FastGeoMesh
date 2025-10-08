using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Exporters;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for the flexible TXT exporter with configurable format.
    /// </summary>
    public sealed class TxtExporterTests
    {
        /// <summary>
        /// Tests the flexible TXT exporter with custom prefixes and count placement.
        /// </summary>
        [Fact]
        public void FlexibleTxtExporterWorksWithCustomFormat()
        {
            // Arrange
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(1, 0),
                new Vec2(1, 1),
                new Vec2(0, 1)
            });
            var structure = new PrismStructureDefinition(poly, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.txt");

            // Act - Use the flexible builder pattern
            indexed.ExportTxt()
                .WithPoints("p", CountPlacement.Top, true)
                .WithEdges("e", CountPlacement.None, false)
                .WithQuads("q", CountPlacement.Bottom, true)
                .ToFile(path);

            // Assert
            File.Exists(path).Should().BeTrue();
            var lines = File.ReadAllLines(path);
            lines.Should().NotBeEmpty();

            // Should start with vertex count (because CountPlacement.Top)
            int.TryParse(lines[0], out int vertexCount).Should().BeTrue();
            vertexCount.Should().BeGreaterThan(0);

            // Should contain point lines with 'p' prefix
            bool hasPointLine = lines.Any(l => l.StartsWith("p ") && l.Split(' ').Length == 5);
            hasPointLine.Should().BeTrue();

            // Should contain edge lines with 'e' prefix (no count because CountPlacement.None)
            bool hasEdgeLine = lines.Any(l => l.StartsWith("e ") && l.Split(' ').Length == 3);
            hasEdgeLine.Should().BeTrue();

            // Should contain quad lines with 'q' prefix 
            bool hasQuadLine = lines.Any(l => l.StartsWith("q ") && l.Split(' ').Length == 6);
            hasQuadLine.Should().BeTrue();

            // Cleanup
            File.Delete(path);
        }

        /// <summary>
        /// Tests the OBJ-like format export method.
        /// </summary>
        [Fact]
        public void ObjLikeFormatExportWorks()
        {
            // Arrange
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(1, 0),
                new Vec2(1, 1),
                new Vec2(0, 1)
            });
            var structure = new PrismStructureDefinition(poly, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}_objlike.txt");

            // Act
            TxtExporter.WriteObjLike(indexed, path);

            // Assert
            File.Exists(path).Should().BeTrue();
            var lines = File.ReadAllLines(path);

            // OBJ-like format: no counts, prefixed lines
            bool hasVertexLine = lines.Any(l => l.StartsWith("v ") && l.Split(' ').Length == 4);
            hasVertexLine.Should().BeTrue();

            bool hasEdgeLine = lines.Any(l => l.StartsWith("l ") && l.Split(' ').Length == 3);
            hasEdgeLine.Should().BeTrue();

            bool hasFaceLine = lines.Any(l => l.StartsWith("f ") && l.Split(' ').Length >= 4);
            hasFaceLine.Should().BeTrue();

            // Should not contain count lines (numbers only)
            var countOnlyLines = lines.Where(l => int.TryParse(l.Trim(), out _) && l.Trim().Length < 5);
            countOnlyLines.Should().BeEmpty();

            // Cleanup
            File.Delete(path);
        }

        /// <summary>
        /// Tests count placement at bottom works correctly.
        /// </summary>
        [Fact]
        public void CountPlacementBottomWorks()
        {
            // Arrange
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(1, 0),
                new Vec2(1, 1),
                new Vec2(0, 1)
            });
            var structure = new PrismStructureDefinition(poly, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = false
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}_bottom.txt");

            // Act - Only export points with count at bottom
            indexed.ExportTxt()
                .WithPoints("pt", CountPlacement.Bottom, false)
                .ToFile(path);

            // Assert
            File.Exists(path).Should().BeTrue();
            var lines = File.ReadAllLines(path);

            // Last line should be the count
            int.TryParse(lines[^1], out int count).Should().BeTrue();
            count.Should().BeGreaterThan(0);

            // All other lines should start with "pt"
            for (int i = 0; i < lines.Length - 1; i++)
            {
                lines[i].Should().StartWith("pt ");
            }

            // Cleanup
            File.Delete(path);
        }
    }
}
