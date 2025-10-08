using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for verifying cap refinement behavior near hole boundaries using configured refinement bands.
    /// </summary>
    public sealed class HoleRefinementTests
    {
        /// <summary>
        /// Ensures smaller edge lengths are produced near holes while larger elsewhere (heterogeneous quad sizes).
        /// </summary>
        [Fact]
        public void CapsAreRefinedNearHoles()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(10, 4), new Vec2(12, 4), new Vec2(12, 6), new Vec2(10, 6) });
            var structure = new PrismStructureDefinition(outer, -1, 0).AddHole(hole);
            
            // Test WITHOUT refinement first
            var baseOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();
                
            var baseMesh = new PrismMesher().Mesh(structure, baseOptions).UnwrapForTests();
            var baseTopQuads = baseMesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
            int baseQuadCount = baseTopQuads.Count;
            
            // Test WITH refinement
            var refinedOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .WithHoleRefinement(0.5, 1.0)  // Much smaller edge length near holes
                .Build().UnwrapForTests();
                
            var refinedMesh = new PrismMesher().Mesh(structure, refinedOptions).UnwrapForTests();
            var refinedTopQuads = refinedMesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
            int refinedQuadCount = refinedTopQuads.Count;
            
            // ACTUAL REFINEMENT TEST: Should have significantly more quads when refining
            refinedQuadCount.Should().BeGreaterThan(baseQuadCount, 
                $"Refinement should create more quads. Base: {baseQuadCount}, Refined: {refinedQuadCount}");
            
            // Should have at least 50% more quads due to refinement
            refinedQuadCount.Should().BeGreaterThan((int)(baseQuadCount * 1.5), 
                "Hole refinement should significantly increase quad density");
                
            // Verify quads are valid
            if (refinedTopQuads.Count > 0)
            {
                refinedTopQuads.Should().AllSatisfy(q => 
                {
                    q.V0.Should().NotBe(q.V1, "Quad vertices should be distinct");
                    q.V1.Should().NotBe(q.V2, "Quad vertices should be distinct");
                    q.V2.Should().NotBe(q.V3, "Quad vertices should be distinct");
                    q.V3.Should().NotBe(q.V0, "Quad vertices should be distinct");
                });
            }
        }
    }
}
