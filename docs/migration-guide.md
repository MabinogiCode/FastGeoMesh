# FastGeoMesh Migration Guide

This guide helps you migrate between different versions of FastGeoMesh and provides upgrade paths for optimal performance and compatibility.

## Table of Contents

1. [Version Overview](#version-overview)
2. [Migration from 1.0.x to 1.1.x](#migration-from-10x-to-11x)
3. [Performance Optimizations](#performance-optimizations)
4. [Breaking Changes](#breaking-changes)
5. [New Features](#new-features)
6. [Best Practices Updates](#best-practices-updates)

## Version Overview

### Version 1.1.0 (Current)
- **Release Date**: 2024
- **Target Framework**: .NET 8.0
- **Key Features**: 
  - Added cap triangle emission (`OutputRejectedCapTriangles`)
  - Triangle support in mesh/indexed mesh + OBJ exporter
  - Performance optimizations with object pooling
  - Enhanced quality control for cap meshes
  - Improved documentation

### Version 1.0.x (Legacy)
- **Target Framework**: .NET 8.0
- **Features**: Basic prism meshing with quad-only output

## Migration from 1.0.x to 1.1.x

### Package Update

Update your package reference:

```xml
<!-- Before: 1.0.x -->
<PackageReference Include="FastGeoMesh" Version="1.0.5" />

<!-- After: 1.1.x -->
<PackageReference Include="FastGeoMesh" Version="1.1.0" />
```

### Code Changes Required

#### 1. MesherOptions API Changes

**Before (1.0.x):**
```csharp
var options = new MesherOptions
{
    TargetEdgeLengthXY = 1.0,
    TargetEdgeLengthZ = 0.5,
    GenerateBottomCap = true,
    GenerateTopCap = true
};
```

**After (1.1.x):**
```csharp
// Option 1: Direct construction (still supported)
var options = new MesherOptions
{
    TargetEdgeLengthXY = 1.0,
    TargetEdgeLengthZ = 0.5,
    GenerateBottomCap = true,
    GenerateTopCap = true,
    OutputRejectedCapTriangles = false  // New property
};

// Option 2: Builder pattern (recommended)
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(0.5)
    .WithCaps(bottom: true, top: true)
    .Build();
```

#### 2. Triangle Support

**New in 1.1.x**: Triangle output for low-quality cap quads

```csharp
// Enable triangle fallbacks for better quality
var options = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.7)          // Higher quality threshold
    .WithRejectedCapTriangles(true)      // Output triangles for rejected quads
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);

// Check for triangles in output
Console.WriteLine($"Quads: {mesh.Quads.Count}, Triangles: {mesh.Triangles.Count}");
```

#### 3. Performance Presets

**New in 1.1.x**: Predefined performance configurations

```csharp
// Fast preset for real-time applications
var fastOptions = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

// High-quality preset for CAD applications
var qualityOptions = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .Build();
```

#### 4. Enhanced Export Support

**Updated in 1.1.x**: OBJ exporter now supports triangles

```csharp
// OBJ export now includes both quads and triangles
var indexed = IndexedMesh.FromMesh(mesh);
ObjExporter.Write(indexed, "output.obj");

// The OBJ file will contain:
// - Quads as "f v1 v2 v3 v4 v5" (5 vertices, last repeats first)
// - Triangles as "f v1 v2 v3 v4" (4 vertices, last repeats first)
```

### Compatibility Layer

For minimal migration effort, most 1.0.x code continues to work:

```csharp
// This 1.0.x code still works in 1.1.x
var structure = new PrismStructureDefinition(polygon, -10, 10);
var options = new MesherOptions { TargetEdgeLengthXY = 1.0 };
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh);
```

## Performance Optimizations

### Object Pooling (1.1.x)

The library now uses object pooling for better performance:

```csharp
// No code changes required - pooling is automatic
var mesher = new PrismMesher();

// Multiple operations reuse pooled objects
for (int i = 0; i < 1000; i++)
{
    var mesh = mesher.Mesh(structures[i], options);  // Benefits from pooling
}
```

### Optimized Data Structures

**1.1.x improvements:**
- Faster coordinate deduplication
- Optimized tessellation algorithms
- Better memory allocation patterns

### Performance Monitoring

**New in 1.1.x**: Built-in performance monitoring

```csharp
using FastGeoMesh.Utils;

// Monitor performance
PerformanceMonitor.Counters.Reset();

// Perform operations...
var mesh = mesher.Mesh(structure, options);

// Get statistics
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Pool hit ratio: {stats.PoolHitRatio:P1}");
```

## Breaking Changes

### Minimal Breaking Changes

FastGeoMesh 1.1.x maintains strong backward compatibility. The only potential breaking changes are:

#### 1. Default Triangle Output
- **Change**: `OutputRejectedCapTriangles` defaults to `false`
- **Impact**: Same behavior as 1.0.x by default
- **Action**: No change required unless you want triangle output

#### 2. Quality Threshold Behavior
- **Change**: Improved quad quality calculation
- **Impact**: Slightly different mesh output for edge cases
- **Action**: Review quality thresholds if precise mesh reproduction is required

### API Additions (Non-Breaking)

All new APIs are additive:

```csharp
// New properties (with backward-compatible defaults)
public bool OutputRejectedCapTriangles { get; set; } = false;
public double MinCapQuadQuality { get; set; } = 0.5;

// New builder methods
public MesherOptionsBuilder WithFastPreset();
public MesherOptionsBuilder WithHighQualityPreset();
public MesherOptionsBuilder WithRejectedCapTriangles(bool output = true);
```

## New Features

### 1. Triangle Fallback System

Generate triangles when quad quality is insufficient:

```csharp
var options = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.8)          // High quality requirement
    .WithRejectedCapTriangles(true)      // Generate triangles for failed quads
    .Build();

var mesh = mesher.Mesh(structure, options);

// Access both quads and triangles
foreach (var quad in mesh.Quads)
{
    // Process high-quality quads
}

foreach (var triangle in mesh.Triangles)
{
    // Process triangular fallbacks
}
```

### 2. Performance Presets

Quickly configure for common scenarios:

```csharp
// Real-time applications
var realTime = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(2.0)  // Override specific settings
    .Build();

// High-precision CAD
var cad = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .WithTargetEdgeLengthXY(0.1)  // Ultra-fine mesh
    .Build();
```

### 3. Enhanced Quality Control

More precise control over mesh quality:

```csharp
var options = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.7)          // Require high quad quality
    .WithHoleRefinement(0.5, 1.0)        // Refine near holes
    .WithSegmentRefinement(0.5, 0.8)     // Refine near segments
    .Build();
```

### 4. Improved Export Formats

Better support for mixed quad/triangle meshes:

```csharp
var indexed = IndexedMesh.FromMesh(mesh);

// OBJ with mixed geometry
ObjExporter.Write(indexed, "mixed.obj");

// glTF (automatically triangulated)
GltfExporter.Write(indexed, "triangulated.gltf");

// SVG topology view
SvgExporter.Write(indexed, "topology.svg");
```

## Best Practices Updates

### 1. Use Builder Pattern

**Recommended for 1.1.x:**
```csharp
// Prefer builder pattern for clarity and future compatibility
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithCaps(bottom: true, top: true)
    .WithRejectedCapTriangles(true)
    .Build();
```

### 2. Choose Appropriate Presets

Start with presets and customize:

```csharp
// Good: Start with preset, customize as needed
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(1.5)  // Override just what you need
    .Build();
```

### 3. Consider Triangle Output

For high-quality applications:

```csharp
var options = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()  // Includes OutputRejectedCapTriangles = true
    .Build();
```

For performance-critical applications:

```csharp
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()  // Includes OutputRejectedCapTriangles = false
    .Build();
```

### 4. Monitor Performance

Use built-in monitoring for optimization:

```csharp
// Profile your specific use case
PerformanceMonitor.Counters.Reset();

// Your meshing operations...

var stats = PerformanceMonitor.Counters.GetStatistics();
// Analyze and optimize based on results
```

## Upgrade Checklist

- [ ] Update package reference to 1.1.0
- [ ] Review triangle output requirements
- [ ] Consider using performance presets
- [ ] Test with existing geometries
- [ ] Update export pipeline if using triangles
- [ ] Add performance monitoring if desired
- [ ] Review documentation for new features

## Migration Example

Complete migration example from 1.0.x to 1.1.x:

**Before (1.0.x):**
```csharp
var structure = new PrismStructureDefinition(polygon, 0, 10);
var options = new MesherOptions
{
    TargetEdgeLengthXY = 1.0,
    TargetEdgeLengthZ = 1.0,
    GenerateTopCap = true,
    GenerateBottomCap = false
};

var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh);
ObjExporter.Write(indexed, "output.obj");
```

**After (1.1.x):**
```csharp
var structure = new PrismStructureDefinition(polygon, 0, 10);

// Option 1: Minimal change (compatible)
var options = new MesherOptions
{
    TargetEdgeLengthXY = 1.0,
    TargetEdgeLengthZ = 1.0,
    GenerateTopCap = true,
    GenerateBottomCap = false,
    OutputRejectedCapTriangles = false  // Explicit for clarity
};

// Option 2: Modern approach (recommended)
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(1.0)
    .WithCaps(bottom: false, top: true)
    .WithRejectedCapTriangles(false)  // Or true for quality
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh);

// Enhanced export (supports triangles)
ObjExporter.Write(indexed, "output.obj");

// Optional: Monitor performance
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Generated {indexed.QuadCount} quads, {indexed.TriangleCount} triangles");
```

This migration guide ensures a smooth transition while taking advantage of the new features and performance improvements in FastGeoMesh 1.1.x.
