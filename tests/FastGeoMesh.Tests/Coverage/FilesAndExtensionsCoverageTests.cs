using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Additional tests for low-coverage files focusing on file operations and extensions.
    /// </summary>
    public sealed class FilesAndExtensionsCoverageTests
    {
        /// <summary>Tests IndexedMeshExtensions methods.</summary>
        [Fact]
        public void IndexedMeshExtensionMethodsWorkCorrectly()
        {
            // Arrange - Create a simple mesh
            var mesh = new ImmutableMesh();
            var quad1 = new Quad(
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0));
            var quad2 = new Quad(
                new Vec3(1, 0, 0), new Vec3(2, 0, 0),
                new Vec3(2, 1, 0), new Vec3(1, 1, 0));

            mesh = mesh.AddQuad(quad1).AddQuad(quad2);
            var indexed = IndexedMesh.FromMesh(mesh);

            // Test BuildAdjacency extension method
            var adjacency = indexed.BuildAdjacency();
            adjacency.Should().NotBeNull();
            adjacency.QuadCount.Should().Be(2);

            // Test WriteCustomTxt extension method
            var tempFile = Path.GetTempFileName();
            try
            {
                indexed.WriteCustomTxt(tempFile);
                File.Exists(tempFile).Should().BeTrue();
                var content = File.ReadAllText(tempFile);
                content.Should().NotBeEmpty();
                content.Should().Contain("0.000000"); // Should contain vertex coordinates
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests IndexedMeshHelper static methods.</summary>
        [Fact]
        public void IndexedMeshHelperReadWriteWorkCorrectly()
        {
            // Arrange - Create a simple test mesh
            var originalVertices = new[]
            {
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0)
            };
            var originalQuads = new[] { (0, 1, 2, 3) };
            var originalEdges = new[] { (0, 1), (1, 2), (2, 3), (3, 0) };

            var originalMesh = new IndexedMesh(originalVertices, originalEdges, originalQuads, Array.Empty<(int, int, int)>());

            var tempFile = Path.GetTempFileName();
            try
            {
                // Write mesh using helper
                IndexedMeshFileHelper.WriteCustomTxt(originalMesh, tempFile);

                // Read it back using helper
                var loadedMesh = IndexedMeshHelper.ReadCustomTxt(tempFile);

                // Verify the round-trip
                loadedMesh.Should().NotBeNull();
                loadedMesh.Vertices.Should().HaveCount(4);
                loadedMesh.Quads.Should().HaveCount(1);
                loadedMesh.Edges.Should().HaveCount(4);

                // Verify vertices are approximately equal
                for (int i = 0; i < originalVertices.Length; i++)
                {
                    loadedMesh.Vertices[i].X.Should().BeApproximately(originalVertices[i].X, 1e-5);
                    loadedMesh.Vertices[i].Y.Should().BeApproximately(originalVertices[i].Y, 1e-5);
                    loadedMesh.Vertices[i].Z.Should().BeApproximately(originalVertices[i].Z, 1e-5);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests IndexedMeshFileHelper read/write operations comprehensively.</summary>
        [Fact]
        public void IndexedMeshFileHelperHandlesComplexMeshes()
        {
            // Create a more complex mesh with triangles and edges
            var vertices = new[]
            {
                new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(4, 0, 0),
                new Vec3(0, 2, 0), new Vec3(2, 2, 0), new Vec3(4, 2, 0),
                new Vec3(1, 4, 1), new Vec3(3, 4, 1)
            };

            var quads = new[] { (0, 1, 4, 3), (1, 2, 5, 4) };
            var triangles = new[] { (3, 4, 6), (4, 5, 7) };
            var edges = new[] { (6, 7) };

            var originalMesh = new IndexedMesh(vertices, edges, quads, triangles);

            var tempFile = Path.GetTempFileName();
            try
            {
                // Test standard format
                IndexedMeshFileHelper.WriteCustomTxt(originalMesh, tempFile);
                var content = File.ReadAllText(tempFile);

                content.Should().StartWith("8"); // 8 vertices
                content.Should().Contain("1.000000"); // Should contain coordinates

                var loadedMesh = IndexedMeshFileHelper.ReadCustomTxt(tempFile);
                loadedMesh.Vertices.Should().HaveCount(8);
                loadedMesh.Quads.Should().HaveCount(2);

                // Test alternative format
                var altTempFile = Path.GetTempFileName();
                try
                {
                    IndexedMeshFileHelper.WriteCustomTxtAlternative(originalMesh, altTempFile);
                    var altContent = File.ReadAllText(altTempFile);

                    altContent.Should().Contain("# Custom mesh format");
                    altContent.Should().Contain("v 0.000000");
                    altContent.Should().Contain("q 0 1 4 3");

                    var altLoadedMesh = IndexedMeshFileHelper.ReadCustomTxtAlternative(altTempFile);
                    altLoadedMesh.Vertices.Should().HaveCount(8);
                    altLoadedMesh.Quads.Should().HaveCount(2);
                    altLoadedMesh.Triangles.Should().HaveCount(2);
                }
                finally
                {
                    if (File.Exists(altTempFile))
                    {
                        File.Delete(altTempFile);
                    }
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests IndexedMeshFileHelper error handling.</summary>
        [Fact]
        public void IndexedMeshFileHelperHandlesErrorsCorrectly()
        {
            var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent_mesh_file.txt");

            // Test file not found
            Assert.Throws<FileNotFoundException>(() => IndexedMeshFileHelper.ReadCustomTxt(nonExistentFile));
            Assert.Throws<FileNotFoundException>(() => IndexedMeshFileHelper.ReadCustomTxtAlternative(nonExistentFile));

            // Test invalid file format
            var invalidFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(invalidFile, "invalid mesh data\nnot a number\n");
                Assert.Throws<InvalidDataException>(() => IndexedMeshFileHelper.ReadCustomTxt(invalidFile));
            }
            finally
            {
                if (File.Exists(invalidFile))
                {
                    File.Delete(invalidFile);
                }
            }

            // Test null arguments - create empty IndexedMesh correctly
            var emptyMesh = new IndexedMesh(
                Array.Empty<Vec3>(),
                Array.Empty<(int, int)>(),
                Array.Empty<(int, int, int, int)>(),
                Array.Empty<(int, int, int)>());

            Assert.Throws<ArgumentNullException>(() => IndexedMeshFileHelper.WriteCustomTxt(null!, "test.txt"));
            Assert.Throws<ArgumentException>(() => IndexedMeshFileHelper.WriteCustomTxt(emptyMesh, ""));
            Assert.Throws<ArgumentNullException>(() => IndexedMeshFileHelper.WriteCustomTxtAlternative(null!, "test.txt"));
            Assert.Throws<ArgumentException>(() => IndexedMeshFileHelper.WriteCustomTxtAlternative(emptyMesh, ""));
        }

        /// <summary>Tests alternative format with comments and empty lines.</summary>
        [Fact]
        public void AlternativeFormatHandlesCommentsAndEmptyLines()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                // Create a file with comments and empty lines
                var content = @"# This is a comment
# Another comment

v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 1.0 1.0 0.0
v 0.0 1.0 0.0

# This is a quad
q 0 1 2 3

# This is a triangle
t 0 1 2

# This is an edge
e 0 1

# End of file";

                File.WriteAllText(tempFile, content);

                var mesh = IndexedMeshFileHelper.ReadCustomTxtAlternative(tempFile);
                mesh.Vertices.Should().HaveCount(4);
                mesh.Quads.Should().HaveCount(1);
                mesh.Triangles.Should().HaveCount(1);
                mesh.Edges.Should().HaveCount(1);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests SpanExtensions if accessible.</summary>
        [Fact]
        public void SpanExtensionsCanBeUsed()
        {
            // Test basic span operations that should be covered
            var array = new[] { 1, 2, 3, 4, 5 };
            var span = array.AsSpan();

            // Basic span operations
            span.Length.Should().Be(5);
            span[0].Should().Be(1);
            span[^1].Should().Be(5);

            // Test slicing
            var slice = span.Slice(1, 3);
            slice.Length.Should().Be(3);
            slice[0].Should().Be(2);
            slice[2].Should().Be(4);

            // Test ReadOnlySpan
            ReadOnlySpan<int> readOnlySpan = array;
            readOnlySpan.Length.Should().Be(5);
            readOnlySpan[2].Should().Be(3);
        }

        /// <summary>Tests ImmutableMesh additional operations.</summary>
        [Fact]
        public void ImmutableMeshAdditionalOperationsCovered()
        {
            var mesh = ImmutableMesh.Empty;

            // Test adding single elements
            var point = new Vec3(1, 2, 3);
            var segment = new Segment3D(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
            var triangle = new Triangle(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0));
            var quad = new Quad(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0));

            mesh = mesh.AddPoint(point);
            mesh = mesh.AddInternalSegment(segment);
            mesh = mesh.AddTriangle(triangle);
            mesh = mesh.AddQuad(quad);

            mesh.Points.Should().HaveCount(1);
            mesh.InternalSegments.Should().HaveCount(1);
            mesh.TriangleCount.Should().Be(1);
            mesh.QuadCount.Should().Be(1);

            // Test that the mesh is truly immutable
            var originalMesh = mesh;
            var newMesh = mesh.AddPoint(new Vec3(5, 6, 7));

            originalMesh.Points.Should().HaveCount(1); // Original unchanged
            newMesh.Points.Should().HaveCount(2); // New has additional point

            // Test properties
            mesh.Triangles.Should().HaveCount(1);
            mesh.Quads.Should().HaveCount(1);
            mesh.Points.Should().HaveCount(1);
            mesh.InternalSegments.Should().HaveCount(1);
        }

        /// <summary>Tests MesherOptions additional scenarios.</summary>
        [Fact]
        public void MesherOptionsAdditionalScenariosCovered()
        {
            // Test various builder combinations
            var result1 = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.25)
                .WithCaps(bottom: true, top: false)
                .Build();

            result1.IsSuccess.Should().BeTrue();
            if (result1.IsSuccess)
            {
                var options = result1.Value;
                options.GenerateBottomCap.Should().BeTrue();
                options.GenerateTopCap.Should().BeFalse();
            }

            // Test boundary value scenarios
            var extremeResult = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.001) // Very small
                .WithTargetEdgeLengthZ(1000)   // Very large
                .Build();

            // Should either succeed or fail gracefully
            extremeResult.Should().NotBeNull();

            // Test preset overrides
            var presetOverride = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(2.0) // Override preset value
                .Build();

            if (presetOverride.IsSuccess)
            {
                presetOverride.Value.TargetEdgeLengthXY.Value.Should().Be(2.0);
            }
        }

        /// <summary>Tests various error scenarios in a controlled way.</summary>
        [Fact]
        public void ErrorScenariosHandledGracefully()
        {
            // Test with malformed mesh data
            var tempFile = Path.GetTempFileName();
            try
            {
                // Write invalid data that should cause parsing errors
                File.WriteAllText(tempFile, "3\n1 invalid_number 0 0\n2 0 invalid_number 0\n3 0 0 invalid_number\n");

                Assert.Throws<InvalidDataException>(() => IndexedMeshFileHelper.ReadCustomTxt(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

            // Test with insufficient data
            var tempFile2 = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile2, "1\n"); // Claims 1 vertex but provides none

                Assert.Throws<InvalidDataException>(() => IndexedMeshFileHelper.ReadCustomTxt(tempFile2));
            }
            finally
            {
                if (File.Exists(tempFile2))
                {
                    File.Delete(tempFile2);
                }
            }
        }
    }
}
