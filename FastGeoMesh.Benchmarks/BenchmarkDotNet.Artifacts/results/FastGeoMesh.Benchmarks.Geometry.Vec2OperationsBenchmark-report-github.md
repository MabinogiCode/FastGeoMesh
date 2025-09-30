```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method                      | Mean      | Error    | StdDev   | Median    | Min       | Max       | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|---------------------------- |----------:|---------:|---------:|----------:|----------:|----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| Vec2Addition_Optimized      |  14.39 μs | 1.122 μs | 3.309 μs |  14.66 μs |  9.268 μs |  19.42 μs |  1.06 |    0.36 |       - |       - |       - |         - |          NA |
| Vec2Addition_NonOptimized   |  28.51 μs | 1.448 μs | 4.271 μs |  29.22 μs | 18.761 μs |  33.47 μs |  2.09 |    0.60 |       - |       - |       - |         - |          NA |
| Vec2DotProduct_Optimized    |  17.52 μs | 0.571 μs | 1.675 μs |  17.87 μs |  8.630 μs |  18.04 μs |  1.29 |    0.33 |       - |       - |       - |         - |          NA |
| Vec2DotProduct_NonOptimized |  28.10 μs | 0.561 μs | 1.608 μs |  28.40 μs | 16.998 μs |  28.60 μs |  2.06 |    0.51 |       - |       - |       - |         - |          NA |
| Vec2AccumulateDot_Batch     |  12.98 μs | 0.633 μs | 1.847 μs |  13.56 μs |  6.708 μs |  14.07 μs |  0.95 |    0.27 |       - |       - |       - |         - |          NA |
| Vec2AccumulateDot_Loop      |  15.36 μs | 0.307 μs | 0.871 μs |  15.32 μs | 11.625 μs |  17.30 μs |  1.13 |    0.28 |       - |       - |       - |         - |          NA |
| Vec2Add_Batch               |  16.41 μs | 0.312 μs | 0.306 μs |  16.34 μs | 15.920 μs |  17.00 μs |  1.21 |    0.29 |       - |       - |       - |         - |          NA |
| Vec2Add_Loop                |  19.85 μs | 0.362 μs | 0.739 μs |  19.42 μs | 19.065 μs |  22.16 μs |  1.46 |    0.36 |       - |       - |       - |         - |          NA |
| Vec2Length_Optimized        |  35.19 μs | 0.417 μs | 0.390 μs |  35.14 μs | 34.588 μs |  36.14 μs |  2.58 |    0.62 |       - |       - |       - |         - |          NA |
| Vec2LengthSquared_Optimized |  11.15 μs | 0.171 μs | 0.160 μs |  11.17 μs | 10.754 μs |  11.35 μs |  0.82 |    0.20 |       - |       - |       - |         - |          NA |
| Vec2Normalize_Optimized     | 116.67 μs | 2.331 μs | 5.164 μs | 116.59 μs | 82.169 μs | 123.65 μs |  8.57 |    2.10 | 49.9268 | 49.9268 | 49.9268 |  160041 B |          NA |
| Vec2Cross_Optimized         |  17.81 μs | 0.813 μs | 2.331 μs |  17.86 μs |  8.583 μs |  20.75 μs |  1.31 |    0.36 |       - |       - |       - |         - |          NA |
