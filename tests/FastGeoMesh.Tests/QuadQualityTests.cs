using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class QuadQualityTests
{
    [Fact]
    public void CapsQuadsExposeQualityScoresBetween0And1()
    {
        var outer = Polygon2D.FromPoints(new[]
        {
            new Vec2(0,0), new Vec2(6,0), new Vec2(6,3), new Vec2(3,3), new Vec2(3,6), new Vec2(0,6)
        });
        var hole = Polygon2D.FromPoints(new[]
        {
            new Vec2(1,1), new Vec2(2,1), new Vec2(2,2), new Vec2(1,2)
        });
        var structure = new PrismStructureDefinition(outer, 0, 1).AddHole(hole);
        var options = new MesherOptions { TargetEdgeLengthXY = 0.75, TargetEdgeLengthZ = 0.5, GenerateTopAndBottomCaps = true };

        var mesh = new PrismMesher().Mesh(structure, options);
        var capQuads = mesh.Quads.Where(q => Math.Abs(q.V0.Z - q.V1.Z) < 1e-12 && Math.Abs(q.V1.Z - q.V2.Z) < 1e-12).ToList();
        capQuads.Should().NotBeEmpty();

        foreach (var q in capQuads)
        {
            q.QualityScore.Should().NotBeNull();
            q.QualityScore!.Value.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1);
        }
    }

    [Fact]
    public void SideQuadsHaveNoQualityScores()
    {
        var outer = Polygon2D.FromPoints(new[]
        {
            new Vec2(0,0), new Vec2(4,0), new Vec2(4,2), new Vec2(0,2)
        });
        var structure = new PrismStructureDefinition(outer, 0, 1);
        var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateTopAndBottomCaps = false };

        var mesh = new PrismMesher().Mesh(structure, options);
        var sideQuads = mesh.Quads.Where(q => !(Math.Abs(q.V0.Z - q.V1.Z) < 1e-12 && Math.Abs(q.V1.Z - q.V2.Z) < 1e-12)).ToList();
        sideQuads.Should().NotBeEmpty();
        sideQuads.All(q => q.QualityScore == null).Should().BeTrue();
    }
}
