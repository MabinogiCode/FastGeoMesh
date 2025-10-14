# Plan de Refactoring - FastGeoMesh v2.0

## 🎯 Objectifs
- Respecter strictement Clean Architecture
- Éliminer les doublons (GeometryHelper)
- Un objet par fichier
- Pas de méthodes statiques dans classes non-statiques
- Respect IDE0011/IDE0055
- Organisation en dossiers logiques

## 🏗️ Structure proposée

### Domain Layer (`FastGeoMesh.Domain`)
```
Domain/
├── Entities/
│   ├── PrismStructureDefinition.cs
│   ├── ImmutableMesh.cs
│   ├── IndexedMesh.cs
│   ├── CapGeometry.cs
│   └── MeshingGeometry.cs
├── ValueObjects/
│   ├── Vec2.cs
│   ├── Vec3.cs
│   ├── EdgeLength.cs
│   ├── Tolerance.cs
│   ├── Quad.cs
│   ├── Triangle.cs
│   ├── Segment2D.cs
│   ├── Segment3D.cs
│   └── Polygon2D.cs
├── Interfaces/
│   ├── IMesher.cs
│   ├── IAsyncMesher.cs
│   └── ICapMeshingStrategy.cs
├── Services/
│   └── IPerformanceMonitor.cs
├── Results/
│   ├── Result.cs
│   ├── Result.Generic.cs
│   └── Error.cs
├── Configuration/
│   ├── MesherOptions.cs
│   ├── MesherOptionsBuilder.cs
│   └── MesherOptionsBuilderExtensions.cs
└── Helpers/ (Domain-only logic)
    ├── MeshAdjacency.cs
    ├── IndexedMeshAdjacencyHelper.cs
    ├── QuadQualityMetrics.cs
    └── QualityEvaluator.cs
```

### Application Layer (`FastGeoMesh.Application`)
```
Application/
├── Services/
│   └── PrismMesher.cs
├── Strategies/
│   └── DefaultCapMeshingStrategy.cs
├── Helpers/ (Application logic only)
│   ├── CapMeshingHelper.cs
│   ├── SideFaceMeshingHelper.cs
│   ├── MeshStructureHelper.cs
│   ├── QuadQualityHelper.cs
│   └── GeometryCalculationHelper.cs (consolidated)
└── Progress/
    ├── MeshingProgress.cs
    ├── MeshingComplexityEstimate.cs
    └── PerformanceStatistics.cs
```

### Infrastructure Layer (`FastGeoMesh.Infrastructure`)
```
Infrastructure/
├── Exporters/
│   ├── ObjExporter.cs
│   ├── GltfExporter.cs
│   ├── SvgExporter.cs
│   ├── LegacyExporter.cs
│   └── TxtExporter.cs
├── FileOperations/
│   ├── IndexedMeshFileHelper.cs (moved from Domain)
│   └── IndexedMeshExtensions.cs (file ops only)
├── Performance/
│   ├── PerformanceMonitor.cs
│   ├── PerformanceMonitorService.cs
│   ├── TessPool.cs
│   └── MeshPool.cs
├── Utilities/
│   ├── MathUtil.cs
│   ├── OptimizedConstants.cs
│   ├── SpanExtensions.cs
│   ├── AdvancedSpanExtensions.cs
│   ├── ValueTaskExtensions.cs
│   └── GeometryConfig.cs
├── Spatial/
│   ├── SpatialPolygonIndex.cs
│   ├── EdgeMappingHelper.cs
│   └── CellResult.cs
└── Geometry/ (Infrastructure-specific calculations)
    └── InfrastructureGeometryHelper.cs (point-in-polygon batch ops, etc.)
```

## 🔧 Actions prioritaires

### 1. Éliminer la dépendance Application -> Infrastructure
**Problem:** `PrismMesher` utilise `PerformanceMonitor` de Infrastructure
**Solution:** Injection de dépendance via interface dans Domain

### 2. Consolider GeometryHelper
**Problem:** Duplication Application/Infrastructure
**Solution:** 
- Application: `GeometryCalculationHelper` (calculs métier)
- Infrastructure: `InfrastructureGeometryHelper` (optimisations techniques)

### 3. Déplacer file operations
**Problem:** `IndexedMeshFileHelper` dans Domain
**Solution:** Déplacer vers Infrastructure/FileOperations/

### 4. Séparer les responsabilités
- Domain: Logique métier pure
- Application: Orchestration, algorithmes
- Infrastructure: I/O, performance, optimisations

### 5. Organizing par responsabilité
- Entities vs ValueObjects
- Services vs Helpers
- Configuration séparée

## 🚫 Violations à corriger

### Clean Architecture
1. ✅ Domain ne dépend de rien
2. ✅ Application dépend uniquement de Domain  
3. ✅ Infrastructure dépend de Domain et Application
4. ❌ **ACTUEL:** Application -> Infrastructure (PerformanceMonitor)
5. ❌ **ACTUEL:** Domain -> System.IO (file operations)

### Code Guidelines
1. ❌ Méthodes statiques dans classes non-statiques
2. ❌ Plusieurs objets dans certains fichiers
3. ❌ Manque d'organisation en dossiers

## 🎯 Résultat attendu
- Clean Architecture stricte
- Testabilité maximale
- Séparation claire des responsabilités
- Respect total des guidelines de code
- Organisation logique et maintenable
