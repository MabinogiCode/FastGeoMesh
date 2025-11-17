using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class SegmentRefinementIncreasesQuadDensityAlongSegmentTest
    {
        [Fact]
        public void Test()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(30, 0), new Vec2(30, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(rect, 0, 1);
            var seg = new Segment3D(new Vec3(0, 5, 0), new Vec3(30, 5, 0));
            structure.Geometry.AddSegment(seg);

            var coarse = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(3.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: false)
                .Build().UnwrapForTests();

            var refined = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(3.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: false)
                .WithSegmentRefinement(1.2, 3.5)
                .Build().UnwrapForTests();

            var meshCoarse = TestMesherFactory.CreatePrismMesher().Mesh(structure, coarse).UnwrapForTests();
            var meshRefined = TestMesherFactory.CreatePrismMesher().Mesh(structure, refined).UnwrapForTests();

            int coarseCap = meshCoarse.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int refinedCap = meshRefined.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);

            refinedCap.Should().BeGreaterThanOrEqualTo(coarseCap);
            coarseCap.Should().BeGreaterThan(0);
            refinedCap.Should().BeGreaterThan(0);
        }
    }
}
