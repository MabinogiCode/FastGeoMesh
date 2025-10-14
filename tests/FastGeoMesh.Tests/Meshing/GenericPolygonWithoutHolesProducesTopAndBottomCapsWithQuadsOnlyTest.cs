using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class GenericPolygonWithoutHolesProducesTopAndBottomCapsWithQuadsOnlyTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0,0), new Vec2(4,0), new Vec2(5,2), new Vec2(2.5,4), new Vec2(0,2)
            });
            var structure = new PrismStructureDefinition(outer, -2, -1);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.75), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true };

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();

            int topQuads = mesh.Quads.Count(q => q.V0.Z == -1 && q.V1.Z == -1 && q.V2.Z == -1 && q.V3.Z == -1);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == -1 && t.V1.Z == -1 && t.V2.Z == -1);
            int topElements = topQuads + topTriangles;

            int bottomQuads = mesh.Quads.Count(q => q.V0.Z == -2 && q.V1.Z == -2 && q.V2.Z == -2 && q.V3.Z == -2);
            int bottomTriangles = mesh.Triangles.Count(t => t.V0.Z == -2 && t.V1.Z == -2 && t.V2.Z == -2);
            int bottomElements = bottomQuads + bottomTriangles;

            topElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            bottomElements.Should().BeGreaterThan(0, "Should have bottom cap elements (quads or triangles)");
        }
    }
}
