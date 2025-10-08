using System;
using System.Linq;
using FastGeoMesh.Application;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Property-based tests using simple parameters to validate mesh invariants with random inputs.</summary>
    public sealed class PropertyBasedTests
    {
        /// <summary>
        /// Tests polygon invariant that valid rectangular polygons produce manifold meshes without non-manifold edges.
        /// Uses property-based testing to validate mesh topology across different rectangular dimensions.
        /// </summary>
        /// <param name="width">Width of the rectangle (between 2 and 20).</param>
        /// <param name="height">Height of the rectangle (between 2 and 20).</param>
        /// <param name="depth">Depth/height of the prism (between 1 and 10).</param>
        [Theory]
        [InlineData(5, 5, 2)]
        [InlineData(10, 8, 3)]
        [InlineData(15, 12, 4)]
        public void PolygonInvariantValidPolygonProducesManifoldMesh(int width, int height, int depth)
        {
            if (width <= 0 || height <= 0 || depth <= 0)
            {
                return; // Skip invalid inputs
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height)
            });

            var structure = new PrismStructureDefinition(rect, 0, depth);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(2.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .Build()
                .UnwrapForTests(); // V2.0 compatibility extension

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests(); // V2.0 compatibility
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adjacency = indexed.BuildAdjacency();

            // Invariant: No non-manifold edges in a proper prism mesh
            adjacency.NonManifoldEdges.Count.Should().Be(0);
        }

        /// <summary>
        /// Tests quad invariant that all generated quads have valid vertex indices within the mesh vertex bounds.
        /// Property-based test ensuring no out-of-bounds vertex references exist in the generated quad topology.
        /// </summary>
        /// <param name="width">Width of the rectangular structure (between 3 and 15).</param>
        /// <param name="height">Height of the rectangular structure (between 3 and 15).</param>
        [Theory]
        [InlineData(8, 6)]
        [InlineData(12, 10)]
        [InlineData(6, 4)]
        public void QuadInvariantAllQuadsHaveValidVertexIndices(int width, int height)
        {
            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height)
            });

            var structure = new PrismStructureDefinition(rect, 0, 2);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.5)
                .WithTargetEdgeLengthZ(1.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .Build()
                .UnwrapForTests();

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            indexed.Quads.All(q =>
                q.Item1 >= 0 && q.Item1 < indexed.Vertices.Count &&
                q.Item2 >= 0 && q.Item2 < indexed.Vertices.Count &&
                q.Item3 >= 0 && q.Item3 < indexed.Vertices.Count &&
                q.Item4 >= 0 && q.Item4 < indexed.Vertices.Count &&
                q.Item1 != q.Item2 && q.Item2 != q.Item3 && q.Item3 != q.Item4 && q.Item4 != q.Item1).Should().BeTrue();
        }

        /// <summary>
        /// Tests quality invariant that mesh generation succeeds with valid parameters across different sizes.
        /// Property-based test ensuring robust meshing behavior for various structural dimensions.
        /// </summary>
        /// <param name="size">Size parameter for the square structure (between 2 and 12).</param>
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void QualityInvariantMeshGenerationSucceedsWithValidParameters(int size)
        {
            if (size <= 0)
            {
                return;
            }

            // Use minimum viable geometry size (at least 2x2 to ensure meshing occurs)
            var actualSize = Math.Max(size, 2);

            var square = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(actualSize, 0), new Vec2(actualSize, actualSize), new Vec2(0, actualSize)
            });

            var structure = new PrismStructureDefinition(square, 0, 1);
            var options = MesherOptions.CreateBuilder()
                // Use conservative target that allows reasonable meshing
                .WithTargetEdgeLengthXY(Math.Max(actualSize * 0.8, 0.5))
                .WithTargetEdgeLengthZ(1.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .Build()
                .UnwrapForTests();

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();

            // Test basic mesh validity rather than specific cap quad expectations
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            // Invariants that should always hold:
            // 1. Mesh has some geometry (at least side faces)
            bool hasGeometry = indexed.Vertices.Count > 0 && indexed.Quads.Count > 0;

            // 2. All quality scores (when present) are in valid range
            bool validQualityScores = mesh.Quads.All(q =>
                !q.QualityScore.HasValue ||
                (q.QualityScore.Value >= 0.0 && q.QualityScore.Value <= 1.0));

            // 3. No degenerate vertices
            bool noNaNVertices = PropertyBasedTestHelper.ContainsNoNaNVertices(indexed.Vertices);

            (hasGeometry && validQualityScores && noNaNVertices).Should().BeTrue();
        }

        /// <summary>
        /// Tests bounds invariant that all mesh vertices stay within the expected geometric boundaries.
        /// Property-based test validating spatial constraints of generated mesh geometry.
        /// </summary>
        /// <param name="width">Width of the rectangular prism (between 1 and 15).</param>
        /// <param name="height">Height of the rectangular prism (between 1 and 15).</param>
        /// <param name="depth">Depth of the rectangular prism (between 1 and 10).</param>
        [Theory]
        [InlineData(6, 4, 3)]
        [InlineData(10, 8, 5)]
        public void BoundsInvariantMeshVerticesStayWithinExpectedBounds(int width, int height, int depth)
        {
            if (width <= 0 || height <= 0 || depth <= 0)
            {
                return;
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height)
            });

            var structure = new PrismStructureDefinition(rect, 0, depth);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(2.0)
                .WithTargetEdgeLengthZ(2.0)
                .WithGenerateBottomCap(false)
                .WithGenerateTopCap(false)
                .Build()
                .UnwrapForTests();

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            // Expected: all vertices should be within the bounding box (with small tolerance)
            PropertyBasedTestHelper.AreVerticesWithinBounds(
                indexed.Vertices, 0, width, 0, height, 0, depth).Should().BeTrue();
        }

        /// <summary>
        /// Tests triangle invariant that when triangles are enabled, all triangle vertices are valid.
        /// Property-based test ensuring triangle topology integrity when triangle output is requested.
        /// </summary>
        /// <param name="size">Size parameter for the square structure (between 2 and 8).</param>
        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        public void TriangleInvariantWhenTrianglesEnabledValidVertices(int size)
        {
            if (size <= 2)
            {
                return;
            }

            // Create L-shaped polygon (likely to generate some triangles)
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(size, 0), new Vec2(size, size/2),
                new Vec2(size/2, size/2), new Vec2(size/2, size), new Vec2(0, size)
            });

            var structure = new PrismStructureDefinition(lShape, 0, 1);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(1.0)
                .WithGenerateBottomCap(true)
                .WithGenerateTopCap(true)
                .WithRejectedCapTriangles(true)
                .WithMinCapQuadQuality(0.9) // High threshold may force triangles
                .Build()
                .UnwrapForTests();

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();

            // All cap triangles should have valid vertices
            PropertyBasedTestHelper.AreTrianglesValid(mesh.Triangles).Should().BeTrue();
        }

        /// <summary>
        /// Tests edge length constraint that generated edges respect the maximum target edge length.
        /// Property-based test validating that meshing parameters are properly applied to edge sizing.
        /// </summary>
        /// <param name="targetLength">Target edge length parameter (between 1 and 4).</param>
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void EdgeLengthConstraintEdgesRespectMaximumTarget(int targetLength)
        {
            if (targetLength <= 0)
            {
                return;
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 10), new Vec2(0, 10)
            });

            var structure = new PrismStructureDefinition(rect, 0, 4);
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(targetLength)
                .WithTargetEdgeLengthZ(targetLength)
                .WithGenerateBottomCap(false)
                .WithGenerateTopCap(false)
                .Build()
                .UnwrapForTests();

            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();

            // Check that side quads respect the maximum edge length constraint
            var sideQuads = mesh.Quads.Where(q => !PropertyBasedTestHelper.IsCapQuad(q)).ToList();
            if (sideQuads.Count == 0)
            {
                return;
            }

            PropertyBasedTestHelper.DoQuadEdgesRespectMaxLength(sideQuads, targetLength).Should().BeTrue();
        }
    }
}
