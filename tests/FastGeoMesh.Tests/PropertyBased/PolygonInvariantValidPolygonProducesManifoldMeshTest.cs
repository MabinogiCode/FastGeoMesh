using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.PropertyBased
{
    public sealed class PolygonInvariantValidPolygonProducesManifoldMeshTest
    {
        [Theory]
        [InlineData(5, 5, 2)]
        [InlineData(10, 8, 3)]
        [InlineData(15, 12, 4)]
        public void Test(int width, int height, int depth)
        {
            if (width <= 0 || height <= 0 || depth <= 0)
            {
                return;
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height)
            });

            var structure = new PrismStructureDefinition(rect, 0, depth);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(2.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .Build()
                .UnwrapForTests();

            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adjacency = indexed.BuildAdjacency();

            adjacency.NonManifoldEdges.Count.Should().Be(0);
        }
    }
}
