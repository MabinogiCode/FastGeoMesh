using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class ReferenceFileComparisonTests
{
    [Fact]
    public void GeneratedMeshIsSubsetOfReferenceWithDoubleEpsilonTolerance()
    {
        // Load reference indexed mesh from file
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "0_maill.txt");
        _ = File.Exists(path).Should().BeTrue($"Reference file not found at {path}");
        var refMesh = IndexedMesh.ReadCustomTxt(path);

        // Build our structure matching the scenario (20x5 rectangle, -10..10), no caps
        var poly = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5)
        });
        var structure = new PrismStructureDefinition(poly, -10, 10);
        // Lierne at 2.5 m (present in reference); optional for Z=0.5 grid but keeps intent explicit
        structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(20, 0)), 2.5);

        // Reference uses XY = 1.0 m and Z = 0.5 m
        var options = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 0.5, GenerateTopAndBottomCaps = false };
        var mesh = new PrismMesher().Mesh(structure, options);
        var im = IndexedMesh.FromMesh(mesh, options.Epsilon); // use mesher epsilon for dedup

        // Map our generated vertices to reference indices using exact match (Double.Epsilon tolerance => exact equality)
        var refIndexByPos = new Dictionary<(double x, double y, double z), int>();
        for (int i = 0; i < refMesh.Vertices.Count; i++)
        {
            var v = refMesh.Vertices[i];
            var key = (v.X, v.Y, v.Z);
            if (!refIndexByPos.ContainsKey(key)) refIndexByPos[key] = i;
        }

        var map = new int[im.Vertices.Count];
        for (int i = 0; i < im.Vertices.Count; i++)
        {
            var v = im.Vertices[i];
            var key = (v.X, v.Y, v.Z);
            _ = refIndexByPos.ContainsKey(key).Should().BeTrue($"Vertex {v} not found in reference with Double.Epsilon tolerance");
            map[i] = refIndexByPos[key];
        }

        // Build reference edge set from reference quads (undirected)
        var refEdges = new HashSet<(int, int)>();
        foreach (var q in refMesh.Quads)
        {
            AddEdge(q.v0, q.v1);
            AddEdge(q.v1, q.v2);
            AddEdge(q.v2, q.v3);
            AddEdge(q.v3, q.v0);
        }

        // Every edge from our quads should exist in the reference edge set
        foreach (var q in im.Quads)
        {
            int a = map[q.v0], b = map[q.v1], c = map[q.v2], d = map[q.v3];
            _ = refEdges.Contains(Norm(a, b)).Should().BeTrue();
            _ = refEdges.Contains(Norm(b, c)).Should().BeTrue();
            _ = refEdges.Contains(Norm(c, d)).Should().BeTrue();
            _ = refEdges.Contains(Norm(d, a)).Should().BeTrue();
        }

        // Quads count should match (no caps)
        _ = im.Quads.Count.Should().Be(refMesh.Quads.Count);

        (int, int) Norm(int i, int j) => i < j ? (i, j) : (j, i);
        void AddEdge(int i, int j) => refEdges.Add(Norm(i, j));
    }
}
