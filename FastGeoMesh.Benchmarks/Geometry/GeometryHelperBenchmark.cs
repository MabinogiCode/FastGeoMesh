using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using FastGeoMesh.Utils;
using System.Runtime.CompilerServices;

namespace FastGeoMesh.Benchmarks.Geometry;

/// <summary>
/// Benchmarks for GeometryHelper operations comparing optimized vs non-optimized implementations.
/// Tests the impact of ReadOnlySpan, AggressiveOptimization, and vectorization-friendly code.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class GeometryHelperBenchmark
{
    private Vec2[] _polygonVertices = null!;
    private Vec2[] _testPoints = null!;
    private const int PolygonSize = 100;
    private const int TestPointCount = 1000;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        
        // Create a complex polygon
        _polygonVertices = new Vec2[PolygonSize];
        for (int i = 0; i < PolygonSize; i++)
        {
            double angle = 2 * Math.PI * i / PolygonSize;
            double radius = 50 + 20 * Math.Sin(5 * angle); // Star-like shape
            _polygonVertices[i] = new Vec2(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle)
            );
        }

        // Create test points
        _testPoints = new Vec2[TestPointCount];
        for (int i = 0; i < TestPointCount; i++)
        {
            _testPoints[i] = new Vec2(
                random.NextDouble() * 200 - 100,
                random.NextDouble() * 200 - 100
            );
        }
    }

    [Benchmark(Baseline = true)]
    public int PointInPolygon_OptimizedSpan()
    {
        int insideCount = 0;
        ReadOnlySpan<Vec2> polygonSpan = _polygonVertices.AsSpan();
        
        for (int i = 0; i < _testPoints.Length; i++)
        {
            if (GeometryHelper.PointInPolygon(polygonSpan, _testPoints[i]))
            {
                insideCount++;
            }
        }
        return insideCount;
    }

    [Benchmark]
    public int PointInPolygon_NonOptimized()
    {
        int insideCount = 0;
        
        for (int i = 0; i < _testPoints.Length; i++)
        {
            if (PointInPolygonSlow(_polygonVertices, _testPoints[i]))
            {
                insideCount++;
            }
        }
        return insideCount;
    }

    [Benchmark]
    public bool[] BatchPointInPolygon_Optimized()
    {
        var results = new bool[_testPoints.Length];
        ReadOnlySpan<Vec2> polygonSpan = _polygonVertices.AsSpan();
        ReadOnlySpan<Vec2> pointsSpan = _testPoints.AsSpan();
        Span<bool> resultsSpan = results.AsSpan();
        
        GeometryHelper.BatchPointInPolygon(polygonSpan, pointsSpan, resultsSpan);
        return results;
    }

    [Benchmark]
    public double PolygonArea_OptimizedSpan()
    {
        ReadOnlySpan<Vec2> polygonSpan = _polygonVertices.AsSpan();
        return GeometryHelper.PolygonArea(polygonSpan);
    }

    [Benchmark]
    public double PolygonArea_NonOptimized()
    {
        return PolygonAreaSlow(_polygonVertices);
    }

    [Benchmark]
    public double DistancePointToSegment_Optimized()
    {
        double totalDistance = 0;
        var segmentStart = new Vec2(-10, -10);
        var segmentEnd = new Vec2(10, 10);
        
        for (int i = 0; i < _testPoints.Length; i++)
        {
            totalDistance += GeometryHelper.DistancePointToSegment(_testPoints[i], segmentStart, segmentEnd);
        }
        return totalDistance;
    }

    [Benchmark]
    public double LinearInterpolation_Optimized()
    {
        double sum = 0;
        var startPoint = Vec2.Zero;
        var endPoint = Vec2.UnitX;
        
        for (int i = 0; i < _testPoints.Length; i++)
        {
            double t = (double)i / _testPoints.Length;
            var lerped = GeometryHelper.Lerp(startPoint, endPoint, t);
            sum += lerped.X + lerped.Y;
        }
        return sum;
    }

    [Benchmark]
    public double ScalarInterpolation_Optimized()
    {
        double sum = 0;
        
        for (int i = 0; i < _testPoints.Length; i++)
        {
            double t = (double)i / _testPoints.Length;
            sum += GeometryHelper.LerpScalar(0.0, 100.0, t);
        }
        return sum;
    }

    [Benchmark]
    public bool[] ConvexityTest_Optimized()
    {
        var results = new bool[_testPoints.Length / 4];
        
        for (int i = 0; i < results.Length; i++)
        {
            int baseIndex = i * 4;
            var quad = (
                _testPoints[baseIndex % _testPoints.Length],
                _testPoints[(baseIndex + 1) % _testPoints.Length],
                _testPoints[(baseIndex + 2) % _testPoints.Length],
                _testPoints[(baseIndex + 3) % _testPoints.Length]
            );
            results[i] = GeometryHelper.IsConvex(quad);
        }
        return results;
    }

    // Non-optimized reference implementations for comparison
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool PointInPolygonSlow(Vec2[] vertices, Vec2 point)
    {
        int n = vertices.Length;
        bool inside = false;
        
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) &&
                (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double PolygonAreaSlow(Vec2[] vertices)
    {
        double area = 0.0;
        int n = vertices.Length;
        
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += vertices[i].X * vertices[j].Y;
            area -= vertices[j].X * vertices[i].Y;
        }
        return Math.Abs(area) * 0.5;
    }
}
