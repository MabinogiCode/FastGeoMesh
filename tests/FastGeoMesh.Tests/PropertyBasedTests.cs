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
        public bool QualityInvariant_CapQuadsHaveValidQualityScores(PositiveInt size)
        {
            var s = Math.Min(size.Get, 8);
            if (s <= 0)
            {
                return true;
            }

            var square = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(s, 0), new Vec2(s, s), new Vec2(0, s)
            });

            var structure = new PrismStructureDefinition(square, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.5,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesh = new PrismMesher().Mesh(structure, options);
            var capQuads = mesh.Quads.Where(IsCapQuad).ToList();

            return capQuads.All(q =>
                q.QualityScore.HasValue &&
                q.QualityScore.Value >= 0.0 &&
                q.QualityScore.Value <= 1.0);
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
        public bool EdgeLengthRespected_QuadsHaveReasonableSize(PositiveInt targetLength)
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

            // Check that side quads have edges roughly respecting target length
            var sideQuads = mesh.Quads.Where(q => !IsCapQuad(q)).ToList();
            if (sideQuads.Count == 0)
            {
                return true;
            }

            // Sample a few quads and check edge lengths are reasonable
            var sampleSize = Math.Min(3, sideQuads.Count);
            var sample = sideQuads.Take(sampleSize);

            return sample.All(q =>
            {
                var edge1 = Length(q.V1 - q.V0);
                var edge2 = Length(q.V2 - q.V1);
                var edge3 = Length(q.V3 - q.V2);
                var edge4 = Length(q.V0 - q.V3);

                // Allow for some deviation but edges shouldn't be wildly off target
                var tolerance = target * 3.0;
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