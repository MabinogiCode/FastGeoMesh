using FastGeoMesh.Application.Helpers.Meshing;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    public sealed class GenerateSideQuadsRespectsOutwardFlagOrientationTest
    {
        [Fact]
        public void Test()
        {
            var loop = new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) };
            var z = new double[] { 0, 1 };
            var opt = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.5), TargetEdgeLengthZ = EdgeLength.From(1.0) };

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var geometryService = provider.GetRequiredService<IGeometryService>();

            var outward = SideFaceMeshingHelper.GenerateSideQuads(loop, z, opt, true, geometryService);
            var inward = SideFaceMeshingHelper.GenerateSideQuads(loop, z, opt, false, geometryService);
            outward.Should().HaveCount(inward.Count);
            outward.Should().NotBeEmpty();
            var oq = outward[0];
            var iq = inward[0];
            var oEdge1 = new Vec3(oq.V1.X - oq.V0.X, oq.V1.Y - oq.V0.Y, oq.V1.Z - oq.V0.Z);
            var oEdge2 = new Vec3(oq.V3.X - oq.V0.X, oq.V3.Y - oq.V0.Y, oq.V3.Z - oq.V0.Z);
            var oNormal = new Vec3(oEdge1.Y * oEdge2.Z - oEdge1.Z * oEdge2.Y, oEdge1.Z * oEdge2.X - oEdge1.X * oEdge2.Z, oEdge1.X * oEdge2.Y - oEdge1.Y * oEdge2.X);
            var iEdge1 = new Vec3(iq.V1.X - iq.V0.X, iq.V1.Y - iq.V0.Y, iq.V1.Z - iq.V0.Z);
            var iEdge2 = new Vec3(iq.V3.X - iq.V0.X, iq.V3.Y - iq.V0.Y, iq.V3.Z - iq.V0.Z);
            var iNormal = new Vec3(iEdge1.Y * iEdge2.Z - iEdge1.Z * iEdge2.Y, iEdge1.Z * iEdge2.X - iEdge1.X * iEdge2.Z, iEdge1.X * iEdge2.Y - iEdge1.Y * iEdge2.X);
            var dotProduct = oNormal.X * iNormal.X + oNormal.Y * iNormal.Y + oNormal.Z * iNormal.Z;
            dotProduct.Should().BeLessThan(0);
        }
    }
}
