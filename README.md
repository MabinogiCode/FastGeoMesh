# FastGeoMesh

**Fast quad meshing for prismatic volumes from 2D footprints and Z elevations.**

[![CI](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/MabinogiCode/FastGeoMesh/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastGeoMesh)
[![NuGet](https://img.shields.io/nuget/v/FastGeoMesh.svg)](https://www.nuget.org/packages/FastGeoMesh/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

## ⚡ Performance Highlights

FastGeoMesh delivers **sub-millisecond meshing** with .NET 8 optimizations:

### 🏗️ **Meshing Performance** (BenchmarkDotNet validated)
| Scenario | Execution Time | Memory | Performance Class |
|----------|---------------|--------|-------------------|
| **Simple Prism** | **~305 μs** | **87 KB** | ⚡ **Ultra-Fast** |
| **Complex Geometry** | ~340 μs | 87 KB | ⚡ **Very Fast** |
| **With Holes (Fast)** | ~907 μs | 1.3 MB | ✅ **Fast** |
| **High Quality** | 1.3-8.7 ms | 17-87 MB | 🔬 **Precision** |

### 🔧 **Geometry Operations** (Zero allocations)
- **PointInPolygon**: ~161 μs | **PolygonArea**: ~180 ns  
- **DistancePointToSegment**: ~6.5 μs | **Linear Interpolation**: ~2.5 μs

## 🚀 Features

- **🏗️ Prism mesher** (side faces + caps)
- **📐 Rectangle fast-path** + generic tessellation  
- **🎯 Quad quality scoring** & thresholds (MinCapQuadQuality)
- **📑 Multiple exporters**: OBJ (quads+triangles), glTF (triangulated), SVG (top view)
- **⚙️ Constraint Z levels** & geometry integration
- **⚡ .NET 8 optimized**: TieredPGO, validation caching, object pooling

## 📦 Installation

```bash
dotnet add package FastGeoMesh
```

## 🚀 Quick Start

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// Define a 20x5 meter prism, 20m high
var poly = Polygon2D.FromPoints(new[]
{
    new Vec2(0, 0), new Vec2(20, 0), 
    new Vec2(20, 5), new Vec2(0, 5)
});

var structure = new PrismStructureDefinition(poly, -10, 10);
structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(20, 0)), 2.5);

// Configure for optimal performance (~305μs, 87KB)
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()                    // Optimized for speed
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithRejectedCapTriangles(true)
    .Build();

// Generate mesh
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

// Export to multiple formats
ObjExporter.Write(indexed, "mesh.obj");      // Quads + triangles
GltfExporter.Write(indexed, "mesh.gltf");    // Triangulated
SvgExporter.Write(indexed, "mesh.svg");      // Top view
```

## 🔧 Performance Presets

```csharp
// Fast preset: ~305μs, 87KB - Real-time applications
var fastOptions = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

// High-quality preset: ~1.3ms, 17MB - CAD/Engineering precision
var qualityOptions = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .Build();
```

## 🧪 Benchmarks

Validate performance locally:

```bash
cd FastGeoMesh.Benchmarks
dotnet run -c Release -- --geometry    # Vec2/Vec3 operations  
dotnet run -c Release -- --meshing     # Mesh generation
dotnet run -c Release -- --all         # Complete benchmark suite
```

**Environment**: .NET 8.0.20, X64 RyuJIT AVX2, BenchmarkDotNet 0.14.0

## 📊 Advanced Usage

### Complex Geometries with Holes
```csharp
var outer = Polygon2D.FromPoints(/* outer boundary */);
var hole = Polygon2D.FromPoints(/* hole geometry */);

var structure = new PrismStructureDefinition(outer, 0, 10)
    .AddHole(hole);                           // ~907μs with holes

var options = MesherOptions.CreateBuilder()
    .WithHoleRefinement(0.25, 1.0)           // Refine near holes
    .Build();
```

### Async Meshing for Large Datasets
```csharp
var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
var mesh = await mesher.MeshAsync(structure, options, cancellationToken);
```

## 🏗️ Architecture

Built with .NET 8 performance optimizations:
- **Readonly structs** for Vec2/Vec3 (reduced IL overhead)  
- **Validation caching** (~90% faster repeated calls)
- **Object pooling** (zero allocations for geometry operations)
- **TieredPGO** for hot-path optimizations

## 🧪 Testing

Comprehensive test coverage with **131 passing tests**:

```bash
dotnet test                                    # Run all tests
dotnet test --filter "Performance"            # Performance validation
```

## 📄 License

MIT License

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feat/amazing-feature`  
3. Run tests: `dotnet test`
4. Run benchmarks: `dotnet run -c Release --project FastGeoMesh.Benchmarks -- --all`
5. Submit a Pull Request

---

**Fast, precise, and .NET 8 optimized.** 🚀

Performance data validated with comprehensive benchmarks. See [FastGeoMesh.Benchmarks](./FastGeoMesh.Benchmarks) for detailed measurements.
