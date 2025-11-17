using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    public sealed class FlexibleTxtExporterWorksWithCustomFormatTest
    {
        [Fact]
        public void Test()
        {
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
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.txt");

            indexed.ExportTxt()
                .WithPoints("p", CountPlacement.Top, true)
                .WithEdges("e", CountPlacement.None, false)
                .WithQuads("q", CountPlacement.Bottom, true)
                .ToFile(path);

            File.Exists(path).Should().BeTrue();
            var lines = File.ReadAllLines(path);
            lines.Should().NotBeEmpty();

            int.TryParse(lines[0], out int vertexCount).Should().BeTrue();
            vertexCount.Should().BeGreaterThan(0);

            bool hasPointLine = lines.Any(l => l.StartsWith("p ") && l.Split(' ').Length == 5);
            hasPointLine.Should().BeTrue();

            bool hasEdgeLine = lines.Any(l => l.StartsWith("e ") && l.Split(' ').Length == 3);
            hasEdgeLine.Should().BeTrue();

            bool hasQuadLine = lines.Any(l => l.StartsWith("q ") && l.Split(' ').Length == 6);
            hasQuadLine.Should().BeTrue();

            File.Delete(path);
        }
    }
}
