using FastGeoMesh.Infrastructure.FileOperations;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Exporters {
    public sealed class LegacyMeshFilesCanBeLoadedTest {
        [Fact]
        public void Test() {
            var possibleDirectories = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "test_data", "legacy_meshes"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "test_data", "legacy_meshes"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "test_data", "legacy_meshes")
            };

            string? legacyMeshDir = null;
            foreach (var dir in possibleDirectories) {
                if (Directory.Exists(dir)) {
                    legacyMeshDir = dir;
                    break;
                }
            }

            if (legacyMeshDir == null) {
                return;
            }

            var legacyFiles = Directory.GetFiles(legacyMeshDir, "*.txt");
            if (legacyFiles.Length == 0) {
                return;
            }

            foreach (var path in legacyFiles) {
                var legacy = IndexedMeshFileOperations.ReadCustomTxt(path);
                legacy.Should().NotBeNull();
                legacy.Vertices.Should().NotBeEmpty();
                foreach (var vertex in legacy.Vertices) {
                    double.IsFinite(vertex.X).Should().BeTrue();
                    double.IsFinite(vertex.Y).Should().BeTrue();
                    double.IsFinite(vertex.Z).Should().BeTrue();
                }
            }
        }
    }
}
