using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests covering L and T shaped excavations with and without auxiliary geometry and constraints.
    /// </summary>
    public sealed class ShapeVariationTests
    {
        /// <summary>
        /// Validates meshing of an L shape without additional geometry, ensuring manifold caps and side faces.
        /// </summary>
        [Fact]
        public void LShapeWithoutExtraGeometryMeshesCapsAndSidesManifold()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(3, 3), new Vec2(3, 8), new Vec2(0, 8) });
            var structure = new PrismStructureDefinition(outer, 0, 4);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.75)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)  // ✅ Forcer la génération des caps
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();

            // ✅ Pour les formes complexes, le système génère des triangles, pas des quads
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 4 && q.V1.Z == 4 && q.V2.Z == 4 && q.V3.Z == 4);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 4 && t.V1.Z == 4 && t.V2.Z == 4);
            int topElements = topQuads + topTriangles; // Total éléments de surface supérieure

            int botQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int botElements = botQuads + botTriangles; // Total éléments de surface inférieure

            _ = topElements.Should().BeGreaterThan(0, "Should have top cap elements (quads or triangles)");
            _ = botElements.Should().BeGreaterThan(0, "Should have bottom cap elements (quads or triangles)");
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }

        /// <summary>
        /// Ensures constraints and internal segments in an L shape are integrated, generating appropriate Z subdivisions.
        /// </summary>
        [Fact]
        public void LShapeWithExtraGeometryIntegratesConstraintAndSegments()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3), new Vec2(3, 3), new Vec2(3, 8), new Vec2(0, 8) });
            var structure = new PrismStructureDefinition(outer, 0, 6);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 1.0), new Vec2(7.5, 1.0)), 2.0);
            var pA = new Vec3(0.5, 2.5, 1.0);
            var pB = new Vec3(2.5, 2.5, 3.0);
            _ = structure.Geometry.AddPoint(pA).AddPoint(pB).AddSegment(new Segment3D(pA, pB));
            var sA = new Vec3(3.0, 3.0, 1.5);
            var sB = new Vec3(0.5, 6.5, 1.5);
            _ = structure.Geometry.AddSegment(new Segment3D(sA, sB));
            var tA = new Vec3(7.5, 0.5, 2.5);
            var tB = new Vec3(5.0, 2.5, 2.5);
            _ = structure.Geometry.AddSegment(new Segment3D(tA, tB));
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.75)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)  // ✅ Forcer la génération des caps
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            _ = mesh.Points.Should().Contain(pA);
            _ = mesh.Points.Should().Contain(pB);
            _ = mesh.InternalSegments.Should().HaveCount(3);
            var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
            _ = zset.Should().Contain(2.0).And.Contain(1.0).And.Contain(3.0).And.Contain(1.5).And.Contain(2.5);

            // ✅ Pour les formes complexes, le système génère des triangles pour les caps
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 6 && q.V1.Z == 6 && q.V2.Z == 6 && q.V3.Z == 6);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 6 && t.V1.Z == 6 && t.V2.Z == 6);
            int topElements = topQuads + topTriangles;

            int botQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int botElements = botQuads + botTriangles;

            _ = topElements.Should().BeGreaterThan(0, "Should have top cap elements");
            _ = botElements.Should().BeGreaterThan(0, "Should have bottom cap elements");
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }

        /// <summary>
        /// Validates meshing of a T shape without auxiliary geometry producing manifold caps and consistent side quads.
        /// </summary>
        [Fact]
        public void TShapeWithoutExtraGeometryMeshesCapsAndSidesManifold()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(7, 0), new Vec2(7, 2), new Vec2(5, 2), new Vec2(5, 5), new Vec2(2, 5), new Vec2(2, 2), new Vec2(0, 2) });
            var structure = new PrismStructureDefinition(outer, -3, 0);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)  // ✅ Forcer la génération des caps
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();

            // ✅ Pour les formes complexes T, accepter triangles et quads
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int topElements = topQuads + topTriangles;

            int botQuads = mesh.Quads.Count(q => q.V0.Z == -3 && q.V1.Z == -3 && q.V2.Z == -3 && q.V3.Z == -3);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == -3 && t.V1.Z == -3 && t.V2.Z == -3);
            int botElements = botQuads + botTriangles;

            _ = topElements.Should().BeGreaterThan(0, "Should have top cap elements");
            _ = botElements.Should().BeGreaterThan(0, "Should have bottom cap elements");
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }

        /// <summary>
        /// Ensures constraints and internal segments in a T shape are correctly propagated and meshed.
        /// </summary>
        [Fact]
        public void TShapeWithExtraGeometryIntegratesConstraints()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(7, 0), new Vec2(7, 2), new Vec2(5, 2), new Vec2(5, 5), new Vec2(2, 5), new Vec2(2, 2), new Vec2(0, 2) });
            var structure = new PrismStructureDefinition(outer, -4, 0);
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0.5, 0.5), new Vec2(6.5, 0.5)), -2.0);
            var b1 = new Vec3(2.5, 3.5, -1.0);
            var b2 = new Vec3(4.5, 3.5, -1.0);
            _ = structure.Geometry.AddSegment(new Segment3D(b1, b2));
            var st1 = new Vec3(2.0, 2.0, -0.5);
            var st2 = new Vec3(5.0, 2.0, -0.5);
            _ = structure.Geometry.AddSegment(new Segment3D(st1, st2));
            var an1 = new Vec3(6.8, 0.2, -3.0);
            var an2 = new Vec3(4.0, 2.0, -3.0);
            _ = structure.Geometry.AddSegment(new Segment3D(an1, an2));
            var refP = new Vec3(1.0, 1.0, -1.5);
            _ = structure.Geometry.AddPoint(refP);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.8)
                .WithTargetEdgeLengthZ(0.4)
                .WithCaps(bottom: true, top: true)  // ✅ Forcer la génération des caps
                .Build().UnwrapForTests();
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adj = im.BuildAdjacency();
            _ = mesh.Points.Should().Contain(refP);
            _ = mesh.InternalSegments.Should().HaveCount(3);
            var zset = mesh.Quads.SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z }).ToHashSet();
            _ = zset.Should().Contain(-2.0).And.Contain(-1.5).And.Contain(-1.0).And.Contain(-0.5).And.Contain(-3.0);

            // ✅ Pour les formes complexes T, accepter triangles et quads
            int topQuads = mesh.Quads.Count(q => q.V0.Z == 0 && q.V1.Z == 0 && q.V2.Z == 0 && q.V3.Z == 0);
            int topTriangles = mesh.Triangles.Count(t => t.V0.Z == 0 && t.V1.Z == 0 && t.V2.Z == 0);
            int topElements = topQuads + topTriangles;

            int botQuads = mesh.Quads.Count(q => q.V0.Z == -4 && q.V1.Z == -4 && q.V2.Z == -4 && q.V3.Z == -4);
            int botTriangles = mesh.Triangles.Count(t => t.V0.Z == -4 && t.V1.Z == -4 && t.V2.Z == -4);
            int botElements = botQuads + botTriangles;

            _ = topElements.Should().BeGreaterThan(0, "Should have top cap elements");
            _ = botElements.Should().BeGreaterThan(0, "Should have bottom cap elements");
            _ = adj.NonManifoldEdges.Should().BeEmpty();
        }
    }
}
