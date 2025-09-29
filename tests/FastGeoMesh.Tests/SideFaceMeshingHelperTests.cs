using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class SideFaceMeshingHelperTests
    {
        [Fact]
        public void GenerateSideQuadsProducesExpectedVerticalLayers()
        {
            var loop = new[] { new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2) };
            var zLevels = new double[] { 0, 1, 2 };
            var options = new MesherOptions { TargetEdgeLengthXY = 2.0, TargetEdgeLengthZ = 1.0 };
            var quads = SideFaceMeshingHelper.GenerateSideQuads(loop, zLevels, options, outward: true);
            // Perimeter edges subdivision: each long edge (length 4) -> 2 segments; short edge (length 2) -> 1 segment
            // Total horizontal segments = (4/2 + 2/2)*2 = (2 + 1)*2 = 6 vertical columns
            // Vertical layers = zLevels.Count-1 = 2 -> expected quads = 12
            quads.Should().HaveCount(12);
            quads.All(q => q.V0.Z is 0 or 1 or 2).Should().BeTrue();
        }

        [Fact]
        public void GenerateSideQuadsRespectsOutwardFlagOrientation()
        {
            var loop = new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) };
            var z = new double[] { 0, 1 };
            var opt = new MesherOptions { TargetEdgeLengthXY = 2.0, TargetEdgeLengthZ = 1.0 };
            var outward = SideFaceMeshingHelper.GenerateSideQuads(loop, z, opt, true);
            var inward = SideFaceMeshingHelper.GenerateSideQuads(loop, z, opt, false);
            outward.Should().HaveCount(inward.Count);
            // Compare first quad orientation via cross product of bottom edge vectors
            var oq = outward.First();
            var iq = inward.First();
            double oCross = (oq.V1.X - oq.V0.X) * (oq.V2.Y - oq.V1.Y) - (oq.V1.Y - oq.V0.Y) * (oq.V2.X - oq.V1.X);
            double iCross = (iq.V1.X - iq.V0.X) * (iq.V2.Y - iq.V1.Y) - (iq.V1.Y - iq.V0.Y) * (iq.V2.X - iq.V1.X);
            (oCross >= 0).Should().BeTrue();
            (iCross <= 0).Should().BeTrue();
        }
    }
}
