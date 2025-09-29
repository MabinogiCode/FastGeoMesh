# FastGeoMesh Performance Benchmarks

Ce projet contient des benchmarks complets pour mesurer l'impact des optimisations .NET 8 implémentées dans FastGeoMesh.

## Vue d'ensemble

Les benchmarks sont organisés par catégories pour tester différents aspects des optimisations :

### 🔧 **Géométrie** (Geometry)
- **Vec2OperationsBenchmark** : Opérations vectorielles 2D avec AggressiveInlining
- **Vec3OperationsBenchmark** : Opérations vectorielles 3D optimisées  
- **GeometryHelperBenchmark** : Algorithmes géométriques avec ReadOnlySpan<T>

### 🔬 **Meshing** (Meshing)
- **PrismMeshingBenchmark** : Génération de maillages avec différentes configurations
- **MeshingOptionsBenchmark** : Création et validation des options de maillage
- **AsyncMeshingBenchmark** : Comparaison Task vs ValueTask pour les opérations asynchrones

### ⚡ **Utilitaires** (Utils)
- **OptimizedConstantsBenchmark** : FrozenCollections vs collections classiques
- **ObjectPoolingBenchmark** : Réutilisation d'objets avec Microsoft.Extensions.ObjectPool
- **FrozenCollectionsBenchmark** : Performance des nouvelles collections .NET 8

## 🚀 **Exécution des benchmarks**

### Prérequis
- .NET 8 SDK
- Mode Release pour des résultats précis

### Commandes

```bash
# Compiler en mode Release
dotnet build -c Release

# Exécuter tous les benchmarks
dotnet run -c Release -- --all

# Exécuter par catégorie
dotnet run -c Release -- --geometry
dotnet run -c Release -- --meshing
dotnet run -c Release -- --utils
dotnet run -c Release -- --collections
dotnet run -c Release -- --async
```

### Benchmarks individuels

```bash
# Benchmarks spécifiques avec BenchmarkDotNet
dotnet run -c Release -- --filter "*Vec2*"
dotnet run -c Release -- --filter "*Frozen*"
dotnet run -c Release -- --filter "*Async*"
```

## 📊 **Optimisations testées**

### .NET 8 Spécifiques
- **FrozenDictionary/FrozenSet** : Collections immutables haute performance
- **ValueTask<T>** : Réduction des allocations pour les opérations async
- **AggressiveInlining** : Optimisations du compilateur JIT
- **ReadOnlySpan<T>** : Accès mémoire zero-copy
- **TieredPGO** : Profile-Guided Optimization

### Patterns de performance
- **Object Pooling** : Réutilisation d'objets avec pools
- **Builder Pattern** : Impact sur les allocations
- **Span/Memory<T>** : Algorithmes sans allocation
- **Validation optimisée** : Vérifications avec constantes pré-calculées

## 📈 **Métriques collectées**

Les benchmarks mesurent :
- **Temps d'exécution** (Mean, Median, Min, Max)
- **Allocations mémoire** (MemoryDiagnoser)
- **Débit** (ops/sec)
- **Collections garbage** (Gen0, Gen1, Gen2)

## 🎯 **Résultats attendus**

### Améliorations ciblées
- **Vec2/Vec3** : +15-30% performance avec inlining
- **FrozenCollections** : +40% vitesse de lookup
- **ValueTask** : -20% allocations async
- **GeometryHelper** : +25% avec ReadOnlySpan
- **Object Pooling** : -60% allocations

### Métriques de référence
- Temps de maillage pour structures simples : < 1ms
- Temps de maillage pour structures complexes : < 50ms
- Allocations par opération vectorielle : 0 bytes
- Taux de réussite du pooling : > 90%

## 🔧 **Configuration**

### BenchmarkDotNet
```csharp
[MemoryDiagnoser]           // Mesure des allocations
[SimpleJob]                 // Configuration standard
[MinColumn, MaxColumn, MeanColumn, MedianColumn]  // Colonnes affichées
```

### Optimisations projet
```xml
<TieredCompilation>true</TieredCompilation>
<TieredPGO>true</TieredPGO>
<Optimize>true</Optimize>
<Deterministic>true</Deterministic>
```

## 📝 **Structure des fichiers**

```
FastGeoMesh.Benchmarks/
├── Program.cs                    # Point d'entrée principal
├── Geometry/
│   ├── Vec2OperationsBenchmark.cs
│   ├── Vec3OperationsBenchmark.cs
│   └── GeometryHelperBenchmark.cs
├── Meshing/
│   ├── PrismMeshingBenchmark.cs
│   ├── MeshingOptionsBenchmark.cs
│   ├── AsyncMeshingBenchmark.cs
│   ├── TestAsyncMesher.cs       # Helper pour tests async
│   └── TestTaskMesher.cs        # Helper pour comparaisons
└── Utils/
    ├── OptimizedConstantsBenchmark.cs
    ├── ObjectPoolingBenchmark.cs
    └── FrozenCollectionsBenchmark.cs
```

## 🎛️ **Options avancées**

### Profilage mémoire détaillé
```bash
dotnet run -c Release -- --memory-randomization --disasm-depth 3
```

### Export des résultats
```bash
dotnet run -c Release -- --exporters json,html,csv
```

### Comparaison avec versions antérieures
```bash
dotnet run -c Release -- --baseline "*.NonOptimized*"
```

## 🚨 **Notes importantes**

1. **Environnement** : Exécuter sur machine dédiée pour résultats précis
2. **Variance** : Plusieurs runs pour confirmer les tendances
3. **Baseline** : Méthodes `*_NonOptimized` servent de référence
4. **Configuration** : Release mode obligatoire pour mesures valides

## 📚 **Documentation de référence**

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [.NET 8 Performance Improvements](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/)
- [FrozenCollections Guide](https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen)
- [Span<T> Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/)

---

*Benchmarks créés pour valider les optimisations .NET 8 de FastGeoMesh - Bibliothèque de maillage géométrique haute performance*
