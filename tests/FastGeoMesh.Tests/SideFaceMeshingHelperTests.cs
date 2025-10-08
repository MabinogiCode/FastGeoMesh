using FastGeoMesh.Domain;
using FastGeoMesh.Meshing.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for side face meshing ensuring vertical layering and orientation handling.
    /// </summary>
    public sealed class SideFaceMeshingHelperTests
    {
        /// <summary>
        /// Verifies expected quad count based on perimeter subdivision and vertical layering.
        /// </summary>
        [Fact]
        public void GenerateSideQuadsProducesExpectedVerticalLayers()
        {
            var loop = new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) };
            var zLevels = new double[] { 0, 1, 2 };
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(2.0), TargetEdgeLengthZ = EdgeLength.From(1.0) };
            var quads = SideFaceMeshingHelper.GenerateSideQuads(loop, zLevels, options, outward: true);
            quads.Should().HaveCount(12);
            quads.All(q => q.V0.Z is 0 or 1 or 2).Should().BeTrue();
        }

        /// <summary>
        /// Ensures outward flag flips orientation (cross product sign) while preserving quad count.
        /// </summary>
        [Fact]
        public void GenerateSideQuadsRespectsOutwardFlagOrientation()
        {
            var loop = new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) };
            var z = new double[] { 0, 1 };
            var opt = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(0.5), TargetEdgeLengthZ = EdgeLength.From(1.0) };
            var outward = SideFaceMeshingHelper.GenerateSideQuads(loop, z, opt, true);
            var inward = SideFaceMeshingHelper.GenerateSideQuads(loop, z, opt, false);
            
            // Should have same number of quads
            outward.Should().HaveCount(inward.Count);
            outward.Should().NotBeEmpty();
            
            var oq = outward[0];
            var iq = inward[0];
            
            // Calculate normal vectors for the quads using cross product of edge vectors
            var oEdge1 = new Vec3(oq.V1.X - oq.V0.X, oq.V1.Y - oq.V0.Y, oq.V1.Z - oq.V0.Z);
            var oEdge2 = new Vec3(oq.V3.X - oq.V0.X, oq.V3.Y - oq.V0.Y, oq.V3.Z - oq.V0.Z);
            var oNormal = new Vec3(
                oEdge1.Y * oEdge2.Z - oEdge1.Z * oEdge2.Y,
                oEdge1.Z * oEdge2.X - oEdge1.X * oEdge2.Z,
                oEdge1.X * oEdge2.Y - oEdge1.Y * oEdge2.X
            );
            
            var iEdge1 = new Vec3(iq.V1.X - iq.V0.X, iq.V1.Y - iq.V0.Y, iq.V1.Z - iq.V0.Z);
            var iEdge2 = new Vec3(iq.V3.X - iq.V0.X, iq.V3.Y - iq.V0.Y, iq.V3.Z - iq.V0.Z);
            var iNormal = new Vec3(
                iEdge1.Y * iEdge2.Z - iEdge1.Z * iEdge2.Y,
                iEdge1.Z * iEdge2.X - iEdge1.X * iEdge2.Z,
                iEdge1.X * iEdge2.Y - iEdge1.Y * iEdge2.X
            );
            
            // The normals should point in opposite directions (different orientations)
            var dotProduct = oNormal.X * iNormal.X + oNormal.Y * iNormal.Y + oNormal.Z * iNormal.Z;
            dotProduct.Should().BeLessThan(0, "Outward and inward quads should have opposite orientations");
        }
    }
}
