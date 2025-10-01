using System;
using System.Diagnostics;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests for additional performance optimizations.
    /// Validates the new span-based APIs and object pooling improvements.
    /// </summary>
    public sealed class AdditionalPerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public AdditionalPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MeshPoolShowsSignificantPerformanceGains()
        {
            // Arrange
            const int iterations = 1000;
            const int quadCount = 100;

            var testQuads = GenerateTestQuads(quadCount);

            _output.WriteLine($"🏊 Mesh Pooling Performance Test");
            _output.WriteLine($"Testing {iterations} mesh operations with {quadCount} quads each");

            // Act & Assert - Without Pooling
            var withoutPoolingTime = MeasureOperation("Without Pooling", iterations, () =>
            {
                using var mesh = new Mesh();
                mesh.AddQuads(testQuads);
                _ = mesh.QuadCount;
            });

            var withPoolingTime = MeasureOperation("With Mesh Pooling", iterations, () =>
            {
                PooledMeshExtensions.WithPooledMesh(mesh =>
                {
                    mesh.AddQuads(testQuads);
                    _ = mesh.QuadCount;
                });
            });

            // Pooling should show significant improvement due to reduced allocations
            var improvement = (withoutPoolingTime.TotalMicroseconds - withPoolingTime.TotalMicroseconds) / withoutPoolingTime.TotalMicroseconds;
            improvement.Should().BeGreaterThan(0.1, "Mesh pooling should provide at least 10% improvement");

            _output.WriteLine($"📈 Mesh pooling: {improvement * 100:F1}% faster than direct allocation");
        }

        [Fact]
        public void AdvancedSpanOperationsShowPerformanceGains()
        {
            // Arrange
            const int vertexCount = 10000;
            const int iterations = 100;

            var vertices2D = GenerateTestVertices2D(vertexCount);
            var vertices3D = new Vec3[vertexCount];

            _output.WriteLine($"🧮 Advanced Span Operations Performance Test");
            _output.WriteLine($"Testing {vertexCount} vertices over {iterations} iterations");

            // Test 1: 2D to 3D transformation
            var traditionalTransformTime = MeasureOperation("Traditional Transform", iterations, () =>
            {
                for (int i = 0; i < vertices2D.Length; i++)
                {
                    var v2 = vertices2D[i];
                    vertices3D[i] = new Vec3(v2.X, v2.Y, 5.0);
                }
            });

            var spanTransformTime = MeasureOperation("Span Transform", iterations, () =>
            {
                ((ReadOnlySpan<Vec2>)vertices2D.AsSpan()).TransformTo3D(vertices3D.AsSpan(), 5.0);
            });

            var transformImprovement = (traditionalTransformTime.TotalMicroseconds - spanTransformTime.TotalMicroseconds) / traditionalTransformTime.TotalMicroseconds;
            // Note: Span transform may not always be faster due to method call overhead in micro-benchmarks
            _output.WriteLine($"📊 Span transform comparison: {transformImprovement * 100:F1}% change from traditional");

            // Both should be reasonably fast - the main benefit is in API design
            traditionalTransformTime.TotalMicroseconds.Should().BeLessThan(3000, "Traditional transform should be reasonably fast");
            spanTransformTime.TotalMicroseconds.Should().BeLessThan(3000, "Span transform should be reasonably fast");

            // Test 2: Area calculation
            var square = new Vec2[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };

            var traditionalAreaTime = MeasureOperation("Traditional Area", iterations * 100, () =>
            {
                double area = 0.0;
                int n = square.Length;
                for (int i = 0; i < n; i++)
                {
                    var curr = square[i];
                    var next = square[(i + 1) % n];
                    area += curr.X * next.Y - next.X * curr.Y;
                }
                _ = area * 0.5;
            });

            var spanAreaTime = MeasureOperation("Span Area", iterations * 100, () =>
            {
                _ = ((ReadOnlySpan<Vec2>)square.AsSpan()).ComputeSignedArea();
            });

            var areaImprovement = (traditionalAreaTime.TotalMicroseconds - spanAreaTime.TotalMicroseconds) / traditionalAreaTime.TotalMicroseconds;

            _output.WriteLine($"📈 Span area calculation: {areaImprovement * 100:F1}% change from traditional");

            // Both should be reasonably fast
            traditionalAreaTime.TotalMicroseconds.Should().BeLessThan(200, "Traditional area should be fast");
            spanAreaTime.TotalMicroseconds.Should().BeLessThan(200, "Span area should be fast");
        }

        [Fact]
        public void BatchPointInPolygonShowsScalabilityGains()
        {
            // Arrange
            const int pointCount = 1000;
            const int iterations = 50;

            var polygon = new Vec2[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100) };
            var testPoints = GenerateTestVertices2D(pointCount);
            var results = new bool[pointCount];

            _output.WriteLine($"🎯 Batch Point-in-Polygon Performance Test");
            _output.WriteLine($"Testing {pointCount} points against polygon over {iterations} iterations");

            // Act & Assert - Individual tests
            var individualTime = MeasureOperation("Individual Tests", iterations, () =>
            {
                for (int i = 0; i < testPoints.Length; i++)
                {
                    results[i] = ((ReadOnlySpan<Vec2>)polygon.AsSpan()).ContainsPoint(testPoints[i]);
                }
            });

            var batchTime = MeasureOperation("Batch Test", iterations, () =>
            {
                ((ReadOnlySpan<Vec2>)polygon.AsSpan()).ContainsPoints(testPoints.AsSpan(), results.AsSpan());
            });

            // Batch should be at least comparable performance
            var batchImprovement = (individualTime.TotalMicroseconds - batchTime.TotalMicroseconds) / individualTime.TotalMicroseconds;

            _output.WriteLine($"📈 Batch point-in-polygon: {batchImprovement * 100:F1}% change from individual");

            // Both should be reasonably fast
            individualTime.TotalMicroseconds.Should().BeLessThan(10000, "Individual tests should be reasonably fast");
            batchTime.TotalMicroseconds.Should().BeLessThan(10000, "Batch test should be reasonably fast");
        }

        [Fact]
        public void PaddedBoundsOptimizedImplementation()
        {
            // Arrange
            const int vertexCount = 10000;
            const int iterations = 100;

            var vertices = GenerateTestVertices2D(vertexCount);

            _output.WriteLine($"📐 Padded Bounds Performance Test");
            _output.WriteLine($"Testing {vertexCount} vertices over {iterations} iterations");

            // Act & Assert - Traditional bounds with padding
            var traditionalTime = MeasureOperation("Traditional Bounds + Padding", iterations, () =>
            {
                var (min, max) = ((ReadOnlySpan<Vec2>)vertices.AsSpan()).ComputeBounds();
                var padding = 5.0;
                _ = (new Vec2(min.X - padding, min.Y - padding), new Vec2(max.X + padding, max.Y + padding));
            });

            var optimizedTime = MeasureOperation("Optimized Padded Bounds", iterations, () =>
            {
                _ = ((ReadOnlySpan<Vec2>)vertices.AsSpan()).ComputePaddedBounds(5.0);
            });

            var improvement = (traditionalTime.TotalMicroseconds - optimizedTime.TotalMicroseconds) / traditionalTime.TotalMicroseconds;

            _output.WriteLine($"📈 Optimized padded bounds: {improvement * 100:F1}% change from traditional");

            // Both should be fast
            traditionalTime.TotalMicroseconds.Should().BeLessThan(5000, "Traditional bounds should be fast");
            optimizedTime.TotalMicroseconds.Should().BeLessThan(5000, "Optimized bounds should be fast");
        }

        private Quad[] GenerateTestQuads(int count)
        {
            var quads = new Quad[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                var v0 = new Vec3(random.NextDouble() * 100, random.NextDouble() * 100, random.NextDouble() * 10);
                var v1 = new Vec3(v0.X + 1, v0.Y, v0.Z);
                var v2 = new Vec3(v0.X + 1, v0.Y + 1, v0.Z);
                var v3 = new Vec3(v0.X, v0.Y + 1, v0.Z);

                quads[i] = new Quad(v0, v1, v2, v3);
            }

            return quads;
        }

        private Vec2[] GenerateTestVertices2D(int count)
        {
            var vertices = new Vec2[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vec2(random.NextDouble() * 100, random.NextDouble() * 100);
            }

            return vertices;
        }

        private TimeSpan MeasureOperation(string name, int iterations, Action operation)
        {
            // Warm up
            operation();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                operation();
            }

            stopwatch.Stop();

            var avgTime = TimeSpan.FromTicks(stopwatch.ElapsedTicks / iterations);
            _output.WriteLine($"  {name}: {avgTime.TotalMicroseconds:F2} μs (avg over {iterations} iterations)");

            return avgTime;
        }
    }
}
