using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.PropertyBased
{
    public sealed class EdgeLengthConstraintEdgesRespectMaximumTargetTest
    {
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void Test(int targetLength)
        {
            if (targetLength <= 0)
            {
                return;
            }

            var rect = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(rect, 0, 4);
            var options = MesherOptions.CreateBuilder().WithTargetEdgeLengthXY(targetLength).WithTargetEdgeLengthZ(targetLength).WithGenerateBottomCap(false).WithGenerateTopCap(false).Build().UnwrapForTests();
            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            var sideQuads = mesh.Quads.Where(q => !PropertyBasedTestHelper.IsCapQuad(q)).ToList();
            if (sideQuads.Count == 0)
            {
                return;
            }
            PropertyBasedTestHelper.DoQuadEdgesRespectMaxLength(sideQuads, targetLength).Should().BeTrue();
        }
    }
}
