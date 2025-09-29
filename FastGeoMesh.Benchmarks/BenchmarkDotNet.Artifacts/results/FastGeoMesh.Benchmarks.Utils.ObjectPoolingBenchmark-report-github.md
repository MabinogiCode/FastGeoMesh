```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.6584)
Intel Core Ultra 7 165U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.304
  [Host] : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2


```
| Method                       | Mean | Error | Min | Max | Median | Ratio | RatioSD | Alloc Ratio |
|----------------------------- |-----:|------:|----:|----:|-------:|------:|--------:|------------:|
| ObjectPooling_IntList_Pooled |   NA |    NA |  NA |  NA |     NA |     ? |       ? |           ? |

Benchmarks with issues:
  ObjectPoolingBenchmark.ObjectPooling_IntList_Pooled: DefaultJob
