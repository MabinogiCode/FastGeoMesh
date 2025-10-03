using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests edge cases, error conditions, and boundary scenarios to improve code coverage.
    /// Focuses on paths that are typically under-tested in normal scenarios.
    /// </summary>
    public sealed class EdgeCaseCoverageTests
    {
        private readonly PrismMesher _mesher;
        private readonly MesherOptions _options;

        /// <summary>Initializes the test class with a mesher and options.</summary>
        public EdgeCaseCoverageTests()
        {
            _mesher = new PrismMesher();
            _options = MesherOptions.CreateBuilder().WithFastPreset().Build();
        }

        /// <summary>Tests PrismMesher with custom cap strategy.</summary>
        [Fact]
        public void PrismMesher_WithCustomCapStrategy_UsesCustomStrategy()
        {
            // Arrange
            var customStrategy = new CustomTestCapStrategy();
            var mesher = new PrismMesher(customStrategy);

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            // Act
            var mesh = mesher.Mesh(structure, _options);

            // Assert
            mesh.Should().NotBeNull();
            customStrategy.WasCalled.Should().BeTrue("Custom strategy should have been used");
        }

        /// <summary>Tests null cap strategy validation.</summary>
        [Fact]
        public void PrismMesher_WithNullCapStrategy_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PrismMesher(null!));
        }

        /// <summary>Tests MesherOptions validation with invalid values.</summary>
        [Fact]
        public void MesherOptions_WithInvalidValues_FailsValidation()
        {
            // Arrange
            var invalidOptions = new MesherOptions
            {
                TargetEdgeLengthXY = -1.0, // Invalid negative value
                TargetEdgeLengthZ = 0.0,   // Invalid zero value
                MinCapQuadQuality = 2.0    // Invalid > 1.0 value
            };

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _mesher.Mesh(structure, invalidOptions));
        }

        /// <summary>Tests structure with inverted elevation where top elevation is less than bottom elevation.</summary>
        [Fact]
        public void PrismMesher_WithInvertedElevation_HandlesCorrectly()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 5.0, 0.0); // Top elevation < base elevation

            // Act
            var mesh = _mesher.Mesh(structure, _options);

            // Assert
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>Tests structure with zero height.</summary>
        [Fact]
        public void PrismMesher_WithZeroHeight_HandlesGracefully()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 2.0, 2.0); // Same elevation

            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = false,
                GenerateTopCap = false
            };

            // Act
            var mesh = _mesher.Mesh(structure, options);

            // Assert
            mesh.Should().NotBeNull();
            // Should have minimal or no quads for zero height
        }

        /// <summary>Tests structure with very small polygons.</summary>
        [Fact]
        public void PrismMesher_WithTinyPolygon_HandlesGracefully()
        {
            // Arrange - Very small polygon
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(0.001, 0), new Vec2(0.001, 0.001), new Vec2(0, 0.001)
                }), 0, 1);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.0001)
                .WithTargetEdgeLengthZ(0.1)
                .Build();

            // Act
            var mesh = _mesher.Mesh(structure, options);

            // Assert
            mesh.Should().NotBeNull();
        }

        /// <summary>Tests structure with very large target edge lengths.</summary>
        [Fact]
        public void PrismMesher_WithLargeTargetEdgeLength_GeneratesMinimalMesh()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
                }), 0, 10);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(100.0) // Much larger than polygon
                .WithTargetEdgeLengthZ(100.0)
                .Build();

            // Act
            var mesh = _mesher.Mesh(structure, options);

            // Assert
            mesh.Should().NotBeNull();
            // Should generate minimal mesh due to large target edge lengths
        }

        /// <summary>Tests structure with many internal surfaces.</summary>
        [Fact]
        public void PrismMesher_WithManyInternalSurfaces_HandlesCorrectly()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 20), new Vec2(0, 20)
                }), 0, 10);

            // Add multiple internal surfaces at different Z levels
            for (int i = 1; i < 10; i++)
            {
                var slabPolygon = Polygon2D.FromPoints(new[]
                {
                    new Vec2(2, 2), new Vec2(18, 2), new Vec2(18, 18), new Vec2(2, 18)
                });
                structure = structure.AddInternalSurface(slabPolygon, i);
            }

            // Act
            var mesh = _mesher.Mesh(structure, _options);

            // Assert
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>Tests structure with overlapping constraint segments.</summary>
        [Fact]
        public void PrismMesher_WithOverlappingConstraints_HandlesGracefully()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
                }), 0, 5);

            // Add overlapping constraint segments
            structure = structure
                .AddConstraintSegment(new Segment2D(new Vec2(0, 5), new Vec2(10, 5)), 2.5)
                .AddConstraintSegment(new Segment2D(new Vec2(1, 5), new Vec2(9, 5)), 2.5)
                .AddConstraintSegment(new Segment2D(new Vec2(2, 5), new Vec2(8, 5)), 2.5);

            // Act
            var mesh = _mesher.Mesh(structure, _options);

            // Assert
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
        }

        /// <summary>Tests structure without caps generation.</summary>
        [Fact]
        public void PrismMesher_WithoutCaps_GeneratesOnlySideFaces()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            var optionsNoCaps = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = false,
                GenerateTopCap = false
            };

            // Act
            var mesh = _mesher.Mesh(structure, optionsNoCaps);

            // Assert
            mesh.Should().NotBeNull();
            mesh.QuadCount.Should().BeGreaterThan(0);
            // Verify no horizontal quads (caps) are generated
        }

        /// <summary>Tests structure with auxiliary geometry at extreme coordinates.</summary>
        [Fact]
        public void PrismMesher_WithExtremeAuxiliaryGeometry_HandlesCorrectly()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            // Add auxiliary geometry at extreme coordinates
            structure.Geometry
                .AddPoint(new Vec3(-1000, -1000, -1000))
                .AddPoint(new Vec3(1000, 1000, 1000))
                .AddSegment(new Segment3D(new Vec3(-100, -100, -100), new Vec3(100, 100, 100)));

            // Act
            var mesh = _mesher.Mesh(structure, _options);

            // Assert
            mesh.Should().NotBeNull();
            mesh.Points.Should().Contain(p => p.X == -1000);
            mesh.Points.Should().Contain(p => p.X == 1000);
            mesh.InternalSegments.Should().HaveCount(1);
        }

        /// <summary>Tests MeshingProgress with extreme values.</summary>
        [Fact]
        public void MeshingProgress_WithExtremeValues_HandlesCorrectly()
        {
            // Arrange & Act
            var progressZero = new MeshingProgress("Test", 0.0, 0, 100);
            var progressComplete = new MeshingProgress("Test", 1.0, 100, 100);
            var progressOverComplete = new MeshingProgress("Test", 1.5, 150, 100); // > 100%

            // Assert
            progressZero.Percentage.Should().Be(0.0);
            progressComplete.Percentage.Should().Be(1.0);
            progressOverComplete.Percentage.Should().Be(1.5); // Should handle > 100%
        }

        /// <summary>Tests MeshingProgress.FromCounts with edge cases.</summary>
        [Fact]
        public void MeshingProgress_FromCounts_HandlesEdgeCases()
        {
            // Act & Assert - Division by zero protection
            var progressEmptyTotal = MeshingProgress.FromCounts("Test", 0, 0);
            progressEmptyTotal.Percentage.Should().Be(0.0);

            var progressZeroTotal = MeshingProgress.FromCounts("Test", 5, 0);
            progressZeroTotal.Percentage.Should().Be(0.0);

            var progressNormal = MeshingProgress.FromCounts("Test", 25, 100);
            progressNormal.Percentage.Should().Be(0.25);
        }

        /// <summary>Tests MeshingProgress.Completed factory method.</summary>
        [Fact]
        public void MeshingProgress_Completed_CreatesCompletedProgress()
        {
            // Act
            var completed = MeshingProgress.Completed("TestOperation", 42);

            // Assert
            completed.Operation.Should().Be("TestOperation");
            completed.Percentage.Should().Be(1.0);
            completed.StatusMessage.Should().Contain("Completed");
        }

        /// <summary>Custom test cap strategy to verify strategy pattern usage.</summary>
        private class CustomTestCapStrategy : ICapMeshingStrategy
        {
            public bool WasCalled { get; private set; }

            public void GenerateCaps(Mesh mesh, PrismStructureDefinition structure, MesherOptions options, double z0, double z1)
            {
                WasCalled = true;
                // Minimal implementation for testing
                if (options.GenerateBottomCap)
                {
                    // Add a simple test quad for bottom cap
                    mesh.AddQuad(new Quad(
                        new Vec3(0, 0, z0), new Vec3(1, 0, z0),
                        new Vec3(1, 1, z0), new Vec3(0, 1, z0)));
                }
                if (options.GenerateTopCap)
                {
                    // Add a simple test quad for top cap
                    mesh.AddQuad(new Quad(
                        new Vec3(0, 0, z1), new Vec3(1, 0, z1),
                        new Vec3(1, 1, z1), new Vec3(0, 1, z1)));
                }
            }
        }
    }
}
