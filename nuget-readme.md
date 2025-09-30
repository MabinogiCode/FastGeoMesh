# FastGeoMesh

**Fast quad meshing for prismatic volumes from 2D footprints and Z elevations.**

[![CI](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/MabinogiCode/FastGeoMesh/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastGeoMesh)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

## ⚡ Performance

**Sub-millisecond meshing** with .NET 8 optimizations:
- **Simple Prism**: ~305 μs, 87 KB
- **Complex Geometry**: ~340 μs, 87 KB  
- **With Holes**: ~907 μs, 1.3 MB
- **Geometry Ops**: < 10 μs, zero allocations

*Benchmarked on .NET 8.0.20, X64 RyuJIT AVX2*

## Features

- **🏗️ Prism mesher** (side faces + caps)
- **📐 Rectangle fast-path** + generic tessellation
- **🎯 Quad quality scoring** & threshold (MinCapQuadQuality)
- **📑 Optional fallback** explicit cap triangles (OutputRejectedCapTriangles)
- **⚙️ Constraint Z levels** & geometry integration
- **📤 Exporters**: OBJ (quads+triangles), glTF (triangulated), SVG (top view)

## Quick start

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// Define geometry (~305μs meshing time)
var poly = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) 
});
var structure = new PrismStructureDefinition(poly, -10, 10);
structure.AddConstraintSegment(new Segment2D(new Vec2(0,0), new Vec2(20,0)), 2.5);

// Fast preset for optimal performance
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithRejectedCapTriangles(true)
    .Build();

// Generate and export
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
SvgExporter.Write(indexed, "mesh.svg");
```

## Performance Presets

```csharp
// Fast: ~305μs, 87KB - Real-time applications
var fast = MesherOptions.CreateBuilder().WithFastPreset().Build();

// High-Quality: ~1.3ms, 17MB - CAD precision  
var quality = MesherOptions.CreateBuilder().WithHighQualityPreset().Build();
```

**License**: MIT | **Docs & source**: https://github.com/MabinogiCode/FastGeoMesh
