using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for class LowCoverageTargetedTests.
    /// </summary>
    public sealed class LowCoverageTargetedTests
    {
        /// <summary>
        /// Runs test PerformanceMonitorServiceProvidesStatistics.
        /// </summary>
        [Fact]
        public void PerformanceMonitorServiceProvidesStatistics()
        {
            // Arrange - resolve via DI to obtain the real implementation
            var services = new ServiceCollection();
            services.AddFastGeoMeshWithMonitoring();
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IPerformanceMonitor>();

            // Act
            var stats = service.GetLiveStatistics();

            // Assert
            stats.Should().NotBeNull();
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
            stats.QuadsGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.TrianglesGenerated.Should().BeGreaterThanOrEqualTo(0);
            stats.PoolHitRate.Should().BeInRange(0.0, 1.0);

            // Test ToString functionality
            var statsString = stats.ToString();
            statsString.Should().Contain("Operations:");
            statsString.Should().Contain("Quads:");
            statsString.Should().Contain("Triangles:");
            statsString.Should().Contain("Pool Hit Rate:");
        }
        /// <summary>
        /// Runs test IndexedMeshAdjacencyHelperBuildsAdjacencyCorrectly.
        /// </summary>
        [Fact]
        public void IndexedMeshAdjacencyHelperBuildsAdjacencyCorrectly()
        {
            // Arrange - Create a simple mesh with 2 adjacent quads
            var mesh = new ImmutableMesh();

            // First quad: (0,0,0) to (1,1,0)
            var quad1 = new Quad(
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0));

            // Second quad: (1,0,0) to (2,1,0) - shares edge with first quad
            var quad2 = new Quad(
                new Vec3(1, 0, 0), new Vec3(2, 0, 0),
                new Vec3(2, 1, 0), new Vec3(1, 1, 0));

            mesh = mesh.AddQuad(quad1).AddQuad(quad2);
            var indexed = IndexedMesh.FromMesh(mesh);

            // Act
            var adjacency = IndexedMeshAdjacencyHelper.BuildAdjacency(indexed);

            // Assert
            adjacency.Should().NotBeNull();
            adjacency.QuadCount.Should().Be(2);
            adjacency.Neighbors.Should().HaveCount(2);
            adjacency.BoundaryEdges.Should().NotBeEmpty(); // Should have boundary edges
            adjacency.NonManifoldEdges.Should().BeEmpty(); // Should not have non-manifold edges for this simple case

            // Test argument validation
            Assert.Throws<ArgumentNullException>(() => IndexedMeshAdjacencyHelper.BuildAdjacency(null!));
        }
        /// <summary>
        /// Runs test SegmentAtZImplementsIElementCorrectly.
        /// </summary>
        [Fact]
        public void SegmentAtZImplementsIElementCorrectly()
        {
            // Arrange
            var segment = new Segment2D(new Vec2(0, 0), new Vec2(10, 5));
            const double z = 3.5;

            // Act
            var segmentAtZ = new SegmentAtZ(segment, z);

            // Assert
            segmentAtZ.Kind.Should().Be(nameof(SegmentAtZ));
            segmentAtZ.Segment.Should().Be(segment);
            segmentAtZ.Z.Should().Be(z);

            // Test that it properly implements IElement
            IElement element = segmentAtZ;
            element.Kind.Should().Be(nameof(SegmentAtZ));

            // Test with different values
            var segment2 = new Segment2D(new Vec2(-5, -10), new Vec2(15, 20));
            var segmentAtZ2 = new SegmentAtZ(segment2, -1.5);

            segmentAtZ2.Segment.Should().Be(segment2);
            segmentAtZ2.Z.Should().Be(-1.5);
        }
        /// <summary>
        /// Runs test MeshPoolProvidesImmutableMeshInstances.
        /// </summary>
        [Fact]
        public void MeshPoolProvidesImmutableMeshInstances()
        {
            // Test basic Get operation
            var mesh1 = MeshPool.Get();
            var mesh2 = MeshPool.Get();

            mesh1.Should().NotBeNull();
            mesh2.Should().NotBeNull();
            mesh1.Should().Be(ImmutableMesh.Empty); // Should return empty mesh
            mesh2.Should().Be(ImmutableMesh.Empty); // Should return empty mesh

            // Test Return operation (should be no-op for immutable objects)
            MeshPool.Return(mesh1); // Should not throw
            MeshPool.Return(mesh2); // Should not throw

            // Test that we can still use meshes after "returning" them (since they're immutable)
            mesh1.QuadCount.Should().Be(0);
            mesh2.TriangleCount.Should().Be(0);
        }
        /// <summary>
        /// Runs test PooledMeshExtensionsExecuteOperationsCorrectly.
        /// </summary>
        [Fact]
        public void PooledMeshExtensionsExecuteOperationsCorrectly()
        {
            // Test WithPooledMesh with return value
            var result = PooledMeshExtensions.WithPooledMesh(mesh =>
            {
                mesh.Should().NotBeNull();
                mesh.Should().Be(ImmutableMesh.Empty);
                return mesh.QuadCount + mesh.TriangleCount;
            });

            result.Should().Be(0); // Empty mesh has 0 quads and 0 triangles

            // Test WithPooledMesh with void operation
            var operationExecuted = false;
            PooledMeshExtensions.WithPooledMesh(mesh =>
            {
                mesh.Should().NotBeNull();
                mesh.Should().Be(ImmutableMesh.Empty);
                operationExecuted = true;
            });

            operationExecuted.Should().BeTrue();

            // Test with more complex operations
            var complexResult = PooledMeshExtensions.WithPooledMesh(mesh =>
            {
                // Add some elements to the mesh
                var modifiedMesh = mesh.AddQuad(new Quad(
                    new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                    new Vec3(1, 1, 0), new Vec3(0, 1, 0)));

                return modifiedMesh.QuadCount;
            });

            complexResult.Should().Be(1);
        }
        /// <summary>
        /// Runs test InfrastructureGeometryHelperWorksCorrectly.
        /// </summary>
        [Fact]
        public void InfrastructureGeometryHelperWorksCorrectly()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            // Test 2D linear interpolation
            var start2D = new Vec2(0, 0);
            var end2D = new Vec2(10, 20);

            var lerp2D_0 = helper.Lerp(start2D, end2D, 0.0);
            var lerp2D_1 = helper.Lerp(start2D, end2D, 1.0);
            var lerp2D_half = helper.Lerp(start2D, end2D, 0.5);

            lerp2D_0.Should().Be(start2D);
            lerp2D_1.Should().Be(end2D);
            lerp2D_half.Should().Be(new Vec2(5, 10));

            // Test scalar interpolation
            var lerpScalar = helper.LerpScalar(0, 100, 0.25);
            lerpScalar.Should().Be(25);

            // Test point in polygon with simple square
            var square = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(4, 0),
                new Vec2(4, 4), new Vec2(0, 4)
            };

            // Points clearly inside
            helper.PointInPolygon(square, new Vec2(2, 2)).Should().BeTrue();
            helper.PointInPolygon(square, 1, 1).Should().BeTrue();

            // Points clearly outside
            helper.PointInPolygon(square, new Vec2(-1, 2)).Should().BeFalse();
            helper.PointInPolygon(square, 5, 2).Should().BeFalse();

            // Test convexity checking
            var convexQuad = (
                a: new Vec2(0, 0),
                b: new Vec2(2, 0),
                c: new Vec2(2, 2),
                d: new Vec2(0, 2)
            );
            helper.IsConvex(convexQuad).Should().BeTrue();

            // Test distance calculation - use a point that's definitely off the segment
            var point1 = new Vec2(0, 0);
            var point2 = new Vec2(3, 4);
            var distance = helper.DistancePointToSegment(new Vec2(0, 2), point1, point2);
            distance.Should().BeGreaterThan(0);

            // Test polygon area calculation
            var area = helper.PolygonArea(square);
            area.Should().Be(16); // 4x4 square
        }
        /// <summary>
        /// Runs test BatchPointInPolygonWorksCorrectly.
        /// </summary>
        [Fact]
        public void BatchPointInPolygonWorksCorrectly()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            var triangle = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(3, 6)
            };

            var testPoints = new Vec2[]
            {
                new Vec2(3, 2),    // inside
                new Vec2(1, 1),    // inside
                new Vec2(-1, 1),   // outside
                new Vec2(7, 1),    // outside
                new Vec2(3, 7)     // outside
            };

            var results = new bool[testPoints.Length];

            // Test batch operation
            helper.BatchPointInPolygon(triangle, testPoints, results);

            results[0].Should().BeTrue();  // inside
            results[1].Should().BeTrue();  // inside
            results[2].Should().BeFalse(); // outside
            results[3].Should().BeFalse(); // outside
            results[4].Should().BeFalse(); // outside

            // Test argument validation
            var wrongSizeResults = new bool[testPoints.Length - 1];
            Assert.Throws<ArgumentException>(() =>
                helper.BatchPointInPolygon(triangle, testPoints, wrongSizeResults));
        }
        /// <summary>
        /// Runs test GeometryConfigurationCanBeModified.
        /// </summary>
        [Fact]
        public void GeometryConfigurationCanBeModified()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var config = provider.GetRequiredService<IGeometryConfig>() as GeometryConfigImpl;
            Assert.NotNull(config);

            // Store original values
            var originalDefaultTolerance = config.DefaultTolerance;
            var originalConvexityTolerance = config.ConvexityTolerance;
            var originalPointInPolygonTolerance = config.PointInPolygonTolerance;

            try
            {
                // Test setting new values
                config.DefaultTolerance = 1e-6;
                config.ConvexityTolerance = -1e-6;
                config.PointInPolygonTolerance = 1e-8;

                config.DefaultTolerance.Should().Be(1e-6);
                config.ConvexityTolerance.Should().Be(-1e-6);
                config.PointInPolygonTolerance.Should().Be(1e-8);

                // Test that the new tolerances are used in calculations
                var square = new Vec2[]
                {
                    new Vec2(0, 0), new Vec2(1, 0),
                    new Vec2(1, 1), new Vec2(0, 1)
                };

                // This should still work with the new tolerance
                var helper = provider.GetRequiredService<IGeometryHelper>();
                var result = helper.PointInPolygon(square, 0.5, 0.5, config.DefaultTolerance);
                result.Should().BeTrue();
            }
            finally
            {
                // Restore original values
                config.DefaultTolerance = originalDefaultTolerance;
                config.ConvexityTolerance = originalConvexityTolerance;
                config.PointInPolygonTolerance = originalPointInPolygonTolerance;
            }
        }
    }
}
