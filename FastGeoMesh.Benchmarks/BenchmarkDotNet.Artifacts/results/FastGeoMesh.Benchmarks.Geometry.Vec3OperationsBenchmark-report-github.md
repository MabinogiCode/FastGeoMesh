```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method                        | Mean      | Error     | StdDev    | Median    | Min        | Max       | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|------------------------------ |----------:|----------:|----------:|----------:|-----------:|----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| Vec3Addition_Optimized        |  18.25 μs |  1.847 μs |  5.446 μs |  18.24 μs |  10.806 μs |  33.49 μs |  1.10 |    0.48 |       - |       - |       - |         - |          NA |
| Vec3Addition_NonOptimized     |  92.77 μs | 11.006 μs | 32.452 μs | 116.98 μs |  35.862 μs | 156.95 μs |  5.58 |    2.65 |       - |       - |       - |         - |          NA |
| Vec3DotProduct_Optimized      |  17.83 μs |  2.108 μs |  6.215 μs |  20.37 μs |   9.308 μs |  36.54 μs |  1.07 |    0.51 |       - |       - |       - |         - |          NA |
| Vec3AccumulateDot_Batch       |  19.71 μs |  0.993 μs |  2.865 μs |  19.85 μs |   9.953 μs |  23.65 μs |  1.18 |    0.40 |       - |       - |       - |         - |          NA |
| Vec3AccumulateDot_Loop        |  16.96 μs |  0.339 μs |  0.594 μs |  16.71 μs |  16.420 μs |  18.32 μs |  1.02 |    0.31 |       - |       - |       - |         - |          NA |
| Vec3Add_Batch                 |  19.69 μs |  0.692 μs |  2.042 μs |  19.50 μs |  10.225 μs |  23.11 μs |  1.18 |    0.38 |       - |       - |       - |         - |          NA |
| Vec3Add_Loop                  |  22.73 μs |  0.633 μs |  1.836 μs |  22.76 μs |  14.603 μs |  25.24 μs |  1.37 |    0.43 |       - |       - |       - |         - |          NA |
| Vec3Cross_Batch               |  25.19 μs |  0.478 μs |  0.798 μs |  24.92 μs |  22.435 μs |  26.75 μs |  1.51 |    0.46 |       - |       - |       - |         - |          NA |
| Vec3Cross_Loop                |  26.39 μs |  0.921 μs |  2.567 μs |  26.98 μs |  13.861 μs |  27.45 μs |  1.59 |    0.51 |       - |       - |       - |         - |          NA |
| Vec3CrossProduct_Optimized    | 109.98 μs |  2.352 μs |  6.596 μs | 107.30 μs | 103.161 μs | 132.34 μs |  6.61 |    2.06 | 71.4111 | 71.4111 | 71.4111 |  240024 B |          NA |
| Vec3CrossProduct_NonOptimized | 167.00 μs |  3.066 μs |  2.868 μs | 167.38 μs | 162.494 μs | 171.09 μs | 10.04 |    3.07 | 71.2891 | 71.2891 | 71.2891 |  240024 B |          NA |
| Vec3Length_Optimized          |  35.59 μs |  0.086 μs |  0.072 μs |  35.58 μs |  35.437 μs |  35.72 μs |  2.14 |    0.65 |       - |       - |       - |         - |          NA |
| Vec3LengthSquared_Optimized   |  13.72 μs |  0.487 μs |  1.435 μs |  13.49 μs |   7.481 μs |  15.86 μs |  0.82 |    0.27 |       - |       - |       - |         - |          NA |
| Vec3Normalize_Optimized       | 142.86 μs |  3.935 μs | 11.602 μs | 138.63 μs | 117.378 μs | 167.26 μs |  8.59 |    2.72 | 71.2891 | 71.2891 | 71.2891 |  240048 B |          NA |
| Vec3Scaling_Optimized         |  13.22 μs |  0.409 μs |  1.093 μs |  13.44 μs |   7.056 μs |  13.79 μs |  0.79 |    0.25 |       - |       - |       - |         - |          NA |
| Vec3Scaling_Commutative       |  13.35 μs |  0.108 μs |  0.096 μs |  13.35 μs |  13.092 μs |  13.48 μs |  0.80 |    0.25 |       - |       - |       - |         - |          NA |
