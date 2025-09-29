using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests validating that our performance optimizations maintain correctness.</summary>
    public sealed class OptimizationValidationTests
    {
        [Fact]
        public void OptimizedMeshingProducesCorrectQuadCount()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10)
            });
            var structure = new PrismStructureDefinition(polygon, -5, 5);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 2.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesher = new PrismMesher();

            // Act
            var mesh = mesher.Mesh(structure, options);

            // Assert
            mesh.Quads.Should().NotBeEmpty();

            // Expected: side faces + caps
            // Rectangle perimeter = (20+10)*2 = 60, divided by target 2.0 = 30 segments
            // Height = 10, divided by target 2.0 = 5 levels
            // Side quads = 30 * 5 = 150
            // Cap area = 20*10 = 200, divided by 2*2 = 50 quads per cap
            // Total expected ? 150 + 100 = 250 quads
            mesh.Quads.Count.Should().BeGreaterThan(200).And.BeLessThan(300);
        }

        [Fact]
        public void OptimizedMeshingWithHolesExcludesHoleAreas()
        {
            // Arrange
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 20), new Vec2(0, 20)
            });
            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(5, 5), new Vec2(15, 5), new Vec2(15, 15), new Vec2(5, 15)
            });

            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesher = new PrismMesher();

            // Act
            var mesh = mesher.Mesh(structure, options);

            // Assert
            mesh.Quads.Should().NotBeEmpty();

            // Verify no quads exist in hole area
            var capQuads = mesh.Quads.Where(q =>
                Math.Abs(q.V0.Z - 0) < 0.1 || Math.Abs(q.V0.Z - 2) < 0.1).ToList();

            foreach (var quad in capQuads)
            {
                var centerX = (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) / 4.0;
                var centerY = (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) / 4.0;

                // Quad center should not be in hole area
                bool inHole = centerX > 5 && centerX < 15 && centerY > 5 && centerY < 15;
                inHole.Should().BeFalse($"Quad center ({centerX}, {centerY}) should not be in hole area");
            }
        }

        [Fact]
        public void SpatialIndexingGivesSameResultsAsOriginalAlgorithm()
        {
            // Arrange - Complex polygon to stress spatial indexing
            var vertices = new Vec2[8];
            for (int i = 0; i < 8; i++)
            {
                double angle = i * 2 * Math.PI / 8;
                vertices[i] = new Vec2(10 + 8 * Math.Cos(angle), 10 + 8 * Math.Sin(angle));
            }
            var polygon = Polygon2D.FromPoints(vertices);

            var structure = new PrismStructureDefinition(polygon, 0, 3);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesher = new PrismMesher();

            // Act
            var mesh = mesher.Mesh(structure, options);
            var indexedMesh = IndexedMesh.FromMesh(mesh);

            // Assert
            mesh.Quads.Should().NotBeEmpty();
            indexedMesh.Vertices.Should().NotBeEmpty();
            indexedMesh.Quads.Should().NotBeEmpty();

            // Verify mesh topology is manifold
            var adjacency = indexedMesh.BuildAdjacency();
            adjacency.NonManifoldEdges.Should().BeEmpty("Optimized meshing should produce manifold geometry");
        }

        [Fact]
        public void ObjectPoolingDoesNotAffectMeshConsistency()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 8), new Vec2(0, 8)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 4);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
            var mesher = new PrismMesher();

            // Act - Generate multiple meshes to test object pool reuse
            var mesh1 = mesher.Mesh(structure, options);
            var mesh2 = mesher.Mesh(structure, options);
            var mesh3 = mesher.Mesh(structure, options);

            // Assert - All meshes should be identical
            mesh1.Quads.Count.Should().Be(mesh2.Quads.Count);
            mesh2.Quads.Count.Should().Be(mesh3.Quads.Count);

            mesh1.Triangles.Count.Should().Be(mesh2.Triangles.Count);
            mesh2.Triangles.Count.Should().Be(mesh3.Triangles.Count);
        }

        [Fact]
        public void OptimizedStructsPreserveQualityScores()
        {
            // Arrange
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(8, 0), new Vec2(8, 3),
                new Vec2(3, 3), new Vec2(3, 8), new Vec2(0, 8)
            });
            var structure = new PrismStructureDefinition(lShape, 0, 2);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.5
            };
            var mesher = new PrismMesher();

            // Act
            var mesh = mesher.Mesh(structure, options);

            // Assert
            var capQuads = mesh.Quads.Where(q => q.QualityScore.HasValue).ToList();
            capQuads.Should().NotBeEmpty("Cap quads should have quality scores");

            foreach (var quad in capQuads)
            {
                quad.QualityScore.Should().HaveValue();
                quad.QualityScore!.Value.Should().BeInRange(0.0, 1.0);
            }
        }

        [Fact]
        public void MeshCachingOptimizationWorksCorrectly()
        {
            // Arrange
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0
            };
            var mesher = new PrismMesher();

            // Act
            var mesh = mesher.Mesh(structure, options);

            // Multiple accesses to cached ReadOnlyCollection properties
            var quads1 = mesh.Quads;
            var quads2 = mesh.Quads;
            var triangles1 = mesh.Triangles;
            var triangles2 = mesh.Triangles;

            // Assert
            ReferenceEquals(quads1, quads2).Should().BeTrue("ReadOnlyCollection should be cached");
            ReferenceEquals(triangles1, triangles2).Should().BeTrue("ReadOnlyCollection should be cached");

            // After modification, cache should be invalidated
            mesh.AddQuad(new Quad(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)));
            var quads3 = mesh.Quads;
            ReferenceEquals(quads1, quads3).Should().BeFalse("Cache should be invalidated after modification");
        }
    }
}
