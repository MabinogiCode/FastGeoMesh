using System.IO;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Additional coverage tests for exporters and core functionality to reach 90%+ coverage.
    /// Covers remaining edge cases in exporters, geometry helpers, and mesh operations.
    /// </summary>
    public sealed class ExporterCoverageTests
    {
        /// <summary>Tests ObjExporter with empty mesh.</summary>
        [Fact]
        public void ObjExporter_WithEmptyMesh_HandlesGracefully()
        {
            // Arrange
            var emptyMesh = new Mesh();
            var indexed = IndexedMesh.FromMesh(emptyMesh);
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act & Assert - Should not throw
                ObjExporter.Write(indexed, tempFile);

                // Verify file was created (even if empty)
                File.Exists(tempFile).Should().BeTrue();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests GltfExporter with empty mesh.</summary>
        [Fact]
        public void GltfExporter_WithEmptyMesh_HandlesGracefully()
        {
            // Arrange
            var emptyMesh = new Mesh();
            var indexed = IndexedMesh.FromMesh(emptyMesh);
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act & Assert - Should not throw
                GltfExporter.Write(indexed, tempFile);

                // Verify file was created
                File.Exists(tempFile).Should().BeTrue();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests SvgExporter with empty mesh.</summary>
        [Fact]
        public void SvgExporter_WithEmptyMesh_HandlesGracefully()
        {
            // Arrange
            var emptyMesh = new Mesh();
            var indexed = IndexedMesh.FromMesh(emptyMesh);
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act & Assert - Should not throw
                SvgExporter.Write(indexed, tempFile);

                // Verify file was created
                File.Exists(tempFile).Should().BeTrue();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>Tests exporters with mesh containing only points.</summary>
        [Fact]
        public void Exporters_WithPointsOnlyMesh_HandleGracefully()
        {
            // Arrange
            var mesh = new Mesh();
            mesh.AddPoint(new Vec3(1, 2, 3));
            mesh.AddPoint(new Vec3(4, 5, 6));
            var indexed = IndexedMesh.FromMesh(mesh);

            // Act & Assert - All should handle points-only mesh
            var objFile = Path.GetTempFileName();
            var gltfFile = Path.GetTempFileName();
            var svgFile = Path.GetTempFileName();

            try
            {
                ObjExporter.Write(indexed, objFile);
                GltfExporter.Write(indexed, gltfFile);
                SvgExporter.Write(indexed, svgFile);

                File.Exists(objFile).Should().BeTrue();
                File.Exists(gltfFile).Should().BeTrue();
                File.Exists(svgFile).Should().BeTrue();
            }
            finally
            {
                if (File.Exists(objFile))
                {
                    File.Delete(objFile);
                }
                if (File.Exists(gltfFile))
                {
                    File.Delete(gltfFile);
                }
                if (File.Exists(svgFile))
                {
                    File.Delete(svgFile);
                }
            }
        }

        /// <summary>Tests IndexedMesh with extreme epsilon values.</summary>
        [Fact]
        public void IndexedMesh_WithExtremeEpsilon_HandlesCorrectly()
        {
            // Arrange
            var mesh = new Mesh();
            mesh.AddQuad(new Quad(
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0)));
            mesh.AddQuad(new Quad(
                new Vec3(0.000001, 0.000001, 0), new Vec3(1.000001, 0.000001, 0),
                new Vec3(1.000001, 1.000001, 0), new Vec3(0.000001, 1.000001, 0)));

            // Act - Large epsilon should merge vertices
            var indexed = IndexedMesh.FromMesh(mesh, epsilon: 0.001);

            // Assert
            indexed.Vertices.Count.Should().BeLessThan(8); // Should merge some vertices
            indexed.Quads.Count.Should().Be(2);
        }

        /// <summary>Tests IndexedMesh with tiny epsilon.</summary>
        [Fact]
        public void IndexedMesh_WithTinyEpsilon_PreservesVertices()
        {
            // Arrange
            var mesh = new Mesh();
            mesh.AddQuad(new Quad(
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0)));
            mesh.AddQuad(new Quad(
                new Vec3(0.0001, 0.0001, 0), new Vec3(1.0001, 0.0001, 0),
                new Vec3(1.0001, 1.0001, 0), new Vec3(0.0001, 1.0001, 0)));

            // Act - Tiny epsilon should not merge vertices
            var indexed = IndexedMesh.FromMesh(mesh, epsilon: 1e-12);

            // Assert
            indexed.Vertices.Count.Should().Be(8); // Should keep all vertices separate
            indexed.Quads.Count.Should().Be(2);
        }

        /// <summary>Tests Mesh.AddInternalSegment functionality.</summary>
        [Fact]
        public void Mesh_AddInternalSegment_AddsCorrectly()
        {
            // Arrange
            var mesh = new Mesh();
            var segment = new Segment3D(new Vec3(0, 0, 0), new Vec3(10, 10, 10));

            // Act
            mesh.AddInternalSegment(segment);

            // Assert
            mesh.InternalSegments.Should().HaveCount(1);
            mesh.InternalSegments[0].Should().Be(segment);
        }

        /// <summary>Tests Mesh quad and triangle count properties.</summary>
        [Fact]
        public void Mesh_CountProperties_ReflectActualCounts()
        {
            // Arrange
            var mesh = new Mesh();

            // Act & Assert - Initially empty
            mesh.QuadCount.Should().Be(0);
            mesh.TriangleCount.Should().Be(0);

            // Add quads and triangles
            mesh.AddQuad(new Quad(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)));
            mesh.AddTriangle(new Triangle(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2.5, 1, 0)));

            mesh.QuadCount.Should().Be(1);
            mesh.TriangleCount.Should().Be(1);
        }

        /// <summary>Tests MesherOptions builder validation paths.</summary>
        [Fact]
        public void MesherOptionsBuilder_ValidationPaths_WorkCorrectly()
        {
            // Act & Assert - Various builder configurations should work
            var fastPreset = MesherOptions.CreateBuilder().WithFastPreset().Build();
            fastPreset.Should().NotBeNull();

            var highQualityPreset = MesherOptions.CreateBuilder().WithHighQualityPreset().Build();
            highQualityPreset.Should().NotBeNull();

            var customOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(2.0)
                .WithMinCapQuadQuality(0.5)
                .WithRejectedCapTriangles(true)
                .Build();

            customOptions.TargetEdgeLengthXY.Should().Be(1.0);
            customOptions.TargetEdgeLengthZ.Should().Be(2.0);
            customOptions.MinCapQuadQuality.Should().Be(0.5);
            customOptions.OutputRejectedCapTriangles.Should().BeTrue();

            // Test direct property assignment
            var directOptions = new MesherOptions
            {
                TargetEdgeLengthXY = 1.5,
                TargetEdgeLengthZ = 2.5,
                GenerateBottomCap = true,
                GenerateTopCap = false
            };

            directOptions.TargetEdgeLengthXY.Should().Be(1.5);
            directOptions.TargetEdgeLengthZ.Should().Be(2.5);
            directOptions.GenerateBottomCap.Should().BeTrue();
            directOptions.GenerateTopCap.Should().BeFalse();
        }

        /// <summary>Tests Polygon2D with collinear points.</summary>
        [Fact]
        public void Polygon2D_WithCollinearPoints_HandlesCorrectly()
        {
            // Arrange - Points that are collinear
            var points = new[]
            {
                new Vec2(0, 0),
                new Vec2(5, 0),  // Collinear with above and below
                new Vec2(10, 0),
                new Vec2(10, 10),
                new Vec2(0, 10)
            };

            // Act
            var polygon = Polygon2D.FromPoints(points);

            // Assert
            polygon.Count.Should().Be(5);
            polygon.Vertices.Should().HaveCount(5);
        }

        /// <summary>Tests Vec2 and Vec3 math operations.</summary>
        [Fact]
        public void VectorTypes_MathOperations_WorkCorrectly()
        {
            // Arrange
            var v2a = new Vec2(1, 2);
            var v2b = new Vec2(3, 4);
            var v3a = new Vec3(1, 2, 3);
            var v3b = new Vec3(4, 5, 6);

            // Act & Assert - Vec2 operations
            (v2a + v2b).Should().Be(new Vec2(4, 6));
            (v2b - v2a).Should().Be(new Vec2(2, 2));
            (v2a * 2).Should().Be(new Vec2(2, 4));

            // Vec3 operations
            (v3a + v3b).Should().Be(new Vec3(5, 7, 9));
            (v3b - v3a).Should().Be(new Vec3(3, 3, 3));
            (v3a * 3).Should().Be(new Vec3(3, 6, 9));
        }

        /// <summary>Tests PrismStructureDefinition method chaining.</summary>
        [Fact]
        public void PrismStructureDefinition_MethodChaining_WorksCorrectly()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
            });

            // Act - Chain multiple operations
            var structure = new PrismStructureDefinition(polygon, 0, 5)
                .AddHole(Polygon2D.FromPoints(new[]
                {
                    new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4)
                }))
                .AddConstraintSegment(new Segment2D(new Vec2(0, 5), new Vec2(10, 5)), 2.5)
                .AddInternalSurface(Polygon2D.FromPoints(new[]
                {
                    new Vec2(1, 1), new Vec2(9, 1), new Vec2(9, 9), new Vec2(1, 9)
                }), 1.0);

            // Assert
            structure.Holes.Should().HaveCount(1);
            structure.ConstraintSegments.Should().HaveCount(1);
            structure.InternalSurfaces.Should().HaveCount(1);
        }
    }
}
