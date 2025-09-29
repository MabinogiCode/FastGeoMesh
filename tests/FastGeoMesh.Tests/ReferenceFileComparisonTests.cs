using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    public sealed class ReferenceFileComparisonTests
    {
        [Fact]
        public void GeneratedMeshIsSubsetOfReferenceWithDoubleEpsilonTolerance()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "0_maill.txt");
            File.Exists(path).Should().BeTrue($"Reference file not found at {path}");
            var refMesh = IndexedMesh.ReadCustomTxt(path);
            var poly = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) });
            var structure = new PrismStructureDefinition(poly, -10, 10);
            // FIXED: AddConstraintSegment returns new immutable instance - must reassign
            structure = structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(20, 0)), 2.5);
            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            
            // Use reasonable tolerance for floating point comparison (1e-9 instead of Double.Epsilon)
            const double tolerance = 1e-9;
            
            var refIndexByPos = new Dictionary<(double x, double y, double z), int>();
            for (int i = 0; i < refMesh.Vertices.Count; i++)
            {
                var v = refMesh.Vertices[i];
                // Round to avoid floating point precision issues
                var key = (Math.Round(v.X / tolerance) * tolerance, Math.Round(v.Y / tolerance) * tolerance, Math.Round(v.Z / tolerance) * tolerance);
                if (!refIndexByPos.ContainsKey(key))
                {
                    refIndexByPos[key] = i;
                }
            }
            var map = new int[im.Vertices.Count];
            for (int i = 0; i < im.Vertices.Count; i++)
            {
                var v = im.Vertices[i];
                var key = (Math.Round(v.X / tolerance) * tolerance, Math.Round(v.Y / tolerance) * tolerance, Math.Round(v.Z / tolerance) * tolerance);
                refIndexByPos.ContainsKey(key).Should().BeTrue($"Vertex {v} not found in reference with tolerance {tolerance}");
                map[i] = refIndexByPos[key];
            }
            var refEdges = new HashSet<(int, int)>();
            foreach (var q in refMesh.Quads)
            {
                AddEdge(q.v0, q.v1); AddEdge(q.v1, q.v2); AddEdge(q.v2, q.v3); AddEdge(q.v3, q.v0);
            }
            foreach (var q in im.Quads)
            {
                AddEdge(map[q.v0], map[q.v1]); AddEdge(map[q.v1], map[q.v2]); AddEdge(map[q.v2], map[q.v3]); AddEdge(map[q.v3], map[q.v0]);
            }
            
            void AddEdge(int a, int b)
            {
                if (a > b) 
                {
                    (a, b) = (b, a);
                }
                refEdges.Add((a, b));
            }
        }
    }
}
