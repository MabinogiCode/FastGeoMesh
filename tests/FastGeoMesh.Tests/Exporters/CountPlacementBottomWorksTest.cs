using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.Exporters;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    /// <summary>
    /// Tests for class CountPlacementBottomWorksTest.
    /// </summary>
    public sealed class CountPlacementBottomWorksTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
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
                GenerateTopCap = false
            };
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}_bottom.txt");

            indexed.ExportTxt()
                .WithPoints("pt", CountPlacement.Bottom, false)
                .ToFile(path);

            File.Exists(path).Should().BeTrue();
            var lines = File.ReadAllLines(path);

            int.TryParse(lines[^1], out int count).Should().BeTrue();
            count.Should().BeGreaterThan(0);

            for (int i = 0; i < lines.Length - 1; i++)
            {
                lines[i].Should().StartWith("pt ");
            }

            File.Delete(path);
        }
    }
}
