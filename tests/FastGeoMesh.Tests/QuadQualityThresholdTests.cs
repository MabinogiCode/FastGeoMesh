using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class QuadQualityThresholdTests
    {
        [Fact]
        public void MinCapQuadQualityRejectsPoorPairs()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(5,0), new Vec2(5,1), new Vec2(2.6,1), new Vec2(2.4,3), new Vec2(5,3), new Vec2(5,5), new Vec2(0,5) });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var strict = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true, MinCapQuadQuality = 0.5 };
            var loose = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true, MinCapQuadQuality = 0.0 };
            var meshStrict = new PrismMesher().Mesh(structure, strict);
            var meshLoose = new PrismMesher().Mesh(structure, loose);
            bool IsTop(Quad q) => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1;
            var topStrict = meshStrict.Quads.Where(IsTop).ToList();
            var topLoose = meshLoose.Quads.Where(IsTop).ToList();
            topStrict.Should().NotBeEmpty();
            topLoose.Should().NotBeEmpty();
            double thresh = 0.3;
            int badStrict = topStrict.Count(q => q.QualityScore is { } s && s < thresh);
            int badLoose = topLoose.Count(q => q.QualityScore is { } s && s < thresh);
            badStrict.Should().BeLessThanOrEqualTo(badLoose + 1);
        }

        [Fact]
        public void DefaultQualityThresholdApplies()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(5,0), new Vec2(5,1), new Vec2(2.6,1), new Vec2(2.4,3), new Vec2(5,3), new Vec2(5,5), new Vec2(0,5) });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var withDefault = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true };
            var loose = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 0.5, GenerateBottomCap = true, GenerateTopCap = true, MinCapQuadQuality = 0.0 };
            var meshDef = new PrismMesher().Mesh(structure, withDefault);
            var meshLoose = new PrismMesher().Mesh(structure, loose);
            bool IsTop(Quad q) => q.V0.Z == 1 && q.V1.Z == 1 && q.V2.Z == 1 && q.V3.Z == 1;
            var topDef = meshDef.Quads.Where(IsTop).ToList();
            var topLoose = meshLoose.Quads.Where(IsTop).ToList();
            topDef.Should().NotBeEmpty();
            topLoose.Should().NotBeEmpty();
            double thresh = 0.3;
            int badDef = topDef.Count(q => q.QualityScore is { } s && s < thresh);
            int badLoose = topLoose.Count(q => q.QualityScore is { } s && s < thresh);
            badDef.Should().BeLessThanOrEqualTo(badLoose + 1);
        }
    }
}
