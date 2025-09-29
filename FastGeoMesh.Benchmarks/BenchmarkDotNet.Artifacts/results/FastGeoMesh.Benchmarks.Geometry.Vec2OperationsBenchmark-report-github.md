```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method                      | Mean      | Error     | StdDev    | Median    | Min       | Max       | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| Vec2Addition_Optimized      | 11.434 μs | 0.8280 μs | 2.4413 μs | 10.710 μs |  8.728 μs | 17.512 μs |  1.04 |    0.30 |       - |       - |       - |         - |          NA |
| Vec2Addition_NonOptimized   | 19.158 μs | 0.9139 μs | 2.5627 μs | 18.117 μs | 16.764 μs | 26.548 μs |  1.74 |    0.41 |       - |       - |       - |         - |          NA |
| Vec2DotProduct_Optimized    |  7.918 μs | 0.1582 μs | 0.3850 μs |  7.791 μs |  7.493 μs |  9.010 μs |  0.72 |    0.14 |       - |       - |       - |         - |          NA |
| Vec2DotProduct_NonOptimized | 15.571 μs | 0.6139 μs | 1.7514 μs | 15.001 μs | 13.522 μs | 20.642 μs |  1.42 |    0.31 |       - |       - |       - |         - |          NA |
| Vec2AccumulateDot_Batch     |  6.040 μs | 0.0995 μs | 0.1329 μs |  5.984 μs |  5.906 μs |  6.340 μs |  0.55 |    0.11 |       - |       - |       - |         - |          NA |
| Vec2AccumulateDot_Loop      |  7.347 μs | 0.0945 μs | 0.0884 μs |  7.334 μs |  7.230 μs |  7.548 μs |  0.67 |    0.13 |       - |       - |       - |         - |          NA |
| Vec2Add_Batch               |  6.399 μs | 0.0950 μs | 0.0742 μs |  6.374 μs |  6.327 μs |  6.580 μs |  0.58 |    0.11 |       - |       - |       - |         - |          NA |
| Vec2Add_Loop                | 10.023 μs | 0.1355 μs | 0.1131 μs | 10.003 μs |  9.849 μs | 10.255 μs |  0.91 |    0.17 |       - |       - |       - |         - |          NA |
| Vec2Length_Optimized        | 12.746 μs | 0.2509 μs | 0.2347 μs | 12.727 μs | 12.430 μs | 13.333 μs |  1.16 |    0.22 |       - |       - |       - |         - |          NA |
| Vec2LengthSquared_Optimized |  5.860 μs | 0.3894 μs | 1.1359 μs |  5.360 μs |  4.883 μs |  8.902 μs |  0.53 |    0.15 |       - |       - |       - |         - |          NA |
| Vec2Normalize_Optimized     | 62.837 μs | 1.2275 μs | 1.0881 μs | 62.897 μs | 60.462 μs | 64.839 μs |  5.72 |    1.09 | 49.9268 | 49.9268 | 49.9268 |  160041 B |          NA |
| Vec2Cross_Optimized         |  7.743 μs | 0.1505 μs | 0.1848 μs |  7.671 μs |  7.542 μs |  8.192 μs |  0.71 |    0.14 |       - |       - |       - |         - |          NA |
