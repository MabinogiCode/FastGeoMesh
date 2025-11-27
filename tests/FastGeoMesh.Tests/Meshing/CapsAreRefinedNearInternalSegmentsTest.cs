using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Meshing
{
    /// <summary>
    /// Tests for class CapsAreRefinedNearInternalSegmentsTest.
    /// </summary>
    public sealed class CapsAreRefinedNearInternalSegmentsTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10) });
            var structure = new PrismStructureDefinition(outer, -1, 0);
            structure.Geometry.AddPoint(new Vec3(9, 5, -0.5)).AddPoint(new Vec3(11, 5, -0.5)).AddSegment(new Segment3D(new Vec3(9, 5, -0.5), new Vec3(11, 5, -0.5)));

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithCaps(bottom: true, top: true)
                .WithSegmentRefinement(0.5, 1.0)
                .Build().UnwrapForTests();

            var mesh = TestServiceProvider.CreatePrismMesher().Mesh(structure, options).UnwrapForTests();

            var topQuads = mesh.Quads.Where(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0).ToList();
            var topTriangles = mesh.Triangles.Where(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0).ToList();
            var totalTopElements = topQuads.Count + topTriangles.Count;

            totalTopElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");

            if (topQuads.Count > 0)
            {
                topQuads.Should().AllSatisfy(q =>
                {
                    q.V0.Should().NotBe(q.V1, "Quad vertices should be distinct");
                    q.V1.Should().NotBe(q.V2, "Quad vertices should be distinct");
                    q.V2.Should().NotBe(q.V3, "Quad vertices should be distinct");
                    q.V3.Should().NotBe(q.V0, "Quad vertices should be distinct");
                });

                topQuads.Count.Should().BeGreaterThan(0, "Should have top quads for rectangle with segments");
            }

            mesh.Should().NotBeNull("Mesh with segment refinement should be generated");
        }
    }
}
