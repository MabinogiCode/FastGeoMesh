#nullable enable
using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Property-based tests using FsCheck to validate mesh invariants with random inputs.</summary>
    public sealed class PropertyBasedTests
    {
        [Property(MaxTest = 15)]
        public bool PolygonInvariant_ValidPolygonProducesManifoldMesh(PositiveInt w, PositiveInt h, PositiveInt d)
        {
            var width = Math.Min(w.Get, 15);
            var height = Math.Min(h.Get, 15);
            var depth = Math.Min(d.Get, 8);

            if (width <= 0 || height <= 0 || depth <= 0)
            {
                return true; // Skip invalid inputs
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(width, 0), new Vec2(width, height), new Vec2(0, height)
            });

            var structure = new PrismStructureDefinition(rect, 0, depth);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 2.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var adjacency = indexed.BuildAdjacency();

            // Invariant: No non-manifold edges in a proper prism mesh
            return adjacency.NonManifoldEdges.Count == 0;
        }

        [Property(MaxTest = 20)]
        public bool QuadInvariant_AllQuadsHaveValidVertexIndices(PositiveInt width, PositiveInt height)
        {
            var w = Math.Min(width.Get, 12);
            var h = Math.Min(height.Get, 12);

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(w, 0), new Vec2(w, h), new Vec2(0, h)
            });

            var structure = new PrismStructureDefinition(rect, 0, 2);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.5,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            return indexed.Quads.All(q =>
                q.v0 >= 0 && q.v0 < indexed.Vertices.Count &&
                q.v1 >= 0 && q.v1 < indexed.Vertices.Count &&
                q.v2 >= 0 && q.v2 < indexed.Vertices.Count &&
                q.v3 >= 0 && q.v3 < indexed.Vertices.Count &&
                q.v0 != q.v1 && q.v1 != q.v2 && q.v2 != q.v3 && q.v3 != q.v0);
        }

        [Property(MaxTest = 15)]
        public bool QualityInvariant_MeshGenerationSucceedsWithValidParameters(PositiveInt size)
        {
            var s = Math.Min(size.Get, 8);
            if (s <= 0)
            {
                return true;
            }

            // Use minimum viable geometry size (at least 2x2 to ensure meshing occurs)
            var actualSize = Math.Max(s, 2);

            var square = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(actualSize, 0), new Vec2(actualSize, actualSize), new Vec2(0, actualSize)
            });

            var structure = new PrismStructureDefinition(square, 0, 1);
            var options = new MesherOptions
            {
                // Use conservative target that allows reasonable meshing
                TargetEdgeLengthXY = Math.Max(actualSize * 0.8, 0.5),
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesh = new PrismMesher().Mesh(structure, options);

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
            bool noNaNVertices = indexed.Vertices.All(v =>
                !double.IsNaN(v.X) && !double.IsNaN(v.Y) && !double.IsNaN(v.Z));

            return hasGeometry && validQualityScores && noNaNVertices;
        }

        [Property(MaxTest = 10)]
        public bool BoundsInvariant_MeshVerticesStayWithinExpectedBounds(PositiveInt width, PositiveInt height, PositiveInt depth)
        {
            var w = Math.Min(width.Get, 10);
            var h = Math.Min(height.Get, 10);
            var d = Math.Min(depth.Get, 6);

            if (w <= 0 || h <= 0 || d <= 0)
            {
                return true;
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(w, 0), new Vec2(w, h), new Vec2(0, h)
            });

            var structure = new PrismStructureDefinition(rect, 0, d);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 2.0,
                GenerateBottomCap = false,
                GenerateTopCap = false
            };

            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

            // Expected: all vertices should be within the bounding box (with small tolerance)
            return indexed.Vertices.All(v =>
                v.X >= -0.1 && v.X <= w + 0.1 &&
                v.Y >= -0.1 && v.Y <= h + 0.1 &&
                v.Z >= -0.1 && v.Z <= d + 0.1);
        }

        [Property(MaxTest = 8)]
        public bool TriangleInvariant_WhenTrianglesEnabled_ValidVertices(PositiveInt size)
        {
            var s = Math.Min(size.Get, 6);
            if (s <= 2)
            {
                return true;
            }

            // Create L-shaped polygon (likely to generate some triangles)
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(s, 0), new Vec2(s, s/2),
                new Vec2(s/2, s/2), new Vec2(s/2, s), new Vec2(0, s)
            });

            var structure = new PrismStructureDefinition(lShape, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                OutputRejectedCapTriangles = true,
                MinCapQuadQuality = 0.9 // High threshold may force triangles
            };

            var mesh = new PrismMesher().Mesh(structure, options);

            // All cap triangles should have valid vertices
            return mesh.Triangles.All(t =>
                !double.IsNaN(t.V0.X) && !double.IsNaN(t.V0.Y) && !double.IsNaN(t.V0.Z) &&
                !double.IsNaN(t.V1.X) && !double.IsNaN(t.V1.Y) && !double.IsNaN(t.V1.Z) &&
                !double.IsNaN(t.V2.X) && !double.IsNaN(t.V2.Y) && !double.IsNaN(t.V2.Z));
        }

        [Property(MaxTest = 12)]
        public bool EdgeLengthConstraint_EdgesRespectMaximumTarget(PositiveInt targetLength)
        {
            var target = Math.Min(targetLength.Get, 8);
            if (target <= 0)
            {
                return true;
            }

            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 10), new Vec2(0, 10)
            });

            var structure = new PrismStructureDefinition(rect, 0, 4);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = target,
                TargetEdgeLengthZ = target,
                GenerateBottomCap = false,
                GenerateTopCap = false
            };

            var mesh = new PrismMesher().Mesh(structure, options);

            // Check that side quads respect the maximum edge length constraint
            var sideQuads = mesh.Quads.Where(q => !IsCapQuad(q)).ToList();
            if (sideQuads.Count == 0)
            {
                return true;
            }

            // Sample a few quads and check that edges are <= target (with reasonable tolerance for numerical precision)
            var sampleSize = Math.Min(3, sideQuads.Count);
            var sample = sideQuads.Take(sampleSize);
            var tolerance = target + 0.1; // Small tolerance for numerical precision

            return sample.All(q =>
            {
                var edge1 = Length(q.V1 - q.V0);
                var edge2 = Length(q.V2 - q.V1);
                var edge3 = Length(q.V3 - q.V2);
                var edge4 = Length(q.V0 - q.V3);

                // Edges should respect the maximum constraint (can be smaller, but not larger)
                return edge1 <= tolerance && edge2 <= tolerance &&
                       edge3 <= tolerance && edge4 <= tolerance;
            });
        }

        private static bool IsCapQuad(Quad q)
        {
            const double epsilon = 1e-12;
            return Math.Abs(q.V0.Z - q.V1.Z) < epsilon &&
                   Math.Abs(q.V1.Z - q.V2.Z) < epsilon &&
                   Math.Abs(q.V2.Z - q.V3.Z) < epsilon;
        }

        private static double Length(Vec3 v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }
    }
}
