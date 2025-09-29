```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method                           | Mean         | Error       | StdDev      | Median       | Min          | Max          | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |-------------:|------------:|------------:|-------------:|-------------:|-------------:|------:|--------:|-------:|----------:|------------:|
| PointInPolygon_OptimizedSpan     | 177,439.7 ns | 2,857.51 ns | 2,672.92 ns | 176,684.8 ns | 174,731.4 ns | 183,151.6 ns | 1.000 |    0.02 |      - |         - |          NA |
| PointInPolygon_NonOptimized      | 136,738.0 ns |   590.19 ns |   523.19 ns | 136,780.0 ns | 135,508.7 ns | 137,478.7 ns | 0.771 |    0.01 |      - |         - |          NA |
| BatchPointInPolygon_Optimized    | 178,455.5 ns | 3,169.78 ns | 4,934.96 ns | 176,738.4 ns | 173,192.3 ns | 192,918.7 ns | 1.006 |    0.03 |      - |    1024 B |          NA |
| PolygonArea_OptimizedSpan        |     128.3 ns |     2.13 ns |     1.99 ns |     127.8 ns |     125.8 ns |     132.0 ns | 0.001 |    0.00 |      - |         - |          NA |
| PolygonArea_NonOptimized         |     127.5 ns |     2.31 ns |     1.81 ns |     127.0 ns |     125.3 ns |     131.9 ns | 0.001 |    0.00 |      - |         - |          NA |
| DistancePointToSegment_Optimized |   4,383.8 ns |   280.40 ns |   826.77 ns |   4,176.8 ns |   2,865.8 ns |   6,400.0 ns | 0.025 |    0.00 |      - |         - |          NA |
| LinearInterpolation_Optimized    |     925.3 ns |    18.30 ns |    39.39 ns |     908.7 ns |     883.2 ns |   1,040.9 ns | 0.005 |    0.00 |      - |         - |          NA |
| ScalarInterpolation_Optimized    |     863.6 ns |    17.03 ns |    35.17 ns |     847.2 ns |     829.5 ns |     960.6 ns | 0.005 |    0.00 |      - |         - |          NA |
| ConvexityTest_Optimized          |   1,421.0 ns |    28.28 ns |    55.16 ns |   1,401.3 ns |   1,369.3 ns |   1,587.5 ns | 0.008 |    0.00 | 0.0439 |     280 B |          NA |
