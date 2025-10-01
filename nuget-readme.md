# FastGeoMesh

**🇬🇧 English** | [🇫🇷 Français](#français)

---

## English

[![CI](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/MabinogiCode/FastGeoMesh/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastGeoMesh)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/FastGeoMesh.svg)](https://www.nuget.org/packages/FastGeoMesh/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Fast quad meshing for prismatic volumes from 2D footprints and Z elevations.**

FastGeoMesh is a high-performance .NET 8 library for generating quad-dominant meshes from 2.5D prismatic structures. Perfect for CAD, GIS, and real-time applications requiring sub-millisecond meshing performance.

## ⚡ Performance

**Sub-millisecond meshing** with .NET 8 optimizations:
- **Simple Prism**: ~305 μs, 87 KB
- **Complex Geometry**: ~340 μs, 87 KB  
- **With Holes**: ~907 μs, 1.3 MB
- **Geometry Operations**: < 10 μs, zero allocations

*Benchmarked on .NET 8.0.20, X64 RyuJIT AVX2*

## 🚀 Features

- **🏗️ Prism Mesher**: Generate side faces and caps from 2D footprints
- **📐 Smart Fast-Paths**: Rectangle optimization + generic tessellation fallback
- **🎯 Quality Control**: Quad quality scoring & configurable thresholds
- **📑 Triangle Fallback**: Optional explicit cap triangles for low-quality quads
- **⚙️ Constraint System**: Z-level segments & integrated auxiliary geometry
- **📤 Multi-Format Export**: OBJ (quads+triangles), glTF (triangulated), SVG (top view)
- **🔧 Performance Presets**: Fast vs High-Quality configurations
- **🧵 Thread-Safe**: Immutable structures, stateless meshers

## 🚀 Quick Start

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// Define geometry
var poly = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) 
});
var structure = new PrismStructureDefinition(poly, -10, 10);

// Add constraint at Z = 2.5
structure = structure.AddConstraintSegment(
    new Segment2D(new Vec2(0,0), new Vec2(20,0)), 2.5);

// Configure options with preset
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()                    // ~305μs performance
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithRejectedCapTriangles(true)      // Include triangle fallbacks
    .Build();

// Generate mesh
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

// Export to multiple formats
ObjExporter.Write(indexed, "mesh.obj");      // Quads + triangles
GltfExporter.Write(indexed, "mesh.gltf");    // Triangulated
SvgExporter.Write(indexed, "mesh.svg");      // Top view
```

## 🎚️ Performance Presets

```csharp
// Fast: ~305μs, 87KB - Real-time applications
var fast = MesherOptions.CreateBuilder().WithFastPreset().Build();

// High-Quality: ~1.3ms, 17MB - CAD precision  
var quality = MesherOptions.CreateBuilder().WithHighQualityPreset().Build();
```

## 🏗️ Advanced Features

### Complex Structures with Holes
```csharp
var outer = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(10,0), new Vec2(10,6), new Vec2(0,6) 
});
var hole = Polygon2D.FromPoints(new[]{ 
    new Vec2(2,2), new Vec2(4,2), new Vec2(4,4), new Vec2(2,4) 
});

var structure = new PrismStructureDefinition(outer, 0, 2)
    .AddHole(hole);

var options = MesherOptions.CreateBuilder()
    .WithHoleRefinement(0.75, 1.0)       // Refine near holes
    .Build();
```

### Internal Surfaces (Slabs)
```csharp
// Add horizontal slab at Z = -2.5 with hole
var slabOutline = Polygon2D.FromPoints(/*...*/);
var slabHole = Polygon2D.FromPoints(/*...*/);

structure = structure.AddInternalSurface(slabOutline, -2.5, slabHole);
```

### Auxiliary Geometry
```csharp
structure.Geometry
    .AddPoint(new Vec3(0, 4, 2))
    .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 2)));
```

**📖 Full Documentation**: https://github.com/MabinogiCode/FastGeoMesh  
**📋 API Reference**: https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/api-reference.md  
**⚡ Performance Guide**: https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/performance-guide.md

**License**: MIT

---

## Français

**Maillage rapide de quads pour volumes prismatiques à partir d'empreintes 2D et d'élévations Z.**

FastGeoMesh est une bibliothèque .NET 8 haute performance pour générer des maillages à dominante quadrilatérale à partir de structures prismatiques 2.5D. Parfaite pour les applications CAO, SIG et temps réel nécessitant des performances de maillage inférieures à la milliseconde.

## ⚡ Performance

**Maillage sous-milliseconde** avec optimisations .NET 8 :
- **Prisme Simple** : ~305 μs, 87 Ko
- **Géométrie Complexe** : ~340 μs, 87 Ko  
- **Avec Trous** : ~907 μs, 1,3 Mo
- **Opérations Géométriques** : < 10 μs, zéro allocation

*Testé sur .NET 8.0.20, X64 RyuJIT AVX2*

## 🚀 Fonctionnalités

- **🏗️ Mailleur de Prismes** : Génère faces latérales et chapeaux depuis empreintes 2D
- **📐 Chemins Rapides Intelligents** : Optimisation rectangle + tessellation générique
- **🎯 Contrôle Qualité** : Scoring qualité des quads & seuils configurables
- **📑 Triangles de Secours** : Triangles explicites optionnels pour quads de faible qualité
- **⚙️ Système de Contraintes** : Segments de niveau Z & géométrie auxiliaire intégrée
- **📤 Export Multi-Format** : OBJ (quads+triangles), glTF (triangulé), SVG (vue de dessus)
- **🔧 Préréglages Performance** : Configurations Rapide vs Haute-Qualité
- **🧵 Thread-Safe** : Structures immutables, mailleurs sans état

## 🚀 Démarrage Rapide

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// Définir la géométrie
var poly = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) 
});
var structure = new PrismStructureDefinition(poly, -10, 10);

// Ajouter contrainte à Z = 2.5
structure = structure.AddConstraintSegment(
    new Segment2D(new Vec2(0,0), new Vec2(20,0)), 2.5);

// Configurer options avec préréglage
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()                    // Performance ~305μs
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithRejectedCapTriangles(true)      // Inclure triangles de secours
    .Build();

// Générer le maillage
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

// Export vers formats multiples
ObjExporter.Write(indexed, "mesh.obj");      // Quads + triangles
GltfExporter.Write(indexed, "mesh.gltf");    // Triangulé
SvgExporter.Write(indexed, "mesh.svg");      // Vue de dessus
```

## 🎚️ Préréglages Performance

```csharp
// Rapide : ~305μs, 87Ko - Applications temps réel
var rapide = MesherOptions.CreateBuilder().WithFastPreset().Build();

// Haute-Qualité : ~1,3ms, 17Mo - Précision CAO  
var qualite = MesherOptions.CreateBuilder().WithHighQualityPreset().Build();
```

## 🏗️ Fonctionnalités Avancées

### Structures Complexes avec Trous
```csharp
var outer = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(10,0), new Vec2(10,6), new Vec2(0,6) 
});
var hole = Polygon2D.FromPoints(new[]{ 
    new Vec2(2,2), new Vec2(4,2), new Vec2(4,4), new Vec2(2,4) 
});

var structure = new PrismStructureDefinition(outer, 0, 2)
    .AddHole(hole);

var options = MesherOptions.CreateBuilder()
    .WithHoleRefinement(0.75, 1.0)       // Affiner près des trous
    .Build();
```

### Surfaces Internes (Dalles)
```csharp
// Ajouter une dalle horizontale à Z = -2.5 avec trou
var slabOutline = Polygon2D.FromPoints(/*...*/);
var slabHole = Polygon2D.FromPoints(/*...*/);

structure = structure.AddInternalSurface(slabOutline, -2.5, slabHole);
```

### Géométrie Auxiliaire
```csharp
structure.Geometry
    .AddPoint(new Vec3(0, 4, 2))
    .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 2)));
```

**📖 Documentation Complète** : https://github.com/MabinogiCode/FastGeoMesh  
**📋 Référence API** : https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/api-reference-fr.md  
**⚡ Guide Performance** : https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/performance-guide-fr.md

**Licence** : MIT
