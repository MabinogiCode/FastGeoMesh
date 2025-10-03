# FastGeoMesh

[![CI](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/MabinogiCode/FastGeoMesh/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastGeoMesh)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/FastGeoMesh.svg)](https://www.nuget.org/packages/FastGeoMesh/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**⚡ Ultra-fast quad meshing for prismatic volumes with async/parallel capabilities**

FastGeoMesh is a high-performance .NET 8 library for generating quad-dominant meshes from 2.5D prismatic structures. Perfect for CAD, GIS, and real-time applications requiring sub-millisecond meshing performance with modern async patterns.

## 🚀 **What's New in v1.4.0-rc1**

### **Async/Parallel Breakthroughs**
- 🔥 **78% faster async** for trivial structures (311μs vs 1433μs sync)
- 🔥 **2.2x parallel speedup** on batch processing (32 structures)
- 🔥 **639ns monitoring overhead** (negligible impact)
- 🔥 **100% backward compatible** with existing sync code

### **New Capabilities**
- ✅ **Complete async/parallel interface** (`IAsyncMesher`)
- ✅ **Real-time progress reporting** with ETA and status
- ✅ **Batch processing** with configurable parallelism
- ✅ **Performance monitoring** and complexity estimation
- ✅ **Robust cancellation support** throughout operations

## ⚡ **Performance (Benchmarked v1.4.0-rc1)**

**Sub-millisecond meshing** with .NET 8 optimizations + async improvements:

| Operation | Sync | Async | Improvement |
|-----------|------|-------|-------------|
| **Trivial structures** | 1433μs | **311μs** | **78% faster** 🔥 |
| **Simple structures** | 348μs | **202μs** | **42% faster** 🔥 |
| **Batch 32 structures** | 7.4ms | **3.3ms** | **2.2x speedup** 🚀 |
| **Performance stats** | N/A | **639ns** | **New feature** ✨ |
| **Complexity estimation** | N/A | **0.8μs** | **New feature** ✨ |

*Benchmarked on .NET 8.0.20, X64 RyuJIT AVX2*

## 🚀 **Quick Start (Sync - Existing Code Works!)**

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// Your existing v1.3.2 code works unchanged!
var poly = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) 
});
var structure = new PrismStructureDefinition(poly, -10, 10);

var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(0.5)
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh);

// Export to multiple formats
ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
```

## 🔥 **New Async Capabilities (v1.4.0)**

```csharp
var mesher = new PrismMesher();
var asyncMesher = (IAsyncMesher)mesher;

// 🚀 Basic async meshing (often faster than sync!)
var mesh = await asyncMesher.MeshAsync(structure, options);

// 🚀 Progress reporting with detailed tracking
var progress = new Progress<MeshingProgress>(p => 
    Console.WriteLine($"{p.Operation}: {p.Percentage:P1} - {p.StatusMessage}"));
var mesh = await asyncMesher.MeshWithProgressAsync(structure, options, progress);

// 🚀 Batch processing with 2.2x parallel speedup
var structures = CreateManyStructures(); // 16+ for best performance
var meshes = await asyncMesher.MeshBatchAsync(structures, options, maxDegreeOfParallelism: 4);

// 🚀 Real-time performance monitoring (639ns overhead)
var stats = await asyncMesher.GetLivePerformanceStatsAsync();
Console.WriteLine($"Operations: {stats.MeshingOperations}, Pool efficiency: {stats.PoolHitRate:P1}");

// 🚀 Complexity estimation for resource planning
var estimate = await asyncMesher.EstimateComplexityAsync(structure, options);
Console.WriteLine($"Estimated time: {estimate.EstimatedComputationTime.TotalMilliseconds}ms");
Console.WriteLine($"Estimated memory: {estimate.EstimatedPeakMemoryBytes / 1024 / 1024:F1}MB");

// 🚀 Cancellation support
using var cts = new CancellationTokenSource();
var mesh = await asyncMesher.MeshAsync(structure, options, cts.Token);
```

## 🎯 **When to Use Each Approach**

| Use Case | Recommendation | Why |
|----------|---------------|-----|
| **Desktop apps** | ✅ Async always | Often faster + non-blocking UI |
| **Web applications** | ✅ Async always | Essential for scalability |
| **Single structures** | ✅ Async preferred | 311μs vs 1433μs (78% faster) |
| **Batch processing** | ✅ Async required | 2.2x speedup with parallelism |
| **Legacy integration** | ✅ Sync works | 100% backward compatible |

## 🏗️ **Advanced Features**

### **Smart Performance Presets**
```csharp
// ⚡ Ultra-fast: optimized for real-time applications
var ultraFast = MesherOptions.CreateBuilder().WithFastPreset().Build();

// 🎯 High-quality: optimized for CAD precision
var highQuality = MesherOptions.CreateBuilder().WithHighQualityPreset().Build();

// 🔧 Custom: fine-tuned for your specific needs
var custom = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithMinCapQuadQuality(0.8)
    .WithRejectedCapTriangles(true)
    .Build();
```

### **Complex Geometry Support**
```csharp
// L-shaped structure with holes
var lShape = Polygon2D.FromPoints(new[]{
    new Vec2(0,0), new Vec2(15,0), new Vec2(15,8),
    new Vec2(8,8), new Vec2(8,15), new Vec2(0,15)
});

var hole = Polygon2D.FromPoints(new[]{ 
    new Vec2(3,3), new Vec2(6,3), new Vec2(6,6), new Vec2(3,6)
});

var structure = new PrismStructureDefinition(lShape, -2, 8)
    .AddHole(hole)
    .AddConstraintSegment(new Segment2D(new Vec2(0,8), new Vec2(15,8)), 0);

// Auxiliary geometry for detailed modeling
structure.Geometry
    .AddPoint(new Vec3(7.5, 4, 3))
    .AddSegment(new Segment3D(new Vec3(0,4,3), new Vec3(15,4,3)));
```

### **Multi-Format Export**
```csharp
var indexed = IndexedMesh.FromMesh(mesh);

// Professional CAD formats
ObjExporter.Write(indexed, "model.obj");        // Quads + triangles
GltfExporter.Write(indexed, "model.gltf");      // Triangulated (WebGL ready)
SvgExporter.Write(indexed, "model.svg");        // 2D top view

// Custom precision
var preciseMesh = IndexedMesh.FromMesh(mesh, epsilon: 1e-12);
```

### **Quality Control & Validation**
```csharp
var options = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.8)          // High quality threshold
    .WithRejectedCapTriangles(true)      // Triangle fallback
    .Build();

var mesh = await asyncMesher.MeshAsync(structure, options);

// Validate mesh integrity
var adjacency = indexed.BuildAdjacency();
if (adjacency.NonManifoldEdges.Count > 0)
{
    Console.WriteLine($"⚠️ {adjacency.NonManifoldEdges.Count} non-manifold edges detected");
}

Console.WriteLine($"✅ Generated {mesh.QuadCount} quads, {mesh.TriangleCount} triangles");
```

## 📊 **Performance Benchmarks**

Run the built-in benchmark suite to validate performance on your hardware:

```bash
# Basic performance validation
dotnet run --project samples/FastGeoMesh.Sample --configuration Release -- --benchmarks

# Async vs sync comparison
dotnet run --project samples/FastGeoMesh.Sample --configuration Release -- --async

# Performance optimization demos
dotnet run --project samples/FastGeoMesh.Sample --configuration Release -- --performance
```

### **Expected Results (Reference Hardware)**
```
🔬 Sync vs Async Performance:
  Trivial:  311μs async vs 1433μs sync (+78% improvement) ✅
  Simple:   202μs async vs 348μs sync (+42% improvement) ✅
  Batch 32: 3.3ms parallel vs 7.4ms sequential (2.2x speedup) ✅

🔬 Monitoring Overhead:
  Stats retrieval: 639ns ✅ Negligible
  Complexity estimation: 0.8μs ✅ Minimal
```

## 🚀 **Migration from v1.3.2 to v1.4.0**

**Zero breaking changes!** Your existing code works unchanged.

### **Instant Performance Gains**
Simply add async to get immediate benefits:

```csharp
// Before (v1.3.2)
var mesh = mesher.Mesh(structure, options);

// After (v1.4.0) - often 40-78% faster!
var mesh = await mesher.MeshAsync(structure, options);
```

### **Optional Advanced Features**
Gradually adopt new capabilities as needed:
- Progress reporting for better UX
- Batch processing for multiple structures
- Performance monitoring for optimization
- Complexity estimation for resource planning

📖 **[Complete Migration Guide](MIGRATION-v1.4.0.md)**

## 🎯 **Use Cases & Examples**

### **Real-time Applications**
- ✅ **CAD modeling**: Interactive geometry editing
- ✅ **Game development**: Procedural level generation  
- ✅ **AR/VR**: Dynamic mesh generation
- ✅ **Scientific visualization**: Real-time data meshing

### **Batch Processing**
- ✅ **GIS processing**: Large-scale terrain meshing
- ✅ **Manufacturing**: Batch part processing
- ✅ **Architecture**: Building information modeling
- ✅ **Simulation**: Multi-structure analysis

### **Web Applications**
- ✅ **Cloud APIs**: Scalable mesh generation services
- ✅ **SaaS platforms**: Multi-tenant processing
- ✅ **Progressive web apps**: Client-side meshing
- ✅ **Microservices**: Containerized mesh processing

## 📚 **Documentation & Resources**

- 📖 **[Migration Guide v1.4.0](MIGRATION-v1.4.0.md)** - Zero-friction upgrade
- 📋 **[Complete Changelog](CHANGELOG.md)** - All version history
- 🗺️ **[Development Roadmap](ROADMAP.md)** - Future plans
- ⚡ **Performance Guide** - Optimization tips
- 🔧 **API Reference** - Complete documentation
- 🎯 **[Examples](samples/)** - Working samples and demos

## 🤝 **Contributing & Support**

FastGeoMesh is actively developed and maintained. We welcome:

- 🐛 **Bug reports** and feature requests
- 📝 **Documentation improvements**
- ⚡ **Performance optimizations**
- 🧪 **Test cases and benchmarks**
- 💡 **Usage examples and tutorials**

## 🏆 **Why FastGeoMesh v1.4.0?**

| Feature | FastGeoMesh v1.4.0 | Alternatives |
|---------|-------------------|--------------|
| **Performance** | **311μs** (trivial) | ~5-50ms typical |
| **Async/Parallel** | ✅ **Native ValueTask** | ❌ or Task overhead |
| **Progress Reporting** | ✅ **Detailed tracking** | ❌ or basic |
| **Batch Processing** | ✅ **2.2x speedup** | ❌ sequential only |
| **Monitoring** | ✅ **639ns overhead** | ❌ or expensive |
| **Quality Control** | ✅ **Quad scoring** | ❌ or triangles only |
| **Export Formats** | ✅ **OBJ/glTF/SVG** | ❌ or limited |
| **Backward Compat** | ✅ **100% compatible** | ❌ breaking changes |
| **.NET 8 Optimized** | ✅ **Vectorization** | ❌ legacy frameworks |

## 📄 **License**

MIT License - see [LICENSE](LICENSE) for details.

---

**FastGeoMesh v1.4.0-rc1** - Where sub-millisecond performance meets modern async patterns! 🚀

*Built with ❤️ for the .NET community*
