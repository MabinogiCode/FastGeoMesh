using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class ConstraintZLevelIsPresentInSideQuadsTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(10, 0)), 2.5);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(5.0), TargetEdgeLengthZ = EdgeLength.From(3.0), GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();
            bool hasZ = mesh.Quads.Any(q => MathUtil.NearlyEqual(q.V0.Z, 2.5, options.Epsilon) || MathUtil.NearlyEqual(q.V1.Z, 2.5, options.Epsilon) || MathUtil.NearlyEqual(q.V2.Z, 2.5, options.Epsilon) || MathUtil.NearlyEqual(q.V3.Z, 2.5, options.Epsilon));
            _ = hasZ.Should().BeTrue();
        }
    }
}
