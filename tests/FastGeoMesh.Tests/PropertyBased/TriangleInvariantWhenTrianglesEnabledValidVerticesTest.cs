using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.PropertyBased
{
    public sealed class TriangleInvariantWhenTrianglesEnabledValidVerticesTest
    {
        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        public void Test(int size)
        {
            if (size <= 2)
            {
                return;
            }

            var lShape = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(size, 0), new Vec2(size, size / 2), new Vec2(size / 2, size / 2), new Vec2(size / 2, size), new Vec2(0, size) });
            var structure = new PrismStructureDefinition(lShape, 0, 1);
            var options = MesherOptions.CreateBuilder().WithTargetEdgeLengthXY(1.0).WithTargetEdgeLengthZ(1.0).WithGenerateBottomCap(true).WithGenerateTopCap(true).WithRejectedCapTriangles(true).WithMinCapQuadQuality(0.9).Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            PropertyBasedTestHelper.AreTrianglesValid(mesh.Triangles).Should().BeTrue();
        }
    }
}
