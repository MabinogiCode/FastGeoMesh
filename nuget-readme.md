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

### Basic Geometry Creation
```csharp
// Rectangle from corner points
var rect = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(10,0), new Vec2(10,5), new Vec2(0,5) 
});

// Square helper
var square = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(5,0), new Vec2(5,5), new Vec2(0,5) 
});

// L-shaped polygon
var lShape = Polygon2D.FromPoints(new[]{
    new Vec2(0,0), new Vec2(6,0), new Vec2(6,3),
    new Vec2(3,3), new Vec2(3,6), new Vec2(0,6)
});
```

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

// Generate mesh with hole handling
var mesh = new PrismMesher().Mesh(structure, options);
```

### Multiple Z-Level Constraints
```csharp
var structure = new PrismStructureDefinition(polygon, -5, 5)
    .AddConstraintSegment(new Segment2D(new Vec2(0,0), new Vec2(10,0)), -2.5)
    .AddConstraintSegment(new Segment2D(new Vec2(0,5), new Vec2(10,5)), 2.5)
    .AddConstraintSegment(new Segment2D(new Vec2(5,0), new Vec2(5,5)), 0.0);

// This creates horizontal divisions at specified Z levels
var mesh = new PrismMesher().Mesh(structure, options);
```

### Internal Surfaces (Slabs)
```csharp
// Add horizontal slab at Z = -2.5 with hole
var slabOutline = Polygon2D.FromPoints(new[]{ 
    new Vec2(1,1), new Vec2(9,1), new Vec2(9,5), new Vec2(1,5) 
});
var slabHole = Polygon2D.FromPoints(new[]{ 
    new Vec2(4,2), new Vec2(6,2), new Vec2(6,4), new Vec2(4,4) 
});

structure = structure.AddInternalSurface(slabOutline, -2.5, slabHole);

// The slab creates a horizontal platform with its own hole
var mesh = new PrismMesher().Mesh(structure, options);
```

### Quality Control and Triangle Fallback
```csharp
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(0.5)
    .WithMinCapQuadQuality(0.8)          // High quality threshold
    .WithRejectedCapTriangles(true)      // Output triangles for low-quality quads
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);

// Check generated content
Console.WriteLine($"Generated {mesh.Quads.Count} quads and {mesh.Triangles.Count} triangles");
```

### Auxiliary Geometry
```csharp
// Add points and line segments for additional detail
structure.Geometry
    .AddPoint(new Vec3(5, 2.5, 0))      // Point at center
    .AddPoint(new Vec3(0, 4, 2))        // Elevated point
    .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 2)))  // Horizontal beam
    .AddSegment(new Segment3D(new Vec3(10, 0, -5), new Vec3(10, 5, 5))); // Vertical support

// Auxiliary geometry affects meshing density around those features
```

### Export with Custom Settings
```csharp
var indexed = IndexedMesh.FromMesh(mesh, 1e-9);  // Custom epsilon for vertex merging

// OBJ export with quads and triangles
ObjExporter.Write(indexed, "output.obj");

// glTF export (always triangulated)
GltfExporter.Write(indexed, "output.gltf");

// SVG export for 2D top view
SvgExporter.Write(indexed, "output.svg");

// Access mesh statistics
Console.WriteLine($"Vertices: {indexed.Vertices.Count}");
Console.WriteLine($"Edges: {indexed.Edges.Count}"); 
Console.WriteLine($"Quads: {indexed.Quads.Count}");
Console.WriteLine($"Triangles: {indexed.Triangles.Count}");
```

### Error Handling and Validation
```csharp
try 
{
    var mesh = new PrismMesher().Mesh(structure, options);
    
    // Validate mesh quality
    var adjacency = indexed.BuildAdjacency();
    if (adjacency.NonManifoldEdges.Count > 0)
    {
        Console.WriteLine($"Warning: {adjacency.NonManifoldEdges.Count} non-manifold edges found");
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid geometry: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Meshing failed: {ex.Message}");
}
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

### Création de Géométrie de Base
```csharp
// Rectangle à partir des points de coin
var rect = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(10,0), new Vec2(10,5), new Vec2(0,5) 
});

// Aide-mémoire pour carré
var square = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(5,0), new Vec2(5,5), new Vec2(0,5) 
});

// Polygone en forme de L
var lShape = Polygon2D.FromPoints(new[]{
    new Vec2(0,0), new Vec2(6,0), new Vec2(6,3),
    new Vec2(3,3), new Vec2(3,6), new Vec2(0,6)
});
```

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

// Générer le maillage avec gestion des trous
var mesh = new PrismMesher().Mesh(structure, options);
```

### Contraintes à Plusieurs Niveaux Z
```csharp
var structure = new PrismStructureDefinition(polygon, -5, 5)
    .AddConstraintSegment(new Segment2D(new Vec2(0,0), new Vec2(10,0)), -2.5)
    .AddConstraintSegment(new Segment2D(new Vec2(0,5), new Vec2(10,5)), 2.5)
    .AddConstraintSegment(new Segment2D(new Vec2(5,0), new Vec2(5,5)), 0.0);

// Ceci crée des divisions horizontales aux niveaux Z spécifiés
var mesh = new PrismMesher().Mesh(structure, options);
```

### Surfaces Internes (Dalles)
```csharp
// Ajouter une dalle horizontale à Z = -2.5 avec trou
var slabOutline = Polygon2D.FromPoints(new[]{ 
    new Vec2(1,1), new Vec2(9,1), new Vec2(9,5), new Vec2(1,5) 
});
var slabHole = Polygon2D.FromPoints(new[]{ 
    new Vec2(4,2), new Vec2(6,2), new Vec2(6,4), new Vec2(4,4) 
});

structure = structure.AddInternalSurface(slabOutline, -2.5, slabHole);

// La dalle crée une plateforme horizontale avec son propre trou
var mesh = new PrismMesher().Mesh(structure, options);
```

### Contrôle de Qualité et Triangle de Secours
```csharp
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(0.5)
    .WithMinCapQuadQuality(0.8)          // Seuil de haute qualité
    .WithRejectedCapTriangles(true)      // Sortir des triangles pour les quads de basse qualité
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);

// Vérifier le contenu généré
Console.WriteLine($"Généré {mesh.Quads.Count} quads et {mesh.Triangles.Count} triangles");
```

### Géométrie Auxiliaire
```csharp
// Ajouter des points et des segments de ligne pour plus de détails
structure.Geometry
    .AddPoint(new Vec3(5, 2.5, 0))      // Point au centre
    .AddPoint(new Vec3(0, 4, 2))        // Point en surélévation
    .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 2)))  // Poutre horizontale
    .AddSegment(new Segment3D(new Vec3(10, 0, -5), new Vec3(10, 5, 5))); // Support vertical

// La géométrie auxiliaire affecte la densité de maillage autour de ces caractéristiques
```

### Exportation avec Paramètres Personnalisés
```csharp
var indexed = IndexedMesh.FromMesh(mesh, 1e-9);  // Epsilon personnalisé pour la fusion des sommets

// Export OBJ avec quads et triangles
ObjExporter.Write(indexed, "output.obj");

// Export glTF (toujours triangulé)
GltfExporter.Write(indexed, "output.gltf");

// Export SVG pour vue 2D de dessus
SvgExporter.Write(indexed, "output.svg");

// Accéder aux statistiques du maillage
Console.WriteLine($"Sommets: {indexed.Vertices.Count}");
Console.WriteLine($"Arêtes: {indexed.Edges.Count}"); 
Console.WriteLine($"Quads: {indexed.Quads.Count}");
Console.WriteLine($"Triangles: {indexed.Triangles.Count}");
```

### Gestion des Erreurs et Validation
```csharp
try 
{
    var mesh = new PrismMesher().Mesh(structure, options);
    
    // Valider la qualité du maillage
    var adjacency = indexed.BuildAdjacency();
    if (adjacency.NonManifoldEdges.Count > 0)
    {
        Console.WriteLine($"Avertissement: {adjacency.NonManifoldEdges.Count} arêtes non-manifold détectées");
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Géométrie invalide: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Échec du maillage: {ex.Message}");
}
```

**📖 Documentation Complète** : https://github.com/MabinogiCode/FastGeoMesh  
**📋 Référence API** : https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/api-reference-fr.md  
**⚡ Guide Performance** : https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/performance-guide-fr.md

**Licence** : MIT
