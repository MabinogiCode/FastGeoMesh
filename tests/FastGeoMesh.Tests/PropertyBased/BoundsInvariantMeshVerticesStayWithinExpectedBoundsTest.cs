using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.PropertyBased
{
    public sealed class BoundsInvariantMeshVerticesStayWithinExpectedBoundsTest
    {
        [Theory]
        [InlineData(6, 4, 3)]
        [InlineData(10, 8, 5)]
        public void Test(int width, int height, int depth)
        {
            if (width <= 0 || height <= 0 || depth <= 0)
            {
                return;
            }

            var rect = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height) });
            var structure = new PrismStructureDefinition(rect, 0, depth);
            var options = MesherOptions.CreateBuilder().WithTargetEdgeLengthXY(2.0).WithTargetEdgeLengthZ(2.0).WithGenerateBottomCap(false).WithGenerateTopCap(false).Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
            PropertyBasedTestHelper.AreVerticesWithinBounds(indexed.Vertices, 0, width, 0, height, 0, depth).Should().BeTrue();
        }
    }
}
