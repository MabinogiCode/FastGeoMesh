using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for adaptive side subdivision functionality.
    /// Verifies that internal surfaces and constraints properly affect side face generation.
    /// </summary>
    public sealed class AdaptiveSideSubdivisionTests
    {
        /// <summary>
        /// Tests that internal surface elevations appear in side quad Z levels.
        /// Verifies that large target edge lengths don't prevent subdivision when internal surfaces are present.
        /// </summary>
        [Fact]
        public void InternalSurfaceElevationAppearsInSideQuadZLevels()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var structure = new PrismStructureDefinition(outer, 0, 5)
                .AddInternalSurface(outer, 2.5);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(5.0),
                TargetEdgeLengthZ = EdgeLength.From(10.0),
                GenerateBottomCap = false,
                GenerateTopCap = false
            };
            // With large TargetEdgeLengthZ we'd normally get a single vertical segment, but internal surface forces subdivision.
            var mesh = new PrismMesher().Mesh(structure, opt).UnwrapForTests();

            // Extract distinct Z values from side quads
            var zset = mesh.Quads
                .Where(q => !(q.V0.Z == q.V1.Z && q.V1.Z == q.V2.Z && q.V2.Z == q.V3.Z))
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Distinct()
                .OrderBy(z => z)
                .ToList();
            zset.Should().Contain(2.5);
            zset.Should().Contain(0);
            zset.Should().Contain(5);
        }
    }
}
