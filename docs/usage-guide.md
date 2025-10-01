# FastGeoMesh Usage Guide

This comprehensive guide covers all aspects of using FastGeoMesh for generating high-performance quad-dominant meshes from 2.5D prismatic structures.

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Basic Usage](#basic-usage)
3. [Advanced Features](#advanced-features)
4. [Performance Optimization](#performance-optimization)
5. [Export Formats](#export-formats)
6. [Best Practices](#best-practices)
7. [Common Patterns](#common-patterns)
8. [Troubleshooting](#troubleshooting)

## Core Concepts

### Prismatic Structures

FastGeoMesh specializes in **2.5D prismatic structures** - geometries defined by:
- A 2D footprint polygon (XY plane)
- Vertical extent (Z range)
- Optional internal features (holes, constraints, slabs)

### Mesh Types

The library generates **quad-dominant meshes** with optional triangle fallbacks:
- **Side Quads**: Vertical faces of the prism
- **Cap Quads**: Top/bottom faces (when quality threshold met)
- **Cap Triangles**: Fallback for low-quality quads (optional)

### Coordinate System

- **XY Plane**: Horizontal footprint
- **Z Axis**: Vertical elevation
- **Winding**: Counter-clockwise (CCW) for outward-facing normals

## Basic Usage

### Simple Rectangular Prism

The most basic use case - a rectangular prism:

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

// Define 20x5 rectangle from Z=-10 to Z=10
var rectangle = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(20, 0), 
    new Vec2(20, 5), new Vec2(0, 5)
});

var structure = new PrismStructureDefinition(rectangle, -10, 10);

// Use fast preset for optimal performance
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh);

Console.WriteLine($"Generated: {indexed.QuadCount} quads, {indexed.TriangleCount} triangles");
```

### Complex Polygon

For non-rectangular footprints:

```csharp
// L-shaped footprint
var lShape = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 4),
    new Vec2(6, 4), new Vec2(6, 8), new Vec2(0, 8)
});

var structure = new PrismStructureDefinition(lShape, 0, 5);

var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(1.0)
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);
```

## Advanced Features

### Constraint Segments

Add horizontal constraint segments at specific Z levels to create intermediate structural elements:

```csharp
var structure = new PrismStructureDefinition(footprint, 0, 10);

// Add beam at Z = 5
structure = structure.AddConstraintSegment(
    new Segment2D(new Vec2(0, 2), new Vec2(20, 2)), 5.0);

// Add multiple constraints
structure = structure
    .AddConstraintSegment(new Segment2D(new Vec2(5, 0), new Vec2(5, 5)), 3.0)
    .AddConstraintSegment(new Segment2D(new Vec2(15, 0), new Vec2(15, 5)), 7.0);
```

### Holes in Footprint

Create structures with internal holes:

```csharp
var outer = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(10, 0), 
    new Vec2(10, 6), new Vec2(0, 6)
});

var hole1 = Polygon2D.FromPoints(new[] {
    new Vec2(2, 2), new Vec2(4, 2), 
    new Vec2(4, 4), new Vec2(2, 4)
});

var hole2 = Polygon2D.FromPoints(new[] {
    new Vec2(6, 1), new Vec2(8, 1), 
    new Vec2(8, 3), new Vec2(6, 3)
});

var structure = new PrismStructureDefinition(outer, 0, 5)
    .AddHole(hole1)
    .AddHole(hole2);

// Configure hole refinement
var options = MesherOptions.CreateBuilder()
    .WithHoleRefinement(targetLength: 0.5, band: 1.0)
    .Build();
```

### Internal Surfaces (Slabs)

Add horizontal internal surfaces at specific elevations:

```csharp
// Main structure
var structure = new PrismStructureDefinition(footprint, -5, 10);

// Add intermediate slab at Z = 2 with hole
var slabOutline = Polygon2D.FromPoints(new[] {
    new Vec2(1, 1), new Vec2(19, 1), 
    new Vec2(19, 4), new Vec2(1, 4)
});

var slabHole = Polygon2D.FromPoints(new[] {
    new Vec2(8, 2), new Vec2(12, 2), 
    new Vec2(12, 3), new Vec2(8, 3)
});

structure = structure.AddInternalSurface(slabOutline, 2.0, slabHole);
```

### Auxiliary Geometry

Add points and segments that will be preserved in the output mesh:

```csharp
structure.Geometry
    .AddPoint(new Vec3(10, 2.5, 5))     // Isolated point
    .AddSegment(new Segment3D(          // 3D segment
        new Vec3(0, 2.5, 0), 
        new Vec3(20, 2.5, 10)))
    .AddPoints(new[] {                  // Multiple points
        new Vec3(5, 1, 3),
        new Vec3(15, 4, 7)
    });
```

## Performance Optimization

### Performance Presets

Choose the appropriate preset for your use case:

```csharp
// Fast Preset (~305μs for simple geometry)
// - Larger edge lengths for fewer elements
// - Lower quality thresholds
// - Minimal triangle fallbacks
var fast = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

// High-Quality Preset (~1.3ms for simple geometry)
// - Smaller edge lengths for denser mesh
// - Higher quality thresholds
// - More triangle fallbacks for quality
var quality = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .Build();
```

### Custom Performance Tuning

Fine-tune performance for specific requirements:

```csharp
var options = MesherOptions.CreateBuilder()
    // Primary edge lengths (smaller = more detail, slower)
    .WithTargetEdgeLengthXY(2.0)        // Fast: 2.0, Quality: 0.5
    .WithTargetEdgeLengthZ(2.0)         // Fast: 2.0, Quality: 0.5
    
    // Refinement near features
    .WithHoleRefinement(1.0, 2.0)       // Optional: finer mesh near holes
    .WithSegmentRefinement(1.0, 1.5)    // Optional: finer mesh near segments
    
    // Quality control
    .WithMinCapQuadQuality(0.3)         // Fast: 0.3, Quality: 0.7
    .WithRejectedCapTriangles(false)    // Fast: false, Quality: true
    
    // Caps generation
    .WithCaps(bottom: true, top: true)
    
    .Build();
```

### Memory Optimization

For memory-constrained environments:

```csharp
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(3.0)        // Larger elements = less memory
    .WithRejectedCapTriangles(false)    // Skip triangle generation
    .Build();

// Use larger epsilon for coordinate deduplication
var indexed = IndexedMesh.FromMesh(mesh, epsilon: 1e-6);
```

### Asynchronous Processing

For non-blocking operations:

```csharp
var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

try
{
    var mesh = await new PrismMesher()
        .MeshAsync(structure, options, cancellationToken);
    
    // Process mesh...
}
catch (OperationCanceledException)
{
    Console.WriteLine("Meshing operation timed out");
}
```

## Export Formats

### OBJ Format (Quads + Triangles)

Preserves quad structure with triangle fallbacks:

```csharp
using FastGeoMesh.Meshing.Exporters;

var indexed = IndexedMesh.FromMesh(mesh);
ObjExporter.Write(indexed, "output.obj");

// The OBJ file will contain:
// - 'v' lines for vertices
// - 'f' lines with 5 indices for quads
// - 'f' lines with 4 indices for triangles
```

### glTF Format (Triangulated)

Standard format for 3D applications:

```csharp
GltfExporter.Write(indexed, "output.gltf");

// Creates self-contained glTF with embedded binary data
// All quads are automatically triangulated
// Suitable for web viewers, game engines, etc.
```

### SVG Format (Top View)

2D representation showing mesh structure:

```csharp
SvgExporter.Write(indexed, "output.svg");

// Creates SVG showing:
// - Top-view projection of all edges
// - Useful for debugging mesh topology
// - Can be viewed in web browsers
```

## Best Practices

### Polygon Definition

1. **Ensure CCW winding** for outer boundaries
2. **Use CW winding** for holes (automatically corrected)
3. **Avoid self-intersections** and degenerate edges
4. **Validate polygons** before processing:

```csharp
if (!Polygon2D.Validate(vertices, out string? error))
{
    throw new ArgumentException($"Invalid polygon: {error}");
}
```

### Edge Length Selection

Choose edge lengths based on your requirements:

```csharp
// For visualization (fast)
var visual = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(5.0)    // Coarse mesh
    .Build();

// For analysis (balanced)
var analysis = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)    // Medium mesh
    .Build();

// For high-precision (slow)
var precision = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(0.1)    // Fine mesh
    .Build();
```

### Quality Control

Balance quad quality vs triangle count:

```csharp
// Prefer quads (accept lower quality)
var quadDominant = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.2)
    .WithRejectedCapTriangles(false)
    .Build();

// Prefer quality (more triangles)
var highQuality = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.8)
    .WithRejectedCapTriangles(true)
    .Build();
```

## Common Patterns

### Building/Architecture Modeling

```csharp
// Building footprint with courtyard
var building = Polygon2D.FromPoints(/* outer boundary */);
var courtyard = Polygon2D.FromPoints(/* inner courtyard */);

var structure = new PrismStructureDefinition(building, 0, 30)  // 30m tall
    .AddHole(courtyard);

// Add floor slabs every 3m
for (double z = 3; z <= 27; z += 3)
{
    structure = structure.AddInternalSurface(building, z, courtyard);
}

var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(2.0)
    .WithTargetEdgeLengthZ(3.0)     // Align with floor heights
    .Build();
```

### Excavation/Mining

```csharp
// Excavation pit with benches
var pit = Polygon2D.FromPoints(/* pit outline */);
var structure = new PrismStructureDefinition(pit, -20, 0);  // 20m deep

// Add bench levels every 5m
for (double z = -15; z <= -5; z += 5)
{
    var benchWidth = 2.0;  // 2m wide benches
    var benchPoly = CreateOffsetPolygon(pit, -benchWidth);
    
    structure = structure.AddConstraintSegment(
        new Segment2D(benchPoly.Vertices[0], benchPoly.Vertices[1]), z);
}
```

### Terrain Modeling

```csharp
// Contour-based terrain
var boundary = Polygon2D.FromPoints(/* terrain boundary */);
var structure = new PrismStructureDefinition(boundary, 0, maxElevation);

// Add contour lines as constraints
foreach (var contour in contourLines)
{
    structure = structure.AddConstraintSegment(contour.Segment, contour.Elevation);
}

var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(10.0)   // 10m resolution
    .WithSegmentRefinement(5.0, 20.0)  // Refine near contours
    .Build();
```

## Troubleshooting

### Common Issues

1. **Empty Mesh Generated**
   - Check polygon winding (should be CCW)
   - Verify Z range (topElevation > baseElevation)
   - Ensure polygon is valid and non-degenerate

2. **Performance Issues**
   - Use FastPreset for better performance
   - Increase target edge lengths
   - Disable triangle generation if not needed
   - Check for excessive refinement settings

3. **Quality Issues**
   - Increase MinCapQuadQuality threshold
   - Enable OutputRejectedCapTriangles
   - Reduce target edge lengths for finer control

4. **Memory Usage**
   - Use larger edge lengths to reduce element count
   - Increase epsilon for coordinate deduplication
   - Disable unnecessary features (caps, triangles)

### Debugging Tools

```csharp
// Check mesh statistics
Console.WriteLine($"Vertices: {indexed.VertexCount}");
Console.WriteLine($"Edges: {indexed.EdgeCount}");
Console.WriteLine($"Quads: {indexed.QuadCount}");
Console.WriteLine($"Triangles: {indexed.TriangleCount}");

// Analyze mesh quality
var adjacency = indexed.BuildAdjacency();
Console.WriteLine($"Boundary edges: {adjacency.BoundaryEdges.Count}");
Console.WriteLine($"Non-manifold edges: {adjacency.NonManifoldEdges.Count}");

// Export for visual inspection
SvgExporter.Write(indexed, "debug.svg");  // View mesh topology
```

### Performance Monitoring

```csharp
// Enable performance monitoring
using FastGeoMesh.Utils;

// Monitor operations
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Meshing operations: {stats.MeshingOperations}");
Console.WriteLine($"Average quads per operation: {stats.AverageQuadsPerOperation:F1}");
```

This completes the comprehensive usage guide. For specific API details, see the [API Reference](api-reference.md).
