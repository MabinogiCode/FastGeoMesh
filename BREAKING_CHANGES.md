# 🚨 Breaking Changes - FastGeoMesh

This document outlines **all breaking changes** for FastGeoMesh, focusing on major releases.

## 📋 Summary of Breaking Changes

| Version | Change Category | Impact Level | Migration Effort |
|---------|-----------------|--------------|------------------|
| **v2.1** | **DI & Constructor** | 🔴 **High** | Low (DI setup) |
| **v2.0** | **Namespace Restructuring** | 🔴 **High** | Low (find/replace) |
| **v2.0** | **Result Pattern Introduction** | 🟡 **Medium** | Medium (API changes) |
| **v2.0** | **Builder Pattern for Options** | 🟡 **Medium** | Low (straightforward) |

---

## 🏗️ Version 2.1 - Dependency Injection

### **BREAKING**: `PrismMesher` Constructor Change

The `PrismMesher` no longer has a parameterless constructor. You must now provide its dependencies.

#### **Before (v2.0):**
```csharp
// This was valid but is now obsolete and removed.
var mesher = new PrismMesher(); 
```

#### **After (v2.1):**

**Option A: Use Dependency Injection (Recommended)**
```csharp
// 1. Register services (e.g., in Program.cs)
services.AddFastGeoMesh();

// 2. Inject IPrismMesher into your class
public class MyService
{
    private readonly IPrismMesher _mesher;
    public MyService(IPrismMesher mesher) => _mesher = mesher;
}
```

**Option B: Manual Construction**
```csharp
using FastGeoMesh.Application.Services;
using FastGeoMesh.Infrastructure.Services;

// Manually create and inject dependencies
var geometryService = new GeometryService();
var zLevelBuilder = new ZLevelBuilder();
var proximityChecker = new ProximityChecker();
var mesher = new PrismMesher(geometryService, zLevelBuilder, proximityChecker);
```

#### **Migration Steps:**
1. **Choose your approach**: DI (recommended) or manual construction.
2. If using DI, call `services.AddFastGeoMesh()` in your application's startup.
3. Update all `new PrismMesher()` calls to use one of the new methods.
4. **See `MIGRATION_GUIDE_DI.md` for detailed examples.**

---

## 🏗️ Version 2.0 - Clean Architecture & Result Pattern

### **BREAKING**: Namespace Restructuring

**All namespace imports must be updated** due to Clean Architecture implementation.

#### **Before (v1.x):**
```csharp
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Geometry;
```

#### **After (v2.0+):**
```csharp
using FastGeoMesh.Domain;           // Core entities (Vec2, Vec3, Polygon2D, etc.)
using FastGeoMesh.Application;      // Meshing logic (PrismMesher, MesherOptions)
using FastGeoMesh.Infrastructure;   // External services (Exporters, Performance)
```

### **BREAKING**: Result-Based Error Handling

**All meshing operations now return `Result<T>` instead of throwing exceptions.**

#### **Before (v1.x):**
```csharp
try 
{
    var mesh = mesher.Mesh(structure, options);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

#### **After (v2.0+):**
```csharp
var meshResult = mesher.Mesh(structure, options);
if (meshResult.IsSuccess)
{
    var mesh = meshResult.Value;
}
else
{
    Console.WriteLine($"Error: {meshResult.Error.Description}");
}
```

### **BREAKING**: Builder Pattern for `MesherOptions`

**`MesherOptions` creation now requires using the builder pattern.**

#### **Before (v1.x):**
```csharp
var options = new MesherOptions { TargetEdgeLengthXY = 1.0 };
```

#### **After (v2.0+):**
```csharp
var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .Build();

if (optionsResult.IsFailure)
{
    // Handle validation error
    return;
}
var options = optionsResult.Value;
```

---

## 📞 Support & Resources

- **Migration Support**: [GitHub Discussions](https://github.com/MabinogiCode/FastGeoMesh/discussions)
- **Bug Reports**: [GitHub Issues](https://github.com/MabinogiCode/FastGeoMesh/issues)
- **DI Migration Guide**: [MIGRATION_GUIDE_DI.md](MIGRATION_GUIDE_DI.md)
- **API Documentation**: See the main `README.md` file.
