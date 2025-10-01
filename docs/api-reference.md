# FastGeoMesh API Reference

Complete API documentation for FastGeoMesh library classes, interfaces, and methods.

## Table of Contents

1. [Core Namespaces](#core-namespaces)
2. [Geometry Types](#geometry-types)
3. [Meshing API](#meshing-api)
4. [Structure Definitions](#structure-definitions)
5. [Export API](#export-api)
6. [Utility Types](#utility-types)
7. [Performance Monitoring](#performance-monitoring)

## Core Namespaces

### FastGeoMesh.Geometry
Fundamental geometric primitives and operations.

### FastGeoMesh.Meshing
Core meshing algorithms and options.

### FastGeoMesh.Structures
High-level structure definitions for prismatic geometries.

### FastGeoMesh.Meshing.Exporters
Export functionality for various file formats.

### FastGeoMesh.Utils
Utility classes for performance and helper functions.

## Geometry Types

### Vec2 Struct

High-performance 2D vector for XY plane operations.

```csharp
public readonly struct Vec2 : IEquatable<Vec2>
{
    // Properties
    public double X { get; }
    public double Y { get; }
    
    // Constructor
    public Vec2(double x, double y)
    
    // Constants
    public static readonly Vec2 Zero;
    public static readonly Vec2 UnitX;
    public static readonly Vec2 UnitY;
    
    // Operators
    public static Vec2 operator +(Vec2 a, Vec2 b);
    public static Vec2 operator -(Vec2 a, Vec2 b);
    public static Vec2 operator *(Vec2 a, double k);
    public static Vec2 operator *(double k, Vec2 a);
    
    // Methods
    public double Dot(in Vec2 b);
    public double Cross(in Vec2 b);
    public double Length();
    public double LengthSquared();
    public Vec2 Normalize();
    
    // Batch operations
    public static double AccumulateDot(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b);
    public static void Add(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b, Span<Vec2> dest);
}
```

**Usage Examples:**
```csharp
// Create vectors
var v1 = new Vec2(3.0, 4.0);
var v2 = Vec2.UnitX;

// Vector operations
var sum = v1 + v2;
var scaled = v1 * 2.0;
var length = v1.Length();  // 5.0
var dot = v1.Dot(v2);      // 3.0

// Normalization
var normalized = v1.Normalize();  // (0.6, 0.8)
```

### Vec3 Struct

High-performance 3D vector for spatial operations.

```csharp
public readonly struct Vec3 : IEquatable<Vec3>
{
    // Properties
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    
    // Constructor
    public Vec3(double x, double y, double z)
    
    // Constants
    public static readonly Vec3 Zero;
    public static readonly Vec3 UnitX;
    public static readonly Vec3 UnitY;
    public static readonly Vec3 UnitZ;
    
    // Operators
    public static Vec3 operator +(Vec3 a, Vec3 b);
    public static Vec3 operator -(Vec3 a, Vec3 b);
    public static Vec3 operator *(Vec3 a, double k);
    
    // Methods
    public double Dot(in Vec3 b);
    public Vec3 Cross(in Vec3 b);
    public double Length();
    public double LengthSquared();
    public Vec3 Normalize();
    
    // Batch operations
    public static double AccumulateDot(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b);
    public static void Add(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest);
    public static void Cross(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest);
}
```

### Polygon2D Class

Represents a simple polygon in the XY plane with CCW vertex ordering.

```csharp
public sealed class Polygon2D
{
    // Properties
    public IReadOnlyList<Vec2> Vertices { get; }
    public int Count { get; }
    
    // Constructor
    public Polygon2D(IEnumerable<Vec2> vertices)
    
    // Factory method
    public static Polygon2D FromPoints(IEnumerable<Vec2> vertices)
    
    // Methods
    public double Perimeter();
    public bool IsRectangleAxisAligned(out Vec2 min, out Vec2 max, double eps = 1e-9);
    
    // Static methods
    public static double SignedArea(IReadOnlyList<Vec2> vertices);
    public static bool Validate(IReadOnlyList<Vec2> vertices, out string? error, double eps = 1e-9);
}
```

**Usage Examples:**
```csharp
// Create rectangle
var rect = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(10, 0), 
    new Vec2(10, 5), new Vec2(0, 5)
});

// Check if axis-aligned rectangle
if (rect.IsRectangleAxisAligned(out Vec2 min, out Vec2 max))
{
    Console.WriteLine($"Rectangle: {min} to {max}");
}

// Validate polygon
if (!Polygon2D.Validate(vertices, out string? error))
{
    throw new ArgumentException($"Invalid polygon: {error}");
}
```

### Segment2D Record

2D line segment defined by two endpoints.

```csharp
public readonly record struct Segment2D(Vec2 A, Vec2 B)
{
    public double Length();
}
```

### Segment3D Record

3D line segment defined by two endpoints.

```csharp
public readonly record struct Segment3D(Vec3 A, Vec3 B)
{
    public double Length();
    public Vec3 Direction();
    public Vec3 Midpoint();
}
```

## Meshing API

### IMesher<T> Interface

Core meshing interface with async support.

```csharp
public interface IMesher<in TStructure> where TStructure : notnull
{
    Mesh Mesh(TStructure input, MesherOptions options);
    ValueTask<Mesh> MeshAsync(TStructure input, MesherOptions options, 
                             CancellationToken cancellationToken = default);
}
```

### PrismMesher Class

Main mesher implementation for prismatic structures.

```csharp
public sealed class PrismMesher : IMesher<PrismStructureDefinition>
{
    // Constructors
    public PrismMesher();
    public PrismMesher(ICapMeshingStrategy capStrategy);
    
    // Methods
    public Mesh Mesh(PrismStructureDefinition structure, MesherOptions options);
    public ValueTask<Mesh> MeshAsync(PrismStructureDefinition structure, 
                                    MesherOptions options, 
                                    CancellationToken cancellationToken = default);
}
```

**Usage Examples:**
```csharp
// Basic usage
var mesher = new PrismMesher();
var mesh = mesher.Mesh(structure, options);

// Async usage
var mesh = await mesher.MeshAsync(structure, options, cancellationToken);

// Custom cap strategy
var customMesher = new PrismMesher(new CustomCapStrategy());
```

### MesherOptions Class

Configuration options for meshing operations.

```csharp
public sealed class MesherOptions
{
    // Primary properties
    public double TargetEdgeLengthXY { get; set; } = 1.0;
    public double TargetEdgeLengthZ { get; set; } = 1.0;
    public bool GenerateBottomCap { get; set; } = true;
    public bool GenerateTopCap { get; set; } = true;
    public double Epsilon { get; set; } = 1e-9;
    
    // Quality control
    public double MinCapQuadQuality { get; set; } = 0.5;
    public bool OutputRejectedCapTriangles { get; set; } = false;
    
    // Refinement options
    public double? TargetEdgeLengthXYNearHoles { get; set; }
    public double HoleRefineBand { get; set; } = 2.0;
    public double? TargetEdgeLengthXYNearSegments { get; set; }
    public double SegmentRefineBand { get; set; } = 1.0;
    
    // Factory method
    public static MesherOptionsBuilder CreateBuilder();
    
    // Validation
    public void Validate();
}
```

### MesherOptionsBuilder Class

Fluent builder for MesherOptions with presets.

```csharp
public sealed class MesherOptionsBuilder
{
    // Target lengths
    public MesherOptionsBuilder WithTargetEdgeLengthXY(double length);
    public MesherOptionsBuilder WithTargetEdgeLengthZ(double length);
    
    // Cap options
    public MesherOptionsBuilder WithCaps(bool bottom = true, bool top = true);
    
    // Refinement
    public MesherOptionsBuilder WithHoleRefinement(double targetLength, double band);
    public MesherOptionsBuilder WithSegmentRefinement(double targetLength, double band);
    
    // Quality
    public MesherOptionsBuilder WithMinCapQuadQuality(double quality);
    public MesherOptionsBuilder WithRejectedCapTriangles(bool output = true);
    
    // Presets
    public MesherOptionsBuilder WithFastPreset();
    public MesherOptionsBuilder WithHighQualityPreset();
    
    // Other options
    public MesherOptionsBuilder WithEpsilon(double epsilon);
    
    // Build
    public MesherOptions Build();
}
```

**Preset Configurations:**

| Preset | TargetEdgeLengthXY | TargetEdgeLengthZ | MinCapQuadQuality | OutputRejectedCapTriangles |
|--------|-------------------|-------------------|-------------------|---------------------------|
| Fast | 2.0 | 2.0 | 0.3 | false |
| High-Quality | 0.5 | 0.5 | 0.7 | true |

**Usage Examples:**
```csharp
// Fast preset
var fast = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

// Custom configuration
var custom = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(0.5)
    .WithCaps(bottom: true, top: false)
    .WithHoleRefinement(0.5, 1.0)
    .WithMinCapQuadQuality(0.6)
    .WithRejectedCapTriangles(true)
    .Build();
```

### Mesh Class

Mutable mesh container for quads, triangles, points, and segments.

```csharp
public sealed class Mesh : IDisposable
{
    // Constructors
    public Mesh();
    public Mesh(int initialQuadCapacity, int initialTriangleCapacity, 
               int initialPointCapacity, int initialSegmentCapacity);
    
    // Properties
    public int QuadCount { get; }
    public int TriangleCount { get; }
    public ReadOnlyCollection<Quad> Quads { get; }
    public ReadOnlyCollection<Triangle> Triangles { get; }
    public ReadOnlyCollection<Vec3> Points { get; }
    public ReadOnlyCollection<Segment3D> InternalSegments { get; }
    
    // Methods
    public void AddQuad(Quad quad);
    public void AddQuads(IEnumerable<Quad> quads);
    public void AddQuadsSpan(ReadOnlySpan<Quad> quads);
    
    public void AddTriangle(Triangle triangle);
    public void AddTriangles(IEnumerable<Triangle> triangles);
    public void AddTrianglesSpan(ReadOnlySpan<Triangle> triangles);
    
    public void AddPoint(Vec3 point);
    public void AddPoints(IEnumerable<Vec3> points);
    
    public void AddInternalSegment(Segment3D segment);
    
    public void Clear();
    public void Dispose();
}
```

### Quad Struct

Quadrilateral primitive with optional quality score.

```csharp
public readonly struct Quad : IEquatable<Quad>
{
    // Properties
    public Vec3 V0 { get; }
    public Vec3 V1 { get; }
    public Vec3 V2 { get; }
    public Vec3 V3 { get; }
    public double? QualityScore { get; }
    
    // Constructors
    public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3);
    public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3, double? qualityScore);
}
```

### Triangle Struct

Triangle primitive with optional quality score.

```csharp
public readonly struct Triangle : IEquatable<Triangle>
{
    // Properties
    public Vec3 V0 { get; }
    public Vec3 V1 { get; }
    public Vec3 V2 { get; }
    public double? QualityScore { get; }
    
    // Constructors
    public Triangle(Vec3 v0, Vec3 v1, Vec3 v2);
    public Triangle(Vec3 v0, Vec3 v1, Vec3 v2, double? qualityScore);
}
```

### IndexedMesh Class

Immutable indexed representation of a mesh.

```csharp
public sealed class IndexedMesh
{
    // Properties
    public ReadOnlyCollection<Vec3> Vertices { get; }
    public ReadOnlyCollection<(int a, int b)> Edges { get; }
    public ReadOnlyCollection<(int v0, int v1, int v2, int v3)> Quads { get; }
    public ReadOnlyCollection<(int v0, int v1, int v2)> Triangles { get; }
    
    // Count properties (optimized)
    public int VertexCount { get; }
    public int EdgeCount { get; }
    public int QuadCount { get; }
    public int TriangleCount { get; }
    
    // Factory method
    public static IndexedMesh FromMesh(Mesh mesh, double epsilon = 1e-9);
    
    // Legacy format support
    public static IndexedMesh ReadCustomTxt(string path);
    public void WriteCustomTxt(string path);
    
    // Adjacency analysis
    public MeshAdjacency BuildAdjacency();
}
```

## Structure Definitions

### PrismStructureDefinition Class

Immutable definition of a prismatic structure.

```csharp
public sealed class PrismStructureDefinition
{
    // Properties
    public Polygon2D Footprint { get; }
    public double BaseElevation { get; }
    public double TopElevation { get; }
    public IReadOnlyList<Polygon2D> Holes { get; }
    public IReadOnlyList<(Segment2D segment, double z)> ConstraintSegments { get; }
    public IReadOnlyList<InternalSurfaceDefinition> InternalSurfaces { get; }
    public MeshingGeometry Geometry { get; }
    
    // Constructor
    public PrismStructureDefinition(Polygon2D footprint, double baseElevation, double topElevation);
    
    // Builder methods (return new instances)
    public PrismStructureDefinition AddHole(Polygon2D hole);
    public PrismStructureDefinition AddConstraintSegment(Segment2D segment, double z);
    public PrismStructureDefinition AddInternalSurface(Polygon2D outer, double z, params Polygon2D[] holes);
}
```

**Usage Examples:**
```csharp
// Basic structure
var structure = new PrismStructureDefinition(footprint, -10, 10);

// Add features (immutable - returns new instances)
structure = structure
    .AddHole(holePolygon)
    .AddConstraintSegment(new Segment2D(p1, p2), 5.0)
    .AddInternalSurface(slabOutline, 2.5, slabHole);

// Add auxiliary geometry
structure.Geometry
    .AddPoint(new Vec3(5, 5, 7))
    .AddSegment(new Segment3D(start, end));
```

### InternalSurfaceDefinition Class

Definition of horizontal internal surfaces (slabs) with holes.

```csharp
public sealed class InternalSurfaceDefinition
{
    // Properties
    public Polygon2D Outer { get; }
    public double Elevation { get; }
    public IReadOnlyList<Polygon2D> Holes { get; }
    
    // Constructor
    public InternalSurfaceDefinition(Polygon2D outer, double elevation, 
                                   IEnumerable<Polygon2D>? holes = null);
}
```

### MeshingGeometry Class

Container for auxiliary geometry to be preserved in the mesh.

```csharp
public sealed class MeshingGeometry
{
    // Properties
    public ReadOnlyCollection<Vec3> Points { get; }
    public ReadOnlyCollection<Segment3D> Segments { get; }
    
    // Methods (fluent interface)
    public MeshingGeometry AddPoint(Vec3 point);
    public MeshingGeometry AddSegment(Segment3D segment);
    public MeshingGeometry AddPoints(IEnumerable<Vec3> points);
    public MeshingGeometry AddSegments(IEnumerable<Segment3D> segments);
}
```

## Export API

### ObjExporter Class

Export meshes to Wavefront OBJ format.

```csharp
public static class ObjExporter
{
    public static void Write(IndexedMesh mesh, string filePath);
}
```

**Output Format:**
- Preserves both quads and triangles
- Quads: `f v1 v2 v3 v4 v5` (5 vertices, last repeats first)
- Triangles: `f v1 v2 v3 v4` (4 vertices, last repeats first)

### GltfExporter Class

Export meshes to glTF 2.0 format.

```csharp
public static class GltfExporter
{
    public static void Write(IndexedMesh mesh, string filePath);
}
```

**Features:**
- Self-contained glTF with embedded binary data
- All geometry triangulated
- Compatible with web viewers and game engines

### SvgExporter Class

Export mesh topology to SVG format (top view).

```csharp
public static class SvgExporter
{
    public static void Write(IndexedMesh mesh, string filePath);
}
```

**Features:**
- 2D top-view projection of mesh edges
- Useful for debugging mesh topology
- Viewable in web browsers

## Utility Types

### MeshAdjacency Class

Adjacency information for mesh analysis.

```csharp
public sealed class MeshAdjacency
{
    // Properties
    public int QuadCount { get; }
    public IReadOnlyList<int[]> Neighbors { get; }  // 4 neighbors per quad (-1 if none)
    public ReadOnlyCollection<(int a, int b)> BoundaryEdges { get; }
    public ReadOnlyCollection<(int a, int b)> NonManifoldEdges { get; }
    
    // Factory method
    public static MeshAdjacency Build(IndexedMesh mesh);
}
```

## Performance Monitoring

### PerformanceMonitor Class

Static performance monitoring utilities.

```csharp
public static class PerformanceMonitor
{
    public static class Counters
    {
        public static void IncrementMeshingOperations();
        public static void AddQuadsGenerated(int count);
        public static void AddTrianglesGenerated(int count);
        public static void IncrementPoolHit();
        public static void IncrementPoolMiss();
        
        public static PerformanceStatistics GetStatistics();
        public static void Reset();
    }
}

public readonly struct PerformanceStatistics
{
    public long MeshingOperations { get; }
    public long TotalQuadsGenerated { get; }
    public long TotalTrianglesGenerated { get; }
    public long PoolHits { get; }
    public long PoolMisses { get; }
    
    public double AverageQuadsPerOperation { get; }
    public double AverageTrianglesPerOperation { get; }
    public double PoolHitRatio { get; }
}
```

**Usage Example:**
```csharp
// Reset counters
PerformanceMonitor.Counters.Reset();

// Perform operations...
var mesh = mesher.Mesh(structure, options);

// Get statistics
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Operations: {stats.MeshingOperations}");
Console.WriteLine($"Avg quads/op: {stats.AverageQuadsPerOperation:F1}");
Console.WriteLine($"Pool hit ratio: {stats.PoolHitRatio:P1}");
```

## Type Compatibility

All geometric types implement appropriate interfaces:
- `IEquatable<T>` for value equality
- Proper `GetHashCode()` implementations
- `ToString()` for debugging

## Thread Safety

- **Structure Definitions**: Immutable, fully thread-safe
- **Meshers**: Stateless, thread-safe for concurrent use
- **Mesh Objects**: Not thread-safe (use separately per thread)
- **IndexedMesh**: Immutable after creation, thread-safe for reading

This completes the comprehensive API reference. For usage examples and patterns, see the [Usage Guide](usage-guide.md).
