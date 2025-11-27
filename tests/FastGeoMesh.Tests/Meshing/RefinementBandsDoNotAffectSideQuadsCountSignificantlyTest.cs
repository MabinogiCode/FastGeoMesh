using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Tests for class RefinementBandsDoNotAffectSideQuadsCountSignificantlyTest.
    /// </summary>
    public sealed class RefinementBandsDoNotAffectSideQuadsCountSignificantlyTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(rect, 0, 4);
            var baseOpt = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();
            var refinedOpt = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .WithHoleRefinement(1.25, 2.0)
                .Build().UnwrapForTests();
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var baseMesh = mesher.Mesh(structure, baseOpt).UnwrapForTests();
            var refinedMesh = mesher.Mesh(structure, refinedOpt).UnwrapForTests();
            int CountSideQuads(ImmutableMesh m) => m.Quads.Count(q => !(q.V0.Z == q.V1.Z && q.V1.Z == q.V2.Z && q.V2.Z == q.V3.Z));
            int sidesBase = CountSideQuads(baseMesh);
            int sidesRefined = CountSideQuads(refinedMesh);
            sidesRefined.Should().BeInRange((int)(sidesBase * 0.9), (int)(sidesBase * 1.1));
        }
    }
}
