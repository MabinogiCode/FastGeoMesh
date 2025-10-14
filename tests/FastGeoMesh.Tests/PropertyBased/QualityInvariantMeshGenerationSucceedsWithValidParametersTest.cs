using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.PropertyBased
{
    public sealed class QualityInvariantMeshGenerationSucceedsWithValidParametersTest
    {
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void Test(int size)
        {
            if (size <= 0)
            {
                return;
            }

            var actualSize = Math.Max(size, 2);
            var square = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(actualSize, 0), new Vec2(actualSize, actualSize), new Vec2(0, actualSize) });
            var structure = new PrismStructureDefinition(square, 0, 1);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(Math.Max(actualSize * 0.8, 0.5))
                .WithTargetEdgeLengthZ(1.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .Build()
                .UnwrapForTests();

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            bool hasGeometry = indexed.Vertices.Count > 0 && indexed.Quads.Count > 0;
            bool validQualityScores = mesh.Quads.All(q => !q.QualityScore.HasValue || (q.QualityScore.Value >= 0.0 && q.QualityScore.Value <= 1.0));
            bool noNaNVertices = PropertyBasedTestHelper.ContainsNoNaNVertices(indexed.Vertices);

            (hasGeometry && validQualityScores && noNaNVertices).Should().BeTrue();
        }
    }
}
