using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Validates that object pooling does not affect mesh consistency.
    /// </summary>
    public sealed class ObjectPoolingDoesNotAffectMeshConsistency
    {
        [Fact]
        public void Test()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 8), new Vec2(0, 8)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 4);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(1.0),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesher = TestServiceProvider.CreatePrismMesher();
            var mesh1 = mesher.Mesh(structure, options).UnwrapForTests();
            var mesh2 = mesher.Mesh(structure, options).UnwrapForTests();
            var mesh3 = mesher.Mesh(structure, options).UnwrapForTests();
            mesh1.Quads.Count.Should().Be(mesh2.Quads.Count);
            mesh2.Quads.Count.Should().Be(mesh3.Quads.Count);
            mesh1.Triangles.Count.Should().Be(mesh2.Triangles.Count);
            mesh2.Triangles.Count.Should().Be(mesh3.Triangles.Count);
        }
    }
}
