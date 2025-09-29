using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests verifying refinement near holes / segments for rectangle fast-path caps.</summary>
    public sealed class RectangleCapRefinementTests
    {
        [Fact]
        public void HoleRefinementIncreasesQuadDensityAroundHole()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(20,0), new Vec2(20,10), new Vec2(0,10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(8,4), new Vec2(12,4), new Vec2(12,6), new Vec2(8,6) });
            var baseStruct = new PrismStructureDefinition(rect, 0, 1).AddHole(hole);
            var coarse = new MesherOptions { TargetEdgeLengthXY = 2.5, TargetEdgeLengthZ = 1, GenerateBottomCap = true, GenerateTopCap = false };
            var refined = new MesherOptions { TargetEdgeLengthXY = 2.5, TargetEdgeLengthZ = 1, GenerateBottomCap = true, GenerateTopCap = false, TargetEdgeLengthXYNearHoles = 1.0, HoleRefineBand = 2.5 };
            var meshCoarse = new PrismMesher().Mesh(baseStruct, coarse);
            var meshRefined = new PrismMesher().Mesh(baseStruct, refined);
            int coarseCap = meshCoarse.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int refinedCap = meshRefined.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            refinedCap.Should().BeGreaterThan(coarseCap, "Refinement near holes should add quads");
        }

        [Fact]
        public void SegmentRefinementIncreasesQuadDensityAlongSegment()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(30,0), new Vec2(30,10), new Vec2(0,10) });
            var structure = new PrismStructureDefinition(rect, 0, 1);
            // Add internal segment across the rectangle midline
            var seg = new Segment3D(new Vec3(0,5,0), new Vec3(30,5,0));
            structure.Geometry.AddSegment(seg);
            var coarse = new MesherOptions { TargetEdgeLengthXY = 3.0, TargetEdgeLengthZ = 1, GenerateBottomCap = true, GenerateTopCap = false };
            var refined = new MesherOptions { TargetEdgeLengthXY = 3.0, TargetEdgeLengthZ = 1, GenerateBottomCap = true, GenerateTopCap = false, TargetEdgeLengthXYNearSegments = 1.2, SegmentRefineBand = 3.5 };
            var meshCoarse = new PrismMesher().Mesh(structure, coarse);
            var meshRefined = new PrismMesher().Mesh(structure, refined);
            int coarseCap = meshCoarse.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int refinedCap = meshRefined.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            refinedCap.Should().BeGreaterThan(coarseCap, "Refinement near segments should add quads");
        }

        [Fact]
        public void RefinementBandsDoNotAffectSideQuadsCountSignificantly()
        {
            var rect = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) });
            var structure = new PrismStructureDefinition(rect, 0, 4);
            var baseOpt = new MesherOptions { TargetEdgeLengthXY = 2.5, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true };
            var refinedOpt = new MesherOptions { TargetEdgeLengthXY = 2.5, TargetEdgeLengthZ = 1.0, GenerateBottomCap = true, GenerateTopCap = true, TargetEdgeLengthXYNearHoles = 1.25, HoleRefineBand = 2.0 };
            var baseMesh = new PrismMesher().Mesh(structure, baseOpt);
            var refinedMesh = new PrismMesher().Mesh(structure, refinedOpt);
            // Side quads identified by differing Z among vertices
            int CountSideQuads(FastGeoMesh.Meshing.Mesh m) => m.Quads.Count(q => !(q.V0.Z == q.V1.Z && q.V1.Z == q.V2.Z && q.V2.Z == q.V3.Z));
            int sidesBase = CountSideQuads(baseMesh);
            int sidesRefined = CountSideQuads(refinedMesh);
            // Allow small variance due to potential different Z-level subdivisions reused
            sidesRefined.Should().BeInRange((int)(sidesBase * 0.9), (int)(sidesBase * 1.1));
        }
    }
}
