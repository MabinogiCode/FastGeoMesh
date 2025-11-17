using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class CapsAreRefinedNearHolesTest
    {
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(10, 4), new Vec2(12, 4), new Vec2(12, 6), new Vec2(10, 6) });
            var structure = new PrismStructureDefinition(outer, -1, 0).AddHole(hole);

            var baseOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();

            var baseMesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, baseOptions).UnwrapForTests();
            var baseTopQuads = baseMesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
            int baseQuadCount = baseTopQuads.Count;

            var refinedOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .WithHoleRefinement(0.5, 1.0)
                .Build().UnwrapForTests();

            var refinedMesh = TestMesherFactory.CreatePrismMesher().Mesh(structure, refinedOptions).UnwrapForTests();
            var refinedTopQuads = refinedMesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
            int refinedQuadCount = refinedTopQuads.Count;

            refinedQuadCount.Should().BeGreaterThan(baseQuadCount);
            refinedQuadCount.Should().BeGreaterThan((int)(baseQuadCount * 1.5));

            if (refinedTopQuads.Count > 0)
            {
                refinedTopQuads.Should().AllSatisfy(q =>
                {
                    q.V0.Should().NotBe(q.V1);
                    q.V1.Should().NotBe(q.V2);
                    q.V2.Should().NotBe(q.V3);
                    q.V3.Should().NotBe(q.V0);
                });
            }
        }
    }
}
