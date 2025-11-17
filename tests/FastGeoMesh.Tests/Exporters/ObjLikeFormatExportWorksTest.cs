using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    public sealed class ObjLikeFormatExportWorksTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
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

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}_objlike.txt");

            TxtExporter.WriteObjLike(indexed, path);

            File.Exists(path).Should().BeTrue();
            var lines = File.ReadAllLines(path);

            bool hasVertexLine = lines.Any(l => l.StartsWith("v ") && l.Split(' ').Length == 4);
            hasVertexLine.Should().BeTrue();

            bool hasEdgeLine = lines.Any(l => l.StartsWith("l ") && l.Split(' ').Length == 3);
            hasEdgeLine.Should().BeTrue();

            bool hasFaceLine = lines.Any(l => l.StartsWith("f ") && l.Split(' ').Length >= 4);
            hasFaceLine.Should().BeTrue();

            var countOnlyLines = lines.Where(l => int.TryParse(l.Trim(), out _) && l.Trim().Length < 5);
            countOnlyLines.Should().BeEmpty();

            File.Delete(path);
        }
    }
}
