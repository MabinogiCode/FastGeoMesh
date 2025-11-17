using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class CapsAreGeneratedWithExpectedQuadCountForAxisAlignedRectangleTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(1.0), TargetEdgeLengthZ = EdgeLength.From(0.5), GenerateBottomCap = true, GenerateTopCap = true };
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            int capCount = mesh.Quads.Count(q => (MathUtil.NearlyEqual(q.V0.Z, -10, options.Epsilon) && MathUtil.NearlyEqual(q.V1.Z, -10, options.Epsilon) && MathUtil.NearlyEqual(q.V2.Z, -10, options.Epsilon) && MathUtil.NearlyEqual(q.V3.Z, -10, options.Epsilon)) || (MathUtil.NearlyEqual(q.V0.Z, 10, options.Epsilon) && MathUtil.NearlyEqual(q.V1.Z, 10, options.Epsilon) && MathUtil.NearlyEqual(q.V2.Z, 10, options.Epsilon) && MathUtil.NearlyEqual(q.V3.Z, 10, options.Epsilon)));
            _ = capCount.Should().Be(200);
        }
    }
}
