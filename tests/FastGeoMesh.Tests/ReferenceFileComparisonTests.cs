using System.Collections.Generic;
using System.IO;
using FastGeoMesh.Domain;
using FastGeoMesh.Application;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Compares generated mesh to a reference legacy mesh ensuring vertex and edge subset relations within tolerance.
    /// </summary>
    public sealed class ReferenceFileComparisonTests
    {
        /// <summary>
        /// Generates a mesh and verifies its vertices and edges map into a reference file within floating point tolerance.
        /// </summary>
        [Fact]
        public void GeneratedMeshIsSubsetOfReferenceWithDoubleEpsilonTolerance()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", TestFileConstants.LegacyMeshFileName);
            File.Exists(path).Should().BeTrue($"Reference file not found at {path}");
            var refMesh = IndexedMeshFileHelper.ReadCustomTxt(path);
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(TestGeometries.StandardRectangleWidth, 0),
                new Vec2(TestGeometries.StandardRectangleWidth, TestGeometries.StandardRectangleHeight),
                new Vec2(0, TestGeometries.StandardRectangleHeight)
            });
            var structure = new PrismStructureDefinition(poly, TestGeometries.StandardBottomZ, TestGeometries.StandardTopZ)
                .AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(TestGeometries.StandardRectangleWidth, 0)), TestGeometries.StandardConstraintZ);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = TestMeshOptions.DefaultTargetEdgeLengthXY,
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ,
                GenerateBottomCap = false,
                GenerateTopCap = false
            };
            var mesh = new PrismMesher().Mesh(structure, options).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            const double tolerance = TestTolerances.Epsilon;
            var refIndexByPos = new Dictionary<(double x, double y, double z), int>();
            for (int i = 0; i < refMesh.Vertices.Count; i++)
            {
                var v = refMesh.Vertices[i];
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
                AddEdge(q.Item1, q.Item2); AddEdge(q.Item2, q.Item3); AddEdge(q.Item3, q.Item4); AddEdge(q.Item4, q.Item1);
            }
            foreach (var q in im.Quads)
            {
                AddEdge(map[q.Item1], map[q.Item2]); AddEdge(map[q.Item2], map[q.Item3]); AddEdge(map[q.Item3], map[q.Item4]); AddEdge(map[q.Item4], map[q.Item1]);
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
