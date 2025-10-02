using System.Diagnostics;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>Tests for performance optimizations and new .NET 8 features.</summary>
    public sealed class PerformanceOptimizationTests
    {
        /// <summary>Tests that MesherOptionsBuilder creates valid options with all configurations.</summary>
        [Fact]
        public void MesherOptionsBuilderCreatesValidOptions()
        {
            // Arrange & Act
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .WithHoleRefinement(0.5, 1.0)
                .WithMinCapQuadQuality(0.6)
                .WithRejectedCapTriangles(true)
                .Build();

            // Assert
            options.TargetEdgeLengthXY.Should().Be(1.0);
            options.TargetEdgeLengthZ.Should().Be(0.5);
            options.GenerateBottomCap.Should().BeTrue();
            options.GenerateTopCap.Should().BeTrue();
            options.TargetEdgeLengthXYNearHoles.Should().Be(0.5);
            options.HoleRefineBand.Should().Be(1.0);
            options.MinCapQuadQuality.Should().Be(0.6);
            options.OutputRejectedCapTriangles.Should().BeTrue();
        }

        /// <summary>Tests that high quality preset configures options correctly.</summary>
        [Fact]
        public void HighQualityPresetConfiguresCorrectly()
        {
            // Arrange & Act
            var options = MesherOptions.CreateBuilder()
                .WithHighQualityPreset()
                .Build();

            // Assert
            options.TargetEdgeLengthXY.Should().Be(0.5);
            options.TargetEdgeLengthZ.Should().Be(0.5);
            options.MinCapQuadQuality.Should().Be(0.7);
            options.OutputRejectedCapTriangles.Should().BeTrue();
        }

        /// <summary>Tests that fast preset configures options correctly.</summary>
        [Fact]
        public void FastPresetConfiguresCorrectly()
        {
            // Arrange & Act
            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build();

            // Assert
            options.TargetEdgeLengthXY.Should().Be(2.0);
            options.TargetEdgeLengthZ.Should().Be(2.0);
            options.MinCapQuadQuality.Should().Be(0.3);
            options.OutputRejectedCapTriangles.Should().BeFalse();
        }

        /// <summary>Tests that performance monitor tracks statistics correctly.</summary>
        [Fact]
        public void PerformanceMonitorTracksStatistics()
        {
            // Arrange
            var initialStats = PerformanceMonitor.Counters.GetStatistics();

            // Act
            PerformanceMonitor.Counters.IncrementMeshingOperations();
            PerformanceMonitor.Counters.AddQuadsGenerated(10);
            PerformanceMonitor.Counters.AddTrianglesGenerated(5);
            PerformanceMonitor.Counters.IncrementPoolHit();
            PerformanceMonitor.Counters.IncrementPoolMiss();

            var finalStats = PerformanceMonitor.Counters.GetStatistics();

            // Assert
            finalStats.MeshingOperations.Should().Be(initialStats.MeshingOperations + 1);
            finalStats.QuadsGenerated.Should().Be(initialStats.QuadsGenerated + 10);
            finalStats.TrianglesGenerated.Should().Be(initialStats.TrianglesGenerated + 5);
            finalStats.PoolHitRate.Should().BeGreaterThan(0.0);
        }

        /// <summary>Tests that performance monitor activity source works correctly.</summary>
        [Fact]
        public void PerformanceMonitorActivitySourceWorks()
        {
            // Arrange & Act - Test both with and without listener
            using var activity1 = PerformanceMonitor.StartMeshingActivity("TestOperation", new
            {
                EdgeLength = 1.0,
                QuadCount = 100
            });

            // Act with listener for full functionality test
            using var listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == "FastGeoMesh",
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(listener);

            using var activity2 = PerformanceMonitor.StartMeshingActivity("TestOperationWithListener", new
            {
                EdgeLength = 2.0,
                QuadCount = 200
            });

            // Assert - At least one activity should be created (fallback mechanism)
            // Either from the listener or from the fallback implementation
            activity1.Should().NotBeNull("Activity should be created via fallback mechanism");
            activity2.Should().NotBeNull("Activity should be created with listener present");

            if (activity1 != null)
            {
                activity1.OperationName.Should().Be("TestOperation");
            }

            if (activity2 != null)
            {
                activity2.OperationName.Should().Be("TestOperationWithListener");
            }
        }

        /// <summary>Tests that tessellation pool statistics work correctly.</summary>
        [Fact]
        public void TessPoolStatisticsWork()
        {
            // Arrange & Act
            var stats = TessPool.GetStatistics();

            // Assert
            stats.PooledCount.Should().BeGreaterThanOrEqualTo(0);
            stats.IsShuttingDown.Should().BeFalse();
        }

        /// <summary>Tests that quad quality helper optimized scoring works correctly.</summary>
        [Fact]
        public void QuadQualityHelperOptimizedScoringWorks()
        {
            // Arrange - Perfect square
            var perfectSquare = (
                new Vec2(0, 0), new Vec2(TestGeometries.UnitSquareSide, 0),
                new Vec2(TestGeometries.UnitSquareSide, TestGeometries.UnitSquareSide), new Vec2(0, TestGeometries.UnitSquareSide)
            );

            // Degenerate quad (very thin)
            var degenerateQuad = (
                new Vec2(0, 0), new Vec2(TestGeometries.StandardSquareSide, 0),
                new Vec2(TestGeometries.StandardSquareSide, 0.1), new Vec2(0, 0.1)
            );

            // Act
            var goodScore = QuadQualityHelper.ScoreQuad(perfectSquare);
            var badScore = QuadQualityHelper.ScoreQuad(degenerateQuad);

            // Assert - The optimized version should produce same results
            goodScore.Should().BeGreaterThanOrEqualTo(TestQualityThresholds.PerfectSquareMinQuality, "Perfect square must have high quality >= 0.8");
            badScore.Should().BeLessThan(TestQualityThresholds.MediumQualityThreshold, "Degenerate quad should have moderate quality");
            goodScore.Should().BeGreaterThan(badScore, "Good quad should score higher than bad quad");
        }

        /// <summary>Tests that async meshing interface is implementable.</summary>
        [Fact]
        public async Task AsyncMeshingInterfaceIsImplementable()
        {
            // Arrange
            var mesher = new TestAsyncMesher();
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0),
                new Vec2(TestGeometries.SmallSquareSide, 0),
                new Vec2(TestGeometries.SmallSquareSide, TestGeometries.SmallSquareSide),
                new Vec2(0, TestGeometries.SmallSquareSide)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build();

            // Act
            var mesh = await mesher.MeshAsync(structure, options, CancellationToken.None);

            // Assert
            mesh.Should().NotBeNull();
            mesh.Quads.Should().NotBeEmpty();
        }

        /// <summary>Tests that enhanced pools reuse objects correctly.</summary>
        [Fact]
        public void EnhancedPoolsReuseObjectsCorrectly()
        {
            // Arrange
            var list1 = MeshingPools.IntListPool.Get();
            var list2 = MeshingPools.DoubleListPool.Get();
            var vec2List = MeshingPools.Vec2ListPool.Get();

            // Act
            list1.Add(1);
            list1.Add(2);
            list2.Add(1.5);
            vec2List.Add(new Vec2(1, 2));

            MeshingPools.IntListPool.Return(list1);
            MeshingPools.DoubleListPool.Return(list2);
            MeshingPools.Vec2ListPool.Return(vec2List);

            var reusedList1 = MeshingPools.IntListPool.Get();
            var reusedList2 = MeshingPools.DoubleListPool.Get();
            var reusedVec2List = MeshingPools.Vec2ListPool.Get();

            // Assert
            reusedList1.Should().NotBeNull();
            reusedList1.Count.Should().Be(0, "Pooled list should be cleared");
            reusedList2.Count.Should().Be(0, "Pooled list should be cleared");
            reusedVec2List.Count.Should().Be(0, "Pooled list should be cleared");

            // Cleanup
            MeshingPools.IntListPool.Return(reusedList1);
            MeshingPools.DoubleListPool.Return(reusedList2);
            MeshingPools.Vec2ListPool.Return(reusedVec2List);
        }
    }
}
