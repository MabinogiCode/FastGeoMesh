using FastGeoMesh.Infrastructure.FileOperations;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    /// <summary>
    /// Tests for class LegacyMeshesHaveValidStructureTest.
    /// </summary>
    public sealed class LegacyMeshesHaveValidStructureTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var possibleDirectories = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "test_data", "legacy_meshes"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "test_data", "legacy_meshes"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "test_data", "legacy_meshes")
            };

            string? legacyMeshDir = null;
            foreach (var dir in possibleDirectories)
            {
                if (Directory.Exists(dir))
                {
                    legacyMeshDir = dir;
                    break;
                }
            }

            if (legacyMeshDir == null)
            {
                return;
            }

            var legacyFiles = Directory.GetFiles(legacyMeshDir, "*.txt");
            foreach (var path in legacyFiles)
            {
                var mesh = IndexedMeshFileOperations.ReadCustomTxt(path);
                foreach (var quad in mesh.Quads)
                {
                    quad.Item1.Should().BeInRange(0, mesh.Vertices.Count - 1);
                    quad.Item2.Should().BeInRange(0, mesh.Vertices.Count - 1);
                    quad.Item3.Should().BeInRange(0, mesh.Vertices.Count - 1);
                    quad.Item4.Should().BeInRange(0, mesh.Vertices.Count - 1);
                }
                foreach (var triangle in mesh.Triangles)
                {
                    triangle.Item1.Should().BeInRange(0, mesh.Vertices.Count - 1);
                    triangle.Item2.Should().BeInRange(0, mesh.Vertices.Count - 1);
                    triangle.Item3.Should().BeInRange(0, mesh.Vertices.Count - 1);
                }
                foreach (var edge in mesh.Edges)
                {
                    edge.Item1.Should().BeInRange(0, mesh.Vertices.Count - 1);
                    edge.Item2.Should().BeInRange(0, mesh.Vertices.Count - 1);
                }
            }
        }
    }
}
