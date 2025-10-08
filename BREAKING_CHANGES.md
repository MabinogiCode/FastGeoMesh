# 🚨 Breaking Changes - FastGeoMesh v2.0

This document outlines **all breaking changes** introduced in FastGeoMesh v2.0 and provides **detailed migration instructions**.

## 📋 Summary of Breaking Changes

| Change Category | Impact Level | Migration Effort |
|----------------|--------------|------------------|
| **Namespace Restructuring** | 🔴 **High** | Low (find/replace) |
| **Result Pattern Introduction** | 🟡 **Medium** | Medium (API changes) |
| **Builder Pattern for Options** | 🟡 **Medium** | Low (straightforward) |
| **Immutable Mesh Objects** | 🟠 **Low** | Low (mostly transparent) |
| **Async-First APIs** | 🟢 **Optional** | Optional (performance) |

---

## 🏗️ 1. Clean Architecture - Namespace Changes

### **BREAKING**: Namespace Restructuring

**All namespace imports must be updated** due to Clean Architecture implementation.

#### **Before (v1.x):**
```csharp
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Geometry;
using FastGeoMesh.Exporters;
using FastGeoMesh.Utils;
```

#### **After (v2.0):**
```csharp
using FastGeoMesh.Domain;           // Core entities (Vec2, Vec3, Polygon2D, etc.)
using FastGeoMesh.Application;      // Meshing logic (PrismMesher, MesherOptions)
using FastGeoMesh.Infrastructure;   // External services (Exporters, Performance)
```

#### **Migration Steps:**
1. **Global find/replace** in your solution:
   - `using FastGeoMesh.Meshing;` → `using FastGeoMesh.Application;`
   - `using FastGeoMesh.Structures;` → `using FastGeoMesh.Domain;`
   - `using FastGeoMesh.Geometry;` → `using FastGeoMesh.Domain;`
   - `using FastGeoMesh.Exporters;` → `using FastGeoMesh.Infrastructure;`
   - `using FastGeoMesh.Utils;` → `using FastGeoMesh.Infrastructure;`

2. **Verify compilation** after namespace updates
3. **No code logic changes required** - only imports

---

## 🎯 2. Result Pattern - Error Handling Revolution

### **BREAKING**: Exception-Based → Result-Based Error Handling

**All meshing operations now return `Result<T>` instead of throwing exceptions.**

#### **Before (v1.x):**
```csharp
// Exception-based error handling
try 
{
    var mesh = mesher.Mesh(structure, options);
    ProcessMesh(mesh);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid argument: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Operation failed: {ex.Message}");
}
```

#### **After (v2.0):**
```csharp
// Result-based error handling
var meshResult = mesher.Mesh(structure, options);
if (meshResult.IsSuccess)
{
    ProcessMesh(meshResult.Value);
}
else
{
    Console.WriteLine($"Error {meshResult.Error.Code}: {meshResult.Error.Description}");
}
```

#### **Alternative: Functional Style with Match**
```csharp
var result = mesher.Mesh(structure, options);
result.Match(
    success => ProcessMesh(success),
    error => LogError(error.Code, error.Description)
);
```

#### **Migration Steps:**
1. **Replace try-catch blocks** with Result checking
2. **Use `.IsSuccess`/`.IsFailure`** instead of exceptions
3. **Access `.Value`** only when `.IsSuccess == true`
4. **Handle `.Error`** when `.IsFailure == true`
5. **Consider using `.Match()`** for cleaner functional code

---

## ⚙️ 3. Builder Pattern - MesherOptions Creation

### **BREAKING**: Direct Instantiation → Builder Pattern with Validation

**MesherOptions creation now requires using the builder pattern and returns a Result.**

#### **Before (v1.x):**
```csharp
var options = new MesherOptions
{
    TargetEdgeLengthXY = 1.0,
    TargetEdgeLengthZ = 0.5,
    GenerateBottomCap = true,
    GenerateTopCap = true,
    MinCapQuadQuality = 0.7
};
```

#### **After (v2.0):**
```csharp
var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(0.5)
    .WithCaps(bottom: true, top: true)
    .WithMinCapQuadQuality(0.7)
    .Build();

if (optionsResult.IsFailure)
{
    Console.WriteLine($"Invalid options: {optionsResult.Error.Description}");
    return;
}

var options = optionsResult.Value;
```

#### **Migration Steps:**
1. **Replace object initializers** with builder calls
2. **Handle validation results** from `.Build()`
3. **Use preset methods** for common configurations:
   ```csharp
   var fastOptions = MesherOptions.CreateBuilder()
       .WithFastPreset()
       .Build().UnwrapForTests(); // Only in tests!
   
   var qualityOptions = MesherOptions.CreateBuilder()
       .WithHighQualityPreset()
       .Build().UnwrapForTests(); // Only in tests!
   ```

---

## 🔄 4. Immutable Mesh Objects

### **BREAKING**: Mutable → Immutable Mesh Structures

**All mesh objects are now immutable and use builder patterns for construction.**

#### **Before (v1.x):**
```csharp
var mesh = new Mesh();
mesh.AddQuad(quad1);
mesh.AddQuad(quad2);
mesh.AddTriangle(triangle);
```

#### **After (v2.0):**
```csharp
var mesh = new ImmutableMesh()
    .AddQuad(quad1)
    .AddQuad(quad2)
    .AddTriangle(triangle);

// Or add collections at once
var mesh = new ImmutableMesh()
    .AddQuads(new[] { quad1, quad2 })
    .AddTriangles(new[] { triangle });
```

#### **Migration Steps:**
1. **Chain method calls** instead of mutating objects
2. **Assign results** of add operations to variables
3. **Use collection methods** for bulk additions
4. **Thread safety** is now guaranteed (benefit!)

---

## 🚀 5. Async-First Performance APIs

### **NEW**: Enhanced Async Support (Optional Migration)

**New async APIs provide significant performance improvements but are optional.**

#### **Synchronous (Still Supported):**
```csharp
var result = mesher.Mesh(structure, options);
```

#### **Async (Recommended for Performance):**
```csharp
var result = await mesher.MeshAsync(structure, options);
// 40-80% faster for complex geometries!
```

#### **Batch Operations:**
```csharp
var structures = new[] { structure1, structure2, structure3 };
var results = await mesher.MeshBatchAsync(structures, options);
// 2.2x speedup with parallel processing!
```

#### **Migration Steps:**
1. **Optional**: Add `async`/`await` to methods
2. **Recommended**: Use `MeshAsync()` for performance
3. **Consider**: Batch operations for multiple structures
4. **No breaking changes** - sync APIs still work

---

## 🏷️ 6. Type Safety Improvements

### **BREAKING**: Primitive Types → Value Objects

**Some primitive parameters are now strongly-typed value objects.**

#### **Before (v1.x):**
```csharp
var options = new MesherOptions
{
    TargetEdgeLengthXY = 1.0,      // double
    TargetEdgeLengthZ = 0.5,       // double
    Epsilon = 1e-9                 // double
};
```

#### **After (v2.0):**
```csharp
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)   // Automatically wrapped in EdgeLength
    .WithTargetEdgeLengthZ(0.5)    // Automatically wrapped in EdgeLength
    .Build().Value;

// Access values if needed:
double xyValue = options.TargetEdgeLengthXY.Value;
double zValue = options.TargetEdgeLengthZ.Value;
```

#### **Migration Steps:**
1. **Use builder methods** - automatic type conversion
2. **Access `.Value`** property when needed
3. **Validation is now built-in** - prevents invalid values

---

## 📤 7. Export API Changes

### **MINOR BREAKING**: Exporter Reorganization

**Export classes moved to Infrastructure namespace with enhanced capabilities.**

#### **Before (v1.x):**
```csharp
using FastGeoMesh.Exporters;

ObjExporter.Export(mesh, "output.obj");
```

#### **After (v2.0):**
```csharp
using FastGeoMesh.Infrastructure.Exporters;

var indexed = IndexedMesh.FromMesh(mesh);
ObjExporter.Write(indexed, "output.obj");

// New flexible TXT exporter
indexed.ExportTxt()
    .WithPoints("v", CountPlacement.Top, indexBased: true)
    .WithQuads("f", CountPlacement.None, indexBased: true)
    .ToFile("output.txt");
```

#### **Migration Steps:**
1. **Update using statements** to Infrastructure namespace
2. **Convert to IndexedMesh** before exporting
3. **Use new exporter methods** (Write instead of Export)
4. **Explore new export formats** (glTF, SVG, flexible TXT)

---

## 🧪 8. Testing Utilities

### **NEW**: Test Helpers for Result Pattern

**New test utilities to simplify testing with Result pattern.**

#### **In Tests Only:**
```csharp
// Use .UnwrapForTests() extension in test code
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build().UnwrapForTests(); // Throws if invalid - only use in tests!

var mesh = mesher.Mesh(structure, options).UnwrapForTests();

// Async version
var asyncMesh = await mesher.MeshAsync(structure, options).UnwrapForTestsAsync();
```

⚠️ **Never use `.UnwrapForTests()` in production code!** It throws exceptions and defeats the purpose of the Result pattern.

---

## 🛠️ Migration Toolkit

### **Automated Migration Steps:**

1. **Update Project File** (.csproj):
   ```xml
   <PackageReference Include="FastGeoMesh" Version="2.0.0" />
   ```

2. **Global Find/Replace** (in order):
   ```
   Find: using FastGeoMesh.Meshing;
   Replace: using FastGeoMesh.Application;

   Find: using FastGeoMesh.Structures;
   Replace: using FastGeoMesh.Domain;

   Find: using FastGeoMesh.Geometry;
   Replace: using FastGeoMesh.Domain;

   Find: using FastGeoMesh.Exporters;
   Replace: using FastGeoMesh.Infrastructure.Exporters;
   ```

3. **Update Error Handling Pattern**:
   - Search for: `try {` + `mesher.Mesh`
   - Replace with Result pattern checking

4. **Update Options Creation**:
   - Search for: `new MesherOptions {`
   - Replace with builder pattern

5. **Test Compilation**:
   ```bash
   dotnet build
   dotnet test
   ```

### **Validation Tools:**

Run these commands to validate your migration:

```bash
# Check for old namespace usage
grep -r "using FastGeoMesh.Meshing" src/
grep -r "using FastGeoMesh.Structures" src/
grep -r "using FastGeoMesh.Geometry" src/

# Check for old exception handling
grep -r "try.*mesher\.Mesh" src/

# Check for old options creation  
grep -r "new MesherOptions" src/
```

---

## 🎯 Migration Priorities

### **Phase 1 (Required - Breaking Changes):**
1. ✅ Update namespaces (5 minutes)
2. ✅ Update MesherOptions creation (15 minutes)
3. ✅ Update error handling to Result pattern (30 minutes)
4. ✅ Test compilation and basic functionality

### **Phase 2 (Recommended - Performance):**
1. 🚀 Add async/await to meshing calls (15 minutes)
2. 🚀 Use batch operations where applicable (10 minutes)
3. 🚀 Profile performance improvements

### **Phase 3 (Optional - Enhanced Features):**
1. 💡 Explore new export formats
2. 💡 Add progress reporting
3. 💡 Use complexity estimation
4. 💡 Implement performance monitoring

---

## 🆘 Troubleshooting

### **Common Migration Issues:**

#### **1. Compilation Errors After Namespace Update**
```
Error: The type or namespace name 'Polygon2D' could not be found
```
**Solution**: Add `using FastGeoMesh.Domain;`

#### **2. Result Pattern Confusion**
```
Error: Cannot implicitly convert type 'Result<ImmutableMesh>' to 'ImmutableMesh'
```
**Solution**: Check `.IsSuccess` and access `.Value`:
```csharp
var result = mesher.Mesh(structure, options);
if (result.IsSuccess)
{
    var mesh = result.Value; // ✅ Correct
}
```

#### **3. Builder Pattern Validation Failures**
```
Error: Building failed: Invalid edge length value
```
**Solution**: Check parameter values and handle builder result:
```csharp
var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0) // ✅ Must be > 0
    .Build();

if (optionsResult.IsFailure)
{
    Console.WriteLine(optionsResult.Error.Description);
}
```

#### **4. Async Method Signatures**
```
Error: Cannot await 'Result<ImmutableMesh>'
```
**Solution**: Use `MeshAsync()` instead of `Mesh()`:
```csharp
var result = await mesher.MeshAsync(structure, options); // ✅ Correct
```

---

## 📞 Support & Resources

- **Migration Support**: [GitHub Discussions](https://github.com/MabinogiCode/FastGeoMesh/discussions)
- **Bug Reports**: [GitHub Issues](https://github.com/MabinogiCode/FastGeoMesh/issues)
- **API Documentation**: [GitHub Repository](https://github.com/MabinogiCode/FastGeoMesh)
- **Performance Guide**: [docs/performance-guide.md](docs/performance-guide.md)

---

## ✅ Migration Checklist

Use this checklist to ensure complete migration:

- [ ] Updated all namespace imports
- [ ] Converted MesherOptions creation to builder pattern
- [ ] Updated error handling to Result pattern
- [ ] Converted mesh operations to immutable patterns
- [ ] Updated export calls to new API
- [ ] Tested compilation successfully
- [ ] Verified basic functionality works
- [ ] (Optional) Added async/await for performance
- [ ] (Optional) Implemented batch operations
- [ ] (Optional) Added progress reporting
- [ ] Updated tests to use new patterns
- [ ] Updated documentation/comments

---

🎉 **Congratulations!** You've successfully migrated to FastGeoMesh v2.0 with **Clean Architecture**, **Result Pattern**, and **massive performance improvements**!
