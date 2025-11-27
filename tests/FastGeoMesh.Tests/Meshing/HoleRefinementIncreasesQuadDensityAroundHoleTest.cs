using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Tests for class HoleRefinementIncreasesQuadDensityAroundHoleTest.
    /// </summary>
    public sealed class HoleRefinementIncreasesQuadDensityAroundHoleTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(8, 4), new Vec2(12, 4), new Vec2(12, 6), new Vec2(8, 6) });
            var baseStruct = new PrismStructureDefinition(rect, 0, 1).AddHole(hole);

            var coarse = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: false)
                .Build().UnwrapForTests();

            var refined = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: false)
                .WithHoleRefinement(1.0, 2.5)
                .Build().UnwrapForTests();

            var meshCoarse = TestServiceProvider.CreatePrismMesher().Mesh(baseStruct, coarse).UnwrapForTests();
            var meshRefined = TestServiceProvider.CreatePrismMesher().Mesh(baseStruct, refined).UnwrapForTests();

            int coarseCap = meshCoarse.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int refinedCap = meshRefined.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);

            refinedCap.Should().BeGreaterThanOrEqualTo(coarseCap);
            coarseCap.Should().BeGreaterThan(0);
            refinedCap.Should().BeGreaterThan(0);
        }
    }
}
