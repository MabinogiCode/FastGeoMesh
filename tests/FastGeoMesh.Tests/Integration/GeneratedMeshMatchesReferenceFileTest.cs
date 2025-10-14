using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.FileOperations;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Integration
{
    public sealed class GeneratedMeshMatchesReferenceFileTest
    {
        [Fact]
        public void Test()
        {
            var possibleDirectories = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "test_data", "reference_meshes"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "test_data", "reference_meshes"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "test_data", "reference_meshes")
            };

            string? referenceDirectory = null;
            foreach (var dir in possibleDirectories)
            {
                if (Directory.Exists(dir))
                {
                    referenceDirectory = dir;
                    break;
                }
            }

            if (referenceDirectory == null)
            {
                // No reference data available in this environment - skip test
                return;
            }

            string path = Path.Combine(referenceDirectory, "simple_rectangle_reference.txt");
            if (!File.Exists(path))
            {
                return; // skip if specific reference file doesn't exist
            }

            var refMesh = IndexedMeshFileOperations.ReadCustomTxt(path);

            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = EdgeLength.From(1.0),
                TargetEdgeLengthZ = EdgeLength.From(0.5),
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            var mesher = new PrismMesher();
            var generatedMeshResult = mesher.Mesh(structure, options);

            if (generatedMeshResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to generate mesh: {generatedMeshResult.Error.Description}");
            }

            var generatedMesh = generatedMeshResult.Value;
            var generatedIndexed = IndexedMesh.FromMesh(generatedMesh);

            CompareMeshes(refMesh, generatedIndexed, tolerance: 1e-6);
        }

        private static void CompareMeshes(IndexedMesh reference, IndexedMesh generated, double tolerance)
        {
            reference.Vertices.Count.Should().Be(generated.Vertices.Count, "Vertex counts should match");
            reference.Quads.Count.Should().Be(generated.Quads.Count, "Quad counts should match");

            var refIndexByPos = new Dictionary<(double x, double y, double z), int>();
            for (int i = 0; i < reference.Vertices.Count; i++)
            {
                var v = reference.Vertices[i];
                var key = (v.X, v.Y, v.Z);
                if (!refIndexByPos.ContainsKey(key))
                {
                    refIndexByPos[key] = i;
                }
            }

            var genToRefMapping = new Dictionary<int, int>();
            for (int i = 0; i < generated.Vertices.Count; i++)
            {
                var genVertex = generated.Vertices[i];
                var found = false;

                foreach (var (refPos, refIndex) in refIndexByPos)
                {
                    if (Math.Abs(genVertex.X - refPos.x) < tolerance &&
                        Math.Abs(genVertex.Y - refPos.y) < tolerance &&
                        Math.Abs(genVertex.Z - refPos.z) < tolerance)
                    {
                        genToRefMapping[i] = refIndex;
                        found = true;
                        break;
                    }
                }

                found.Should().BeTrue($"Generated vertex {i} should have a corresponding reference vertex");
            }

            var refQuadSet = new HashSet<(int, int, int, int)>(reference.Quads.Select(q => NormalizeQuad(q)));

            var genQuadSet = new HashSet<(int, int, int, int)>(
                generated.Quads.Select(q => NormalizeQuad((
                    genToRefMapping.GetValueOrDefault(q.Item1, q.Item1),
                    genToRefMapping.GetValueOrDefault(q.Item2, q.Item2),
                    genToRefMapping.GetValueOrDefault(q.Item3, q.Item3),
                    genToRefMapping.GetValueOrDefault(q.Item4, q.Item4)))));

            var commonQuads = refQuadSet.Intersect(genQuadSet).Count();
            var totalQuads = Math.Max(refQuadSet.Count, genQuadSet.Count);

            if (totalQuads > 0)
            {
                var similarity = (double)commonQuads / totalQuads;
                similarity.Should().BeGreaterThan(0.7, "Meshes should have similar topology");
            }
        }

        private static (int, int, int, int) NormalizeQuad((int, int, int, int) quad)
        {
            var indices = new[] { quad.Item1, quad.Item2, quad.Item3, quad.Item4 };
            var minIndex = indices.Min();
            var startPos = Array.IndexOf(indices, minIndex);

            return startPos switch
            {
                0 => quad,
                1 => (quad.Item2, quad.Item3, quad.Item4, quad.Item1),
                2 => (quad.Item3, quad.Item4, quad.Item1, quad.Item2),
                3 => (quad.Item4, quad.Item1, quad.Item2, quad.Item3),
                _ => quad
            };
        }
    }
}
