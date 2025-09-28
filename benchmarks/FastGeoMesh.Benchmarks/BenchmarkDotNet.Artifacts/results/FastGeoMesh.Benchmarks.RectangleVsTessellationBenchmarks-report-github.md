```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method              | Mean     | Error    | StdDev    | Median   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|-------------------- |---------:|---------:|----------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
| RectangleFastPath   | 28.98 μs | 7.276 μs | 21.454 μs | 14.52 μs |  1.60 |    1.58 | 17.9596 | 5.3864 | 110.11 KB |        1.00 |
| HexagonTessellation | 18.96 μs | 1.468 μs |  4.328 μs | 21.01 μs |  1.05 |    0.61 |  9.6130 | 1.3428 |  59.06 KB |        0.54 |
