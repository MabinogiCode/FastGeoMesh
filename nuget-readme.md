# FastGeoMesh

**🇬🇧 English** | [🇫🇷 Français](#français)

---

## English

[![CI](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastGeoMesh/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/MabinogiCode/FastGeoMesh/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastGeoMesh)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/FastGeoMesh.svg)](https://www.nuget.org/packages/FastGeoMesh/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Fast, safe, quad-dominant meshing for prismatic volumes from 2D footprints and Z elevations.**

FastGeoMesh is a high-performance .NET 8 library for generating quad-dominant meshes from 2.5D prismatic structures. Perfect for CAD, GIS, and real-time applications requiring sub-millisecond meshing performance and robust error handling.

## ⚡ Performance

**Sub-millisecond meshing** with .NET 8 optimizations and async improvements:
- **Trivial Structures (Async)**: ~311 μs (78% faster than sync!)
- **Simple Structures (Async)**: ~202 μs (42% faster than sync!)
- **Complex Geometry**: ~340 μs, 87 KB
- **Batch Processing (32 items)**: 3.3ms parallel vs 7.4ms sequential (2.2x speedup)
- **Performance Monitoring**: 639ns overhead (negligible)
- **Geometry Operations**: < 10 μs, zero allocations

*Benchmarked on .NET 8, X64 RyuJIT AVX2.*

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
- **📤 Multi-Format Export**: OBJ (quads+triangles), glTF (triangulated), SVG (top view).
- **🔧 Performance Presets**: Fast vs High-Quality configurations.
- **🧵 Thread-Safe**: Immutable structures and stateless meshers.

## 🚀 Quick Start

```csharp
using FastGeoMesh.Core;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// 1. Define geometry
var poly = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) 
});
var structure = new PrismStructureDefinition(poly, -10, 10);

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
    // Use asyncMeshResult.Value
}

// 5. Convert to indexed mesh and export
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
SvgExporter.Write(indexed, "mesh.svg");
```

## 💥 Breaking Changes in v2.0

The introduction of the `Result` pattern is a breaking change designed to improve API safety and eliminate exceptions for validation errors.

- `MesherOptions.CreateBuilder().Build()` now returns a `Result<MesherOptions>`.
- `PrismMesher.Mesh()` and its async variants now return a `Result<Mesh>`.

You must now check the `IsSuccess` property of the result before accessing the `Value`.

## 🏗️ Advanced Features

### Error Handling and Validation

With the new `Result` pattern, you can handle configuration and meshing errors without `try-catch` blocks.

```csharp
// Example of an invalid configuration
var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(-1.0) // This is invalid
    .Build();

if (optionsResult.IsFailure)
{
    // Catches the error without throwing an exception
    Console.WriteLine($"Configuration error: {optionsResult.Error.Description}");
    // Output: Configuration error: Edge length must be between 1E-06 and 1000000.
    return;
}

// The rest of your code won't execute if the configuration is invalid
var meshResult = new PrismMesher().Mesh(structure, optionsResult.Value);
```

### Complex Structures with Holes

```csharp
var outer = Polygon2D.FromPoints(new[] { new Vec2(0,0), new Vec2(10,0), new Vec2(10,6), new Vec2(0,6) });
var hole = Polygon2D.FromPoints(new[] { new Vec2(2,2), new Vec2(4,2), new Vec2(4,4), new Vec2(2,4) });
var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);

var optionsResult = MesherOptions.CreateBuilder()
    .WithHoleRefinement(0.75, 1.0) // Refine near holes
    .Build();

if (optionsResult.IsSuccess)
{
    var meshResult = new PrismMesher().Mesh(structure, optionsResult.Value);
    // ...
}
```

**📖 Full Documentation**: https://github.com/MabinogiCode/FastGeoMesh  
**📋 API Reference**: https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/api-reference.md  
**⚡ Performance Guide**: https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/performance-guide.md

**License**: MIT

---

## Français

**Maillage rapide, sûr et à dominante quadrilatérale pour volumes prismatiques à partir d'empreintes 2D et d'élévations Z.**

FastGeoMesh est une bibliothèque .NET 8 haute performance pour générer des maillages à dominante quadrilatérale à partir de structures prismatiques 2.5D. Parfaite pour les applications CAO, SIG et temps réel nécessitant des performances de maillage inférieures à la milliseconde et une gestion d'erreurs robuste.

## ⚡ Performance

**Maillage sous-milliseconde** avec optimisations .NET 8 et améliorations asynchrones :
- **Structures Triviales (Async)** : ~311 μs (78% plus rapide que sync !)
- **Structures Simples (Async)** : ~202 μs (42% plus rapide que sync !)
- **Géométrie Complexe** : ~340 μs, 87 Ko
- **Traitement par Lots (32 items)** : 3.3ms en parallèle vs 7.4ms en séquentiel (2.2x plus rapide)

*Testé sur .NET 8, X64 RyuJIT AVX2.*

## 🚀 Fonctionnalités

- **🏗️ Mailleur de Prismes** : Génère faces latérales et chapeaux depuis des empreintes 2D.
- **✨ Gestion d'Erreurs Robuste** : Utilise un pattern `Result` pour éliminer les exceptions du flux de travail standard.
- **⚡ Traitement Asynchrone/Parallèle** : Interface asynchrone complète avec une accélération de 2.2x en parallèle.
- **📊 Suivi en Temps Réel** : Statistiques de performance et estimation de la complexité.
- **🎯 Suivi de Progression** : Suivi détaillé des opérations avec ETA.
- **📐 Chemins Rapides Intelligents** : Optimisation pour les rectangles + fallback par tessellation générique.
- **🎯 Contrôle Qualité** : Scoring de la qualité des quads & seuils configurables.
- **📤 Export Multi-Format** : OBJ (quads+triangles), glTF (triangulé), SVG (vue de dessus).
- **🔧 Préréglages de Performance** : Configurations Rapide vs Haute-Qualité.
- **🧵 Thread-Safe** : Structures immutables et mailleurs sans état.

## 🚀 Démarrage Rapide

```csharp
using FastGeoMesh.Core;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// 1. Définir la géométrie
var poly = Polygon2D.FromPoints(new[]{ 
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) 
});
var structure = new PrismStructureDefinition(poly, -10, 10);

// 2. Configurer les options de manière sûre
var optionsResult = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(0.5)
    .WithTargetEdgeLengthZ(1.0)
    .WithRejectedCapTriangles(true)
    .Build();

if (optionsResult.IsFailure)
{
    Console.WriteLine($"Erreur de configuration: {optionsResult.Error.Description}");
    return;
}
var options = optionsResult.Value;

// 3. Générer le maillage de manière sûre
var mesher = new PrismMesher();
var meshResult = mesher.Mesh(structure, options);

if (meshResult.IsFailure)
{
    Console.WriteLine($"Échec du maillage: {meshResult.Error.Description}");
    return;
}
var mesh = meshResult.Value;

// 4. (Optionnel) Utiliser l'API asynchrone pour de meilleures performances
var asyncMesher = (IAsyncMesher)mesher;
var asyncMeshResult = await asyncMesher.MeshAsync(structure, options);
if (asyncMeshResult.IsSuccess)
{
    // Utiliser asyncMeshResult.Value
}

// 5. Convertir en maillage indexé et exporter
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
SvgExporter.Write(indexed, "mesh.svg");
```

## 💥 Changements Cassants (Breaking Changes) dans la v2.0

L'introduction du pattern `Result` est un changement cassant visant à améliorer la sécurité de l'API et à éliminer les exceptions pour les erreurs de validation.

- `MesherOptions.CreateBuilder().Build()` retourne maintenant un `Result<MesherOptions>`.
- `PrismMesher.Mesh()` et ses variantes asynchrones retournent maintenant un `Result<Mesh>`.

Vous devez maintenant vérifier la propriété `IsSuccess` du résultat avant d'accéder à la `Value`.

## 🏗️ Fonctionnalités Avancées

### Gestion des Erreurs et Validation

Avec le nouveau pattern `Result`, vous pouvez gérer les erreurs de configuration et de maillage sans blocs `try-catch`.

```csharp
// Exemple de configuration invalide
var optionsResult = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(-1.0) // Valeur invalide
    .Build();

if (optionsResult.IsFailure)
{
    // L'erreur est interceptée sans lever d'exception
    Console.WriteLine($"Erreur de configuration: {optionsResult.Error.Description}");
    // Sortie: Erreur de configuration: Edge length must be between 1E-06 and 1000000.
    return;
}

// Le reste de votre code ne s'exécutera pas si la configuration est invalide
var meshResult = new PrismMesher().Mesh(structure, optionsResult.Value);
```

**📖 Documentation Complète** : https://github.com/MabinogiCode/FastGeoMesh  
**📋 Référence API** : https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/api-reference-fr.md  
**⚡ Guide Performance** : https://github.com/MabinogiCode/FastGeoMesh/blob/main/docs/performance-guide-fr.md

**Licence** : MIT
