# FastGeoMesh v2.0

**🇬🇧 English** | [🇫🇷 Français](#français)

---

## English

[![CI](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/MabinogiCode/FastGeoMesh/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastGeoMesh)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/FastGeoMesh.svg)](https://www.nuget.org/packages/FastGeoMesh/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Fast, safe, quad-dominant meshing for prismatic volumes from 2D footprints and Z elevations.**

FastGeoMesh v2.0 is a high-performance .NET 8 library for generating quad-dominant meshes from 2.5D prismatic structures. Built with **Clean Architecture** principles, it offers excellent separation of concerns, testability, and maintainability.

## ⚡ Performance

**Sub-millisecond meshing** with .NET 8 optimizations and async improvements:
- **Trivial Structures (Async)**: ~311 μs (78% faster than sync!)
- **Simple Structures (Async)**: ~202 μs (42% faster than sync!)
- **Complex Geometry**: ~340 μs, 87 KB
- **Batch Processing (32 items)**: 3.3ms parallel vs 7.4ms sequential (2.2x speedup)
- **Performance Monitoring**: 639ns overhead (negligible)
- **Geometry Operations**: < 10 μs, zero allocations

*Benchmarked on .NET 8, X64 RyuJIT AVX2.*

## 🏗️ Clean Architecture

FastGeoMesh v2.0 is built with Clean Architecture principles:

- **🔵 Domain Layer** (`FastGeoMesh.Domain`): Core entities, value objects, and domain logic
- **🟡 Application Layer** (`FastGeoMesh.Application`): Use cases and meshing algorithms 
- **🟢 Infrastructure Layer** (`FastGeoMesh.Infrastructure`): External concerns (file I/O, performance optimization)

## 🚀 Features

- **🏗️ Prism Mesher**: Generate side faces and caps from 2D footprints.
- **✨ Robust Error Handling**: Uses a `Result` pattern to eliminate exceptions in the standard workflow.
- **⚡ Async/Parallel Processing**: Complete async interface with 2.2x parallel speedup.
- **📊 Real-time Monitoring**: Performance statistics and complexity estimation.
- **🎯 Progress Reporting**: Detailed operation tracking with ETA.
- **📐 Smart Fast-Paths**: Rectangle optimization + generic tessellation fallback.
- **🎯 Quality Control**: Quad quality scoring & configurable thresholds.
- **📑 Triangle Fallback**: Optional explicit cap triangles for low-quality quads.
- **⚙️ Constraint System**: Z-level segments & integrated auxiliary geometry.
- **📤 Multi-Format Export**: OBJ (quads+triangles), glTF (triangulated), SVG (top view), Legacy format.
- **🔧 Performance Presets**: Fast vs High-Quality configurations.
- **🧵 Thread-Safe**: Immutable structures and stateless meshers.

## 🚀 Quick Start

```csharp
using FastGeoMesh.Domain;
using FastGeoMesh.Application;
using FastGeoMesh.Infrastructure.Exporters;

// 1. Define geometry
var polygon = Polygon2D.FromPoints(new[]
{
    new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 5), new Vec2(0, 5)
});
var structure = new PrismStructureDefinition(polygon, -10, 10);

// 2. Configure options safely
var optionsResult = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithRejectedCapTriangles(true)
    .Build();

if (optionsResult.IsFailure)
{
    Console.WriteLine($"Configuration error: {optionsResult.Error.Description}");
    return;
}
var options = optionsResult.Value;

// 3. Generate the mesh safely
var mesher = new PrismMesher();
var meshResult = mesher.Mesh(structure, options);

if (meshResult.IsFailure)
{
    Console.WriteLine($"Meshing failed: {meshResult.Error.Description}");
    return;
}
var mesh = meshResult.Value;

// 4. (Optional) Use the async API for better performance
var asyncMesher = (IAsyncMesher)mesher;
var asyncMeshResult = await asyncMesher.MeshAsync(structure, options);
if (asyncMeshResult.IsSuccess)
{
    var asyncMesh = asyncMeshResult.Value;
}

// 5. Convert to indexed mesh and export to your preferred format
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

// Choose your export format:
ObjExporter.Write(indexed, "mesh.obj");           // Wavefront OBJ
GltfExporter.Write(indexed, "mesh.gltf");         // glTF 2.0
SvgExporter.Write(indexed, "mesh.svg");           // SVG top view
LegacyExporter.Write(indexed, "mesh.txt");        // Legacy format
LegacyExporter.WriteWithLegacyName(indexed, "./output/"); // Creates 0_maill.txt

// Or use the new flexible TXT exporter with builder pattern:
indexed.ExportTxt()
    .WithPoints("p", CountPlacement.Top, indexBased: true)
    .WithEdges("e", CountPlacement.None, indexBased: false)
    .WithQuads("q", CountPlacement.Bottom, indexBased: true)
    .ToFile("custom_mesh.txt");

// Pre-configured formats:
TxtExporter.WriteObjLike(indexed, "objlike.txt");   // OBJ-style format
```

## 💥 Breaking Changes in v2.0

FastGeoMesh v2.0 introduces **Clean Architecture** which requires some namespace changes:

**OLD (v1.x):**
```csharp
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Geometry;
```

**NEW (v2.0):**
```csharp
using FastGeoMesh.Domain;           // Core types
using FastGeoMesh.Application;      // Meshing logic
using FastGeoMesh.Infrastructure;   // External services
```

**API Changes:**
- `MesherOptions.CreateBuilder().Build()` returns a `Result<MesherOptions>`.
- `PrismMesher.Mesh()` and its async variants return a `Result<ImmutableMesh>`.
- Direct access to Clean Architecture layers (no more wrapper classes).

## 🏗️ Advanced Features

### Error Handling with Result Pattern

```csharp
var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(-1.0) // Invalid value
    .Build();

if (optionsResult.IsFailure)
{
    Console.WriteLine($"Configuration error: {optionsResult.Error.Description}");
    return;
}
```

### Complex Structures with Holes

```csharp
var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(10,0), new Vec2(10,6), new Vec2(0,6) });
var hole = Polygon2D.FromPoints(new[] { new Vec2(2,2), new Vec2(4,2), new Vec2(4,4), new Vec2(2,4) });
var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);

var optionsResult = MesherOptions.CreateBuilder()
    .WithHoleRefinement(0.75, 1.0)
    .Build();

if (optionsResult.IsSuccess)
{
    var meshResult = new PrismMesher().Mesh(structure, optionsResult.Value);
    // Handle result...
}
```

### L-Shaped Structures

```csharp
// Create L-shaped footprint
var lshape = Polygon2D.FromPoints(new[]
{
    new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 3),
    new Vec2(3, 3), new Vec2(3, 6), new Vec2(0, 6)
});
var structure = new PrismStructureDefinition(lshape, 0, 4);

var optionsResult = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .WithTargetEdgeLengthXY(0.8)
    .Build();

if (optionsResult.IsSuccess)
{
    var meshResult = new PrismMesher().Mesh(structure, optionsResult.Value);
    if (meshResult.IsSuccess)
    {
        var indexed = IndexedMesh.FromMesh(meshResult.Value);
        ObjExporter.Write(indexed, "lshape.obj");
    }
}
```

### T-Shaped Structures

```csharp
// Create T-shaped footprint  
var tshape = Polygon2D.FromPoints(new[]
{
    new Vec2(0, 2), new Vec2(8, 2), new Vec2(8, 4),
    new Vec2(5, 4), new Vec2(5, 6), new Vec2(3, 6),
    new Vec2(3, 4), new Vec2(0, 4)
});
var structure = new PrismStructureDefinition(tshape, -2, 3);

// Add constraint segments for structural analysis
structure = structure.AddConstraintSegment(
    new Segment2D(new Vec2(0, 3), new Vec2(8, 3)), 0.5);

var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(0.6)
    .WithTargetEdgeLengthZ(1.0)
    .WithSegmentRefinement(0.3, 0.5)
    .Build();

if (optionsResult.IsSuccess)
{
    var meshResult = new PrismMesher().Mesh(structure, optionsResult.Value);
    if (meshResult.IsSuccess)
    {
        var indexed = IndexedMesh.FromMesh(meshResult.Value);
        GltfExporter.Write(indexed, "tshape.gltf");
    }
}
```

### Flexible TXT Export

The new TXT exporter provides complete control over output format:

```csharp
// Custom format with builder pattern
indexed.ExportTxt()
    .WithPoints("vertex", CountPlacement.Top, indexBased: true)      // "8\nvertex 1 0.0 0.0 0.0\n..."
    .WithEdges("edge", CountPlacement.None, indexBased: false)       // "edge 0 1\nedge 1 2\n..."
    .WithQuads("quad", CountPlacement.Bottom, indexBased: true)      // "quad 1 0 1 2 3\n...\n4"
    .ToFile("mesh.txt");

// Count placement options:
// - CountPlacement.Top: Count at beginning of section
// - CountPlacement.Bottom: Count at end of section  
// - CountPlacement.None: No count written
```

### Migration notes: Geometry configuration

The project migrated from a mutable static `GeometryConfig` to an immutable `GeometryOptions` record in `FastGeoMesh.Domain`.

- Old API: `FastGeoMesh.Infrastructure.GeometryConfig.DefaultTolerance = 1e-6;`
- New API: create a `GeometryOptions` instance and pass it explicitly to helpers:

```csharp
var opts = new GeometryOptions(DefaultTolerance: 1e-6, ConvexityTolerance: -1e-6, PointInPolygonTolerance: 1e-8);
var inside = GeometryHelper.PointInPolygon(polygonSpan, x, y, options: opts);
```

Rationale:
- Removes global mutable state and improves test determinism.
- Encourages dependency injection when components require non-default geometry tolerances.

Recommendations:
- For long-lived services, inject `GeometryOptions` via constructor.
- For one-off calls, pass `options` directly to `GeometryHelper` methods that accept it.
