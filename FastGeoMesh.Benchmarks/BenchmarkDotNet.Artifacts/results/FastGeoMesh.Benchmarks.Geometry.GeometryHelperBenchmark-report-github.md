```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method                           | Mean         | Error       | StdDev      | Median       | Min          | Max          | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |-------------:|------------:|------------:|-------------:|-------------:|-------------:|------:|-------:|----------:|------------:|
| PointInPolygon_OptimizedSpan     | 275,646.3 ns |   246.31 ns |   230.40 ns | 275,611.0 ns | 275,204.7 ns | 276,079.0 ns | 1.000 |      - |         - |          NA |
| PointInPolygon_NonOptimized      | 161,121.8 ns | 1,244.15 ns | 1,102.90 ns | 161,362.0 ns | 158,267.2 ns | 162,325.6 ns | 0.585 |      - |         - |          NA |
| BatchPointInPolygon_Optimized    | 275,958.9 ns |   676.29 ns |   632.60 ns | 275,930.3 ns | 274,933.3 ns | 277,122.7 ns | 1.001 |      - |    1024 B |          NA |
| PolygonArea_OptimizedSpan        |     180.2 ns |     0.39 ns |     0.37 ns |     180.3 ns |     179.2 ns |     180.7 ns | 0.001 |      - |         - |          NA |
| PolygonArea_NonOptimized         |     177.1 ns |     1.18 ns |     1.04 ns |     177.5 ns |     174.7 ns |     178.1 ns | 0.001 |      - |         - |          NA |
| DistancePointToSegment_Optimized |   6,526.1 ns |   208.84 ns |   615.77 ns |   6,490.0 ns |   2,948.6 ns |   7,705.1 ns | 0.024 |      - |         - |          NA |
| LinearInterpolation_Optimized    |   2,542.2 ns |    74.06 ns |   218.38 ns |   2,545.6 ns |   1,336.6 ns |   2,856.6 ns | 0.009 |      - |         - |          NA |
| ScalarInterpolation_Optimized    |   2,325.6 ns |   131.85 ns |   363.16 ns |   2,428.9 ns |     932.3 ns |   2,643.9 ns | 0.008 |      - |         - |          NA |
| ConvexityTest_Optimized          |   2,983.7 ns |    62.49 ns |   170.00 ns |   3,024.8 ns |   1,967.6 ns |   3,062.1 ns | 0.011 | 0.0420 |     280 B |          NA |
