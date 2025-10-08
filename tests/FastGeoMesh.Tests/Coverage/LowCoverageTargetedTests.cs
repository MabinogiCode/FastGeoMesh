using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.Services;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests specifically targeting the lowest coverage files to improve overall coverage percentage.
    /// Focuses on PerformanceMonitorService, IndexedMeshAdjacencyHelper, SegmentAtZ, MeshPool, and GeometryHelper.
    /// </summary>
    public sealed class LowCoverageTargetedTests
    {
        /// <summary>Tests PerformanceMonitorService functionality completely.</summary>
        [Fact]
        public void PerformanceMonitorServiceProvidesStatistics()
        {
            // Arrange
            var service = new PerformanceMonitorService();

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

        /// <summary>Tests IndexedMeshAdjacencyHelper methods thoroughly.</summary>
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

        /// <summary>Tests SegmentAtZ class completely.</summary>
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

        /// <summary>Tests MeshPool functionality completely.</summary>
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

        /// <summary>Tests PooledMeshExtensions methods completely.</summary>
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

        /// <summary>Tests Infrastructure GeometryHelper methods (using Utils namespace).</summary>
        [Fact]
        public void InfrastructureGeometryHelperWorksCorrectly()
        {
            // Test 2D linear interpolation
            var start2D = new Vec2(0, 0);
            var end2D = new Vec2(10, 20);

            var lerp2D_0 = FastGeoMesh.Utils.GeometryHelper.Lerp(start2D, end2D, 0.0);
            var lerp2D_1 = FastGeoMesh.Utils.GeometryHelper.Lerp(start2D, end2D, 1.0);
            var lerp2D_half = FastGeoMesh.Utils.GeometryHelper.Lerp(start2D, end2D, 0.5);

            lerp2D_0.Should().Be(start2D);
            lerp2D_1.Should().Be(end2D);
            lerp2D_half.Should().Be(new Vec2(5, 10));

            // Test scalar interpolation
            var lerpScalar = FastGeoMesh.Utils.GeometryHelper.LerpScalar(0, 100, 0.25);
            lerpScalar.Should().Be(25);

            // Test point in polygon with simple square
            var square = new Vec2[]
            {
                new Vec2(0, 0), new Vec2(4, 0),
                new Vec2(4, 4), new Vec2(0, 4)
            };

            // Points clearly inside
            FastGeoMesh.Utils.GeometryHelper.PointInPolygon(square, new Vec2(2, 2)).Should().BeTrue();
            FastGeoMesh.Utils.GeometryHelper.PointInPolygon(square, 1, 1).Should().BeTrue();

            // Points clearly outside
            FastGeoMesh.Utils.GeometryHelper.PointInPolygon(square, new Vec2(-1, 2)).Should().BeFalse();
            FastGeoMesh.Utils.GeometryHelper.PointInPolygon(square, 5, 2).Should().BeFalse();

            // Test convexity checking
            var convexQuad = (
                a: new Vec2(0, 0),
                b: new Vec2(2, 0),
                c: new Vec2(2, 2),
                d: new Vec2(0, 2)
            );
            FastGeoMesh.Utils.GeometryHelper.IsConvex(convexQuad).Should().BeTrue();

            // Test distance calculation - use a point that's definitely off the segment
            var point1 = new Vec2(0, 0);
            var point2 = new Vec2(3, 4);
            var distance = FastGeoMesh.Utils.GeometryHelper.DistancePointToSegment(new Vec2(0, 2), point1, point2);
            distance.Should().BeGreaterThan(0);

            // Test polygon area calculation
            var area = FastGeoMesh.Utils.GeometryHelper.PolygonArea(square);
            area.Should().Be(16); // 4x4 square
        }

        /// <summary>Tests batch point-in-polygon functionality.</summary>
        [Fact]
        public void BatchPointInPolygonWorksCorrectly()
        {
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
            FastGeoMesh.Utils.GeometryHelper.BatchPointInPolygon(triangle, testPoints, results);

            results[0].Should().BeTrue();  // inside
            results[1].Should().BeTrue();  // inside
            results[2].Should().BeFalse(); // outside
            results[3].Should().BeFalse(); // outside
            results[4].Should().BeFalse(); // outside

            // Test argument validation
            var wrongSizeResults = new bool[testPoints.Length - 1];
            Assert.Throws<ArgumentException>(() =>
                FastGeoMesh.Utils.GeometryHelper.BatchPointInPolygon(triangle, testPoints, wrongSizeResults));
        }

        /// <summary>Tests GeometryConfig settings.</summary>
        [Fact]
        public void GeometryConfigurationCanBeModified()
        {
            // Store original values
            var originalDefaultTolerance = GeometryConfig.DefaultTolerance;
            var originalConvexityTolerance = GeometryConfig.ConvexityTolerance;
            var originalPointInPolygonTolerance = GeometryConfig.PointInPolygonTolerance;

            try
            {
                // Test setting new values
                GeometryConfig.DefaultTolerance = 1e-6;
                GeometryConfig.ConvexityTolerance = -1e-6;
                GeometryConfig.PointInPolygonTolerance = 1e-8;

                GeometryConfig.DefaultTolerance.Should().Be(1e-6);
                GeometryConfig.ConvexityTolerance.Should().Be(-1e-6);
                GeometryConfig.PointInPolygonTolerance.Should().Be(1e-8);

                // Test that the new tolerances are used in calculations
                var square = new Vec2[]
                {
                    new Vec2(0, 0), new Vec2(1, 0),
                    new Vec2(1, 1), new Vec2(0, 1)
                };

                // This should still work with the new tolerance
                var result = FastGeoMesh.Utils.GeometryHelper.PointInPolygon(square, 0.5, 0.5, GeometryConfig.DefaultTolerance);
                result.Should().BeTrue();
            }
            finally
            {
                // Restore original values
                GeometryConfig.DefaultTolerance = originalDefaultTolerance;
                GeometryConfig.ConvexityTolerance = originalConvexityTolerance;
                GeometryConfig.PointInPolygonTolerance = originalPointInPolygonTolerance;
            }
        }
    }
}
