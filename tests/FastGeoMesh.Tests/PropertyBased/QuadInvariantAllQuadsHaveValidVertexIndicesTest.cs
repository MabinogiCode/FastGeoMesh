using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.PropertyBased
{
    public sealed class QuadInvariantAllQuadsHaveValidVertexIndicesTest
    {
        [Theory]
        [InlineData(8, 6)]
        [InlineData(12, 10)]
        [InlineData(6, 4)]
        public void Test(int width, int height)
        {
            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height)
            });

            var structure = new PrismStructureDefinition(rect, 0, 2);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .Build()
                .UnwrapForTests();

            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            indexed.Quads.All(q => q.Item1 >= 0 && q.Item1 < indexed.Vertices.Count && q.Item2 >= 0 && q.Item2 < indexed.Vertices.Count && q.Item3 >= 0 && q.Item3 < indexed.Vertices.Count && q.Item4 >= 0 && q.Item4 < indexed.Vertices.Count && q.Item1 != q.Item2 && q.Item2 != q.Item3 && q.Item3 != q.Item4 && q.Item4 != q.Item1).Should().BeTrue();
        }
    }
}
