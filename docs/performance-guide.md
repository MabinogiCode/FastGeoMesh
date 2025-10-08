# FastGeoMesh v2.0 - Performance Guide

This guide covers performance optimization strategies, benchmarks, and best practices for FastGeoMesh v2.0.

## 🚀 Quick Performance Wins

### **1. Use Async APIs (40-80% Faster)**
```csharp
// ❌ Slow - Synchronous
var result = mesher.Mesh(structure, options);

// ✅ Fast - Asynchronous (40-80% faster!)
var result = await mesher.MeshAsync(structure, options);
```

### **2. Use Performance Presets**
```csharp
// ✅ Fast meshing for prototyping
var fastOptions = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build().Value;

// ✅ High quality for production
var qualityOptions = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .Build().Value;
```

### **3. Enable Batch Processing (2.2x Speedup)**
```csharp
// ❌ Slow - Sequential processing
foreach (var structure in structures)
{
    var result = await mesher.MeshAsync(structure, options);
    results.Add(result);
}

// ✅ Fast - Parallel batch processing (2.2x faster!)
var batchResult = await mesher.MeshBatchAsync(structures, options);
```

## ⚡ Performance Benchmarks

### **Async vs Sync Performance (v2.0)**

| Structure Type | Sync Time | Async Time | **Improvement** |
|----------------|-----------|------------|----------------|
| **Trivial (Rectangle)** | ~559 μs | ~311 μs | **🚀 78% faster** |
| **Simple (L-shape)** | ~348 μs | ~202 μs | **🚀 42% faster** |
| **Complex (Multi-hole)** | ~580 μs | ~340 μs | **🚀 41% faster** |
| **Large (1000+ quads)** | ~2.1 ms | ~1.2 ms | **🚀 43% faster** |

### **v2.0 vs v1.x Comparison**

| Operation | v1.x Time | v2.0 Sync | v2.0 Async | **Total Improvement** |
|-----------|-----------|-----------|------------|----------------------|
| Simple Rectangle | ~1,420 μs | ~559 μs | ~311 μs | **🚀 78% faster** |
| L-shaped Structure | ~680 μs | ~348 μs | ~202 μs | **🚀 70% faster** |
| Complex Geometry | ~980 μs | ~580 μs | ~340 μs | **🚀 65% faster** |

### **Batch Processing Performance**

| Batch Size | Sequential Time | Parallel Time | **Speedup** |
|------------|----------------|---------------|-------------|
| 8 structures | 1.8 ms | 0.9 ms | **2.0x** |
| 16 structures | 3.5 ms | 1.6 ms | **2.2x** |
| 32 structures | 7.4 ms | 3.3 ms | **2.2x** |
| 64 structures | 14.8 ms | 6.9 ms | **2.1x** |

*Benchmarked on .NET 8, X64 RyuJIT AVX2*

## 🎯 Performance Optimization Strategies

### **1. Choose the Right Target Edge Lengths**

```csharp
// ❌ Too small - Unnecessary detail, slow performance
.WithTargetEdgeLengthXY(0.01)   // Creates millions of quads

// ✅ Balanced - Good quality, fast performance  
.WithTargetEdgeLengthXY(0.5)    // Creates reasonable mesh density

// ✅ Fast - Coarse mesh, maximum speed
.WithTargetEdgeLengthXY(2.0)    // Creates minimal quads
```

### **2. Optimize Cap Generation**

```csharp
// ❌ Slow - Both caps with high quality
.WithCaps(bottom: true, top: true)
.WithMinCapQuadQuality(0.9)

// ✅ Fast - Only necessary caps
.WithCaps(bottom: true, top: false)  // Only bottom cap
.WithMinCapQuadQuality(0.3)          // Lower quality = faster

// ⚡ Fastest - No caps (sides only)
.WithCaps(bottom: false, top: false)
```

### **3. Use Rectangle Fast Path**

```csharp
// ✅ Fastest - Rectangle structures use optimized algorithms
var rectangle = Polygon2D.FromPoints(new[]
{
    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
});
// Automatically detected and optimized!

// ❌ Slower - Complex polygons require general tessellation
var complexShape = Polygon2D.FromPoints(complexVertices);
```

### **4. Minimize Hole Complexity**

```csharp
// ✅ Fast - Simple holes
var simpleHole = Polygon2D.FromPoints(new[]
{
    new Vec2(2, 2), new Vec2(4, 2), new Vec2(4, 4), new Vec2(2, 4)
});

// ❌ Slower - Many complex holes
foreach (var complexHole in manyComplexHoles)
{
    structure = structure.AddHole(complexHole);  // Each hole adds complexity
}
```

## 💡 Architecture Performance Benefits

### **Clean Architecture Optimizations**

1. **Domain Layer**: Immutable structs avoid allocations
   ```csharp
   // ✅ Vec2/Vec3 are value types - no heap allocations
   var vertex = new Vec3(x, y, z);  // Stack allocation
   ```

2. **Application Layer**: Aggressive inlining for hot paths
   ```csharp
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static double CalculateDistance(Vec2 a, Vec2 b)
   ```

3. **Infrastructure Layer**: Object pooling for temporary collections
   ```csharp
   // ✅ Reused collections from object pool
   var tempList = MeshingPools.IntListPool.Get();
   // ... use list ...
   MeshingPools.IntListPool.Return(tempList);
   ```

### **SIMD Optimizations**

FastGeoMesh v2.0 uses SIMD operations for batch calculations:

```csharp
// ✅ Vectorized operations for multiple Vec3 calculations
Vec3.Add(sourceSpan, operandSpan, destinationSpan);        // SIMD batch add
Vec3.Cross(aSpan, bSpan, resultSpan);                      // SIMD batch cross product
var dotSum = Vec3.AccumulateDot(aSpan, bSpan);             // SIMD batch dot products
```

## 🏆 Performance Best Practices Summary

### **Do's ✅**
- **Use async APIs** for 40-80% performance improvement
- **Use performance presets** for optimized defaults
- **Enable batch processing** for multiple structures
- **Choose appropriate edge lengths** for your use case
- **Use rectangle fast-paths** when possible
- **Minimize hole complexity** in polygons

### **Don'ts ❌**
- Don't use synchronous APIs for production performance
- Don't set edge lengths too small unnecessarily
- Don't generate both caps if you only need one
- Don't ignore complexity estimation warnings
- Don't create unnecessary holes in polygons

---

**🚀 Result**: Following these guidelines should give you **40-80% performance improvements** over v1.x and optimal performance in v2.0!
