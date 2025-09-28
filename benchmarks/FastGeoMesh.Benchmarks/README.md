# FastGeoMesh Benchmarks

Performance benchmarking suite for FastGeoMesh using BenchmarkDotNet.

## Running Benchmarks

Navigate to the benchmarks directory and run:

```bash
cd benchmarks/FastGeoMesh.Benchmarks
dotnet run -c Release -- [benchmark-name]
```

## Available Benchmarks

### Rectangle vs Tessellation
Compares performance of axis-aligned rectangle fast-path vs generic polygon tessellation:
```bash
dotnet run -c Release -- rectangle
```

### Mesh Size Impact
Tests performance scaling with geometry size and target edge lengths:
```bash
dotnet run -c Release -- size
```

### Quad Quality Scoring
Measures impact of quality thresholds on tessellation and triangulation fallback:
```bash
dotnet run -c Release -- quality
```

### IndexedMesh Operations
Benchmarks mesh conversion and adjacency building:
```bash
dotnet run -c Release -- indexed
```

### All Benchmarks
Run the complete benchmark suite:
```bash
dotnet run -c Release -- all
```

## Results Location

Benchmark results are saved to `BenchmarkDotNet.Artifacts/` directory with:
- Detailed HTML reports
- CSV/JSON data exports
- Performance plots (if supported)

## Typical Performance Expectations

- **Rectangle fast-path**: 10-50x faster than generic tessellation for simple rectangles
- **Memory allocation**: Rectangle path should have minimal GC pressure
- **Scaling**: Performance should scale roughly O(n) with target edge length reduction
- **Quality scoring**: Higher thresholds increase computation but may reduce final element count

## Adding New Benchmarks

1. Create a new class in `Program.cs` with `[MemoryDiagnoser]` and `[SimpleJob]` attributes
2. Add benchmark methods with `[Benchmark]` attribute
3. Use `[Params]` for parameterized benchmarks
4. Add the new benchmark to the main switch statement