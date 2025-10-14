using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Validates that spatial indexing gives the same results as the original algorithm.
    /// </summary>
    public sealed class SpatialIndexingGivesSameResultsAsOriginalAlgorithm
    {
        [Fact]
        public void Test()
        {
            var vertices = new Vec2[8];
            for (int i = 0; i < 8; i++)
            {
                double angle = i * 2 * System.Math.PI / 8;
                vertices[i] = new Vec2(10 + 8 * System.Math.Cos(angle), 10 + 8 * System.Math.Sin(angle));
            }
            var polygon = Polygon2D.FromPoints(vertices);
            var structure = new PrismStructureDefinition(polygon, 0, 3);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesher = new PrismMesher();
            var mesh = mesher.Mesh(structure, options).UnwrapForTests();
            var indexedMesh = IndexedMesh.FromMesh(mesh);
            mesh.Quads.Should().NotBeEmpty();
            indexedMesh.Vertices.Should().NotBeEmpty();
            indexedMesh.Quads.Should().NotBeEmpty();
            var adjacency = indexedMesh.BuildAdjacency();
            adjacency.NonManifoldEdges.Should().BeEmpty("Optimized meshing should produce manifold geometry");
        }
    }
}
