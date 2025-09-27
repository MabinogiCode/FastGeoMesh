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
            structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(20, 0)), 2.5);
            var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateBottomCap = false, GenerateTopCap = false };
            var mesh = new PrismMesher().Mesh(structure, options);
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            var refIndexByPos = new Dictionary<(double x, double y, double z), int>();
            for (int i = 0; i < refMesh.Vertices.Count; i++)
            {
                var v = refMesh.Vertices[i];
                var key = (v.X, v.Y, v.Z);
                if (!refIndexByPos.ContainsKey(key))
                {
                    refIndexByPos[key] = i;
                }
            }
            var map = new int[im.Vertices.Count];
            for (int i = 0; i < im.Vertices.Count; i++)
            {
                var v = im.Vertices[i];
                var key = (v.X, v.Y, v.Z);
                refIndexByPos.ContainsKey(key).Should().BeTrue($"Vertex {v} not found in reference with Double.Epsilon tolerance");
                map[i] = refIndexByPos[key];
            }
            var refEdges = new HashSet<(int, int)>();
            foreach (var q in refMesh.Quads)
            {
                AddEdge(q.v0, q.v1); AddEdge(q.v1, q.v2); AddEdge(q.v2, q.v3); AddEdge(q.v3, q.v0);
            }
            foreach (var q in im.Quads)
            {
                int a = map[q.v0], b = map[q.v1], c = map[q.v2], d = map[q.v3];
                refEdges.Contains(Norm(a, b)).Should().BeTrue();
                refEdges.Contains(Norm(b, c)).Should().BeTrue();
                refEdges.Contains(Norm(c, d)).Should().BeTrue();
                refEdges.Contains(Norm(d, a)).Should().BeTrue();
            }
            im.Quads.Count.Should().Be(refMesh.Quads.Count);
            (int, int) Norm(int i, int j) => i < j ? (i, j) : (j, i);
            void AddEdge(int i, int j) => refEdges.Add(Norm(i, j));
        }
    }
}
