using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class PrismMesherSideQuadsAreGeneratedCcwTest
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(10.0)
                .WithTargetEdgeLengthZ(20.0)
                .Build().UnwrapForTests();
            var mesher = new PrismMesher();
            var result = mesher.Mesh(structure, options);
            result.IsSuccess.Should().BeTrue();
            var mesh = result.Value;
            mesh.Quads.Should().NotBeEmpty();
            foreach (var q in mesh.Quads)
            {
                var ax = q.V1.X - q.V0.X;
                var ay = q.V1.Y - q.V0.Y;
                var bx = q.V2.X - q.V1.X;
                var by = q.V2.Y - q.V1.Y;
                double cross = ax * by - ay * bx;
                cross.Should().BeGreaterThanOrEqualTo(0);
            }
        }
    }
}
