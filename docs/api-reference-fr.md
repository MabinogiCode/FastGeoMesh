# Référence API FastGeoMesh

Documentation API complète pour les classes, interfaces et méthodes de la bibliothèque FastGeoMesh.

## Table des Matières

1. [Espaces de Noms Principaux](#espaces-de-noms-principaux)
2. [Types Géométriques](#types-géométriques)
3. [API de Maillage](#api-de-maillage)
4. [Définitions de Structures](#définitions-de-structures)
5. [API d'Export](#api-dexport)
6. [Types Utilitaires](#types-utilitaires)
7. [Surveillance Performance](#surveillance-performance)

## Espaces de Noms Principaux

### FastGeoMesh.Geometry
Primitives géométriques fondamentales et opérations.

### FastGeoMesh.Meshing
Algorithmes de maillage principaux et options.

### FastGeoMesh.Structures
Définitions de structures de haut niveau pour géométries prismatiques.

### FastGeoMesh.Meshing.Exporters
Fonctionnalité d'export pour divers formats de fichiers.

### FastGeoMesh.Utils
Classes utilitaires pour performance et fonctions d'aide.

## Types Géométriques

### Structure Vec2

Vecteur 2D haute performance pour opérations dans le plan XY.

```csharp
public readonly struct Vec2 : IEquatable<Vec2>
{
    // Propriétés
    public double X { get; }
    public double Y { get; }
    
    // Constructeur
    public Vec2(double x, double y)
    
    // Constantes
    public static readonly Vec2 Zero;
    public static readonly Vec2 UnitX;
    public static readonly Vec2 UnitY;
    
    // Opérateurs
    public static Vec2 operator +(Vec2 a, Vec2 b);
    public static Vec2 operator -(Vec2 a, Vec2 b);
    public static Vec2 operator *(Vec2 a, double k);
    public static Vec2 operator *(double k, Vec2 a);
    
    // Méthodes
    public double Dot(in Vec2 b);
    public double Cross(in Vec2 b);
    public double Length();
    public double LengthSquared();
    public Vec2 Normalize();
    
    // Opérations par lots
    public static double AccumulateDot(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b);
    public static void Add(ReadOnlySpan<Vec2> a, ReadOnlySpan<Vec2> b, Span<Vec2> dest);
}
```

**Exemples d'Usage :**
```csharp
// Créer vecteurs
var v1 = new Vec2(3.0, 4.0);
var v2 = Vec2.UnitX;

// Opérations vectorielles
var somme = v1 + v2;
var echelle = v1 * 2.0;
var longueur = v1.Length();  // 5.0
var produitScalaire = v1.Dot(v2);      // 3.0

// Normalisation
var normalise = v1.Normalize();  // (0.6, 0.8)
```

### Structure Vec3

Vecteur 3D haute performance pour opérations spatiales.

```csharp
public readonly struct Vec3 : IEquatable<Vec3>
{
    // Propriétés
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    
    // Constructeur
    public Vec3(double x, double y, double z)
    
    // Constantes
    public static readonly Vec3 Zero;
    public static readonly Vec3 UnitX;
    public static readonly Vec3 UnitY;
    public static readonly Vec3 UnitZ;
    
    // Opérateurs
    public static Vec3 operator +(Vec3 a, Vec3 b);
    public static Vec3 operator -(Vec3 a, Vec3 b);
    public static Vec3 operator *(Vec3 a, double k);
    
    // Méthodes
    public double Dot(in Vec3 b);
    public Vec3 Cross(in Vec3 b);
    public double Length();
    public double LengthSquared();
    public Vec3 Normalize();
    
    // Opérations par lots
    public static double AccumulateDot(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b);
    public static void Add(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest);
    public static void Cross(ReadOnlySpan<Vec3> a, ReadOnlySpan<Vec3> b, Span<Vec3> dest);
}
```

### Classe Polygon2D

Représente un polygone simple dans le plan XY avec ordre de sommets CCW.

```csharp
public sealed class Polygon2D
{
    // Propriétés
    public IReadOnlyList<Vec2> Vertices { get; }
    public int Count { get; }
    
    // Constructeur
    public Polygon2D(IEnumerable<Vec2> vertices)
    
    // Méthode factory
    public static Polygon2D FromPoints(IEnumerable<Vec2> vertices)
    
    // Méthodes
    public double Perimeter();
    public bool IsRectangleAxisAligned(out Vec2 min, out Vec2 max, double eps = 1e-9);
    
    // Méthodes statiques
    public static double SignedArea(IReadOnlyList<Vec2> vertices);
    public static bool Validate(IReadOnlyList<Vec2> vertices, out string? error, double eps = 1e-9);
}
```

**Exemples d'Usage :**
```csharp
// Créer rectangle
var rect = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(10, 0), 
    new Vec2(10, 5), new Vec2(0, 5)
});

// Vérifier si rectangle aligné sur axes
if (rect.IsRectangleAxisAligned(out Vec2 min, out Vec2 max))
{
    Console.WriteLine($"Rectangle : {min} à {max}");
}

// Valider polygone
if (!Polygon2D.Validate(sommets, out string? erreur))
{
    throw new ArgumentException($"Polygone invalide : {erreur}");
}
```

### Record Segment2D

Segment de ligne 2D défini par deux points finaux.

```csharp
public readonly record struct Segment2D(Vec2 A, Vec2 B)
{
    public double Length();
}
```

### Record Segment3D

Segment de ligne 3D défini par deux points finaux.

```csharp
public readonly record struct Segment3D(Vec3 A, Vec3 B)
{
    public double Length();
    public Vec3 Direction();
    public Vec3 Midpoint();
}
```

## API de Maillage

### Interface IMesher<T>

Interface de maillage principale avec support async.

```csharp
public interface IMesher<in TStructure> where TStructure : notnull
{
    Mesh Mesh(TStructure input, MesherOptions options);
    ValueTask<Mesh> MeshAsync(TStructure input, MesherOptions options, 
                             CancellationToken cancellationToken = default);
}
```

### Classe PrismMesher

Implémentation principale du mailleur pour structures prismatiques.

```csharp
public sealed class PrismMesher : IMesher<PrismStructureDefinition>
{
    // Constructeurs
    public PrismMesher();
    public PrismMesher(ICapMeshingStrategy capStrategy);
    
    // Méthodes
    public Mesh Mesh(PrismStructureDefinition structure, MesherOptions options);
    public ValueTask<Mesh> MeshAsync(PrismStructureDefinition structure, 
                                    MesherOptions options, 
                                    CancellationToken cancellationToken = default);
}
```

**Exemples d'Usage :**
```csharp
// Usage basique
var mesher = new PrismMesher();
var mesh = mesher.Mesh(structure, options);

// Usage async
var mesh = await mesher.MeshAsync(structure, options, cancellationToken);

// Stratégie chapeau personnalisée
var mesheurPersonnalise = new PrismMesher(new StrategieChapeauPersonnalisee());
```

### Classe MesherOptions

Options de configuration pour opérations de maillage.

```csharp
public sealed class MesherOptions
{
    // Propriétés principales
    public double TargetEdgeLengthXY { get; set; } = 1.0;
    public double TargetEdgeLengthZ { get; set; } = 1.0;
    public bool GenerateBottomCap { get; set; } = true;
    public bool GenerateTopCap { get; set; } = true;
    public double Epsilon { get; set; } = 1e-9;
    
    // Contrôle qualité
    public double MinCapQuadQuality { get; set; } = 0.5;
    public bool OutputRejectedCapTriangles { get; set; } = false;
    
    // Options raffinement
    public double? TargetEdgeLengthXYNearHoles { get; set; }
    public double HoleRefineBand { get; set; } = 2.0;
    public double? TargetEdgeLengthXYNearSegments { get; set; }
    public double SegmentRefineBand { get; set; } = 1.0;
    
    // Méthode factory
    public static MesherOptionsBuilder CreateBuilder();
    
    // Validation
    public void Validate();
}
```

### Classe MesherOptionsBuilder

Constructeur fluide pour MesherOptions avec préréglages.

```csharp
public sealed class MesherOptionsBuilder
{
    // Longueurs cibles
    public MesherOptionsBuilder WithTargetEdgeLengthXY(double length);
    public MesherOptionsBuilder WithTargetEdgeLengthZ(double length);
    
    // Options chapeaux
    public MesherOptionsBuilder WithCaps(bool bottom = true, bool top = true);
    
    // Raffinement
    public MesherOptionsBuilder WithHoleRefinement(double targetLength, double band);
    public MesherOptionsBuilder WithSegmentRefinement(double targetLength, double band);
    
    // Qualité
    public MesherOptionsBuilder WithMinCapQuadQuality(double quality);
    public MesherOptionsBuilder WithRejectedCapTriangles(bool output = true);
    
    // Préréglages
    public MesherOptionsBuilder WithFastPreset();
    public MesherOptionsBuilder WithHighQualityPreset();
    
    // Autres options
    public MesherOptionsBuilder WithEpsilon(double epsilon);
    
    // Construction
    public MesherOptions Build();
}
```

**Configurations des Préréglages :**

| Préréglage | TargetEdgeLengthXY | TargetEdgeLengthZ | MinCapQuadQuality | OutputRejectedCapTriangles |
|------------|-------------------|-------------------|-------------------|---------------------------|
| Rapide | 2.0 | 2.0 | 0.3 | false |
| Haute-Qualité | 0.5 | 0.5 | 0.7 | true |

**Exemples d'Usage :**
```csharp
// Préréglage rapide
var rapide = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

// Configuration personnalisée
var personnalise = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(0.5)
    .WithCaps(bottom: true, top: false)
    .WithHoleRefinement(0.5, 1.0)
    .WithMinCapQuadQuality(0.6)
    .WithRejectedCapTriangles(true)
    .Build();
```

### Classe Mesh

Conteneur de maillage mutable pour quads, triangles, points et segments.

```csharp
public sealed class Mesh : IDisposable
{
    // Constructeurs
    public Mesh();
    public Mesh(int initialQuadCapacity, int initialTriangleCapacity, 
               int initialPointCapacity, int initialSegmentCapacity);
    
    // Propriétés
    public int QuadCount { get; }
    public int TriangleCount { get; }
    public ReadOnlyCollection<Quad> Quads { get; }
    public ReadOnlyCollection<Triangle> Triangles { get; }
    public ReadOnlyCollection<Vec3> Points { get; }
    public ReadOnlyCollection<Segment3D> InternalSegments { get; }
    
    // Méthodes
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

### Structure Quad

Primitive quadrilatérale avec score de qualité optionnel.

```csharp
public readonly struct Quad : IEquatable<Quad>
{
    // Propriétés
    public Vec3 V0 { get; }
    public Vec3 V1 { get; }
    public Vec3 V2 { get; }
    public Vec3 V3 { get; }
    public double? QualityScore { get; }
    
    // Constructeurs
    public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3);
    public Quad(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3, double? qualityScore);
}
```

### Structure Triangle

Primitive triangulaire avec score de qualité optionnel.

```csharp
public readonly struct Triangle : IEquatable<Triangle>
{
    // Propriétés
    public Vec3 V0 { get; }
    public Vec3 V1 { get; }
    public Vec3 V2 { get; }
    public double? QualityScore { get; }
    
    // Constructeurs
    public Triangle(Vec3 v0, Vec3 v1, Vec3 v2);
    public Triangle(Vec3 v0, Vec3 v1, Vec3 v2, double? qualityScore);
}
```

### Classe IndexedMesh

Représentation indexée immutable d'un maillage.

```csharp
public sealed class IndexedMesh
{
    // Propriétés
    public ReadOnlyCollection<Vec3> Vertices { get; }
    public ReadOnlyCollection<(int a, int b)> Edges { get; }
    public ReadOnlyCollection<(int v0, int v1, int v2, int v3)> Quads { get; }
    public ReadOnlyCollection<(int v0, int v1, int v2)> Triangles { get; }
    
    // Propriétés de compte (optimisées)
    public int VertexCount { get; }
    public int EdgeCount { get; }
    public int QuadCount { get; }
    public int TriangleCount { get; }
    
    // Méthode factory
    public static IndexedMesh FromMesh(Mesh mesh, double epsilon = 1e-9);
    
    // Support format legacy
    public static IndexedMesh ReadCustomTxt(string path);
    public void WriteCustomTxt(string path);
    
    // Analyse adjacence
    public MeshAdjacency BuildAdjacency();
}
```

## Définitions de Structures

### Classe PrismStructureDefinition

Définition immutable d'une structure prismatique.

```csharp
public sealed class PrismStructureDefinition
{
    // Propriétés
    public Polygon2D Footprint { get; }
    public double BaseElevation { get; }
    public double TopElevation { get; }
    public IReadOnlyList<Polygon2D> Holes { get; }
    public IReadOnlyList<(Segment2D segment, double z)> ConstraintSegments { get; }
    public IReadOnlyList<InternalSurfaceDefinition> InternalSurfaces { get; }
    public MeshingGeometry Geometry { get; }
    
    // Constructeur
    public PrismStructureDefinition(Polygon2D footprint, double baseElevation, double topElevation);
    
    // Méthodes constructeur (retournent nouvelles instances)
    public PrismStructureDefinition AddHole(Polygon2D hole);
    public PrismStructureDefinition AddConstraintSegment(Segment2D segment, double z);
    public PrismStructureDefinition AddInternalSurface(Polygon2D outer, double z, params Polygon2D[] holes);
}
```

**Exemples d'Usage :**
```csharp
// Structure basique
var structure = new PrismStructureDefinition(empreinte, -10, 10);

// Ajouter caractéristiques (immutable - retourne nouvelles instances)
structure = structure
    .AddHole(polygoneTrou)
    .AddConstraintSegment(new Segment2D(p1, p2), 5.0)
    .AddInternalSurface(contourDalle, 2.5, trouDalle);

// Ajouter géométrie auxiliaire
structure.Geometry
    .AddPoint(new Vec3(5, 5, 7))
    .AddSegment(new Segment3D(debut, fin));
```

### Classe InternalSurfaceDefinition

Définition de surfaces internes horizontales (dalles) avec trous.

```csharp
public sealed class InternalSurfaceDefinition
{
    // Propriétés
    public Polygon2D Outer { get; }
    public double Elevation { get; }
    public IReadOnlyList<Polygon2D> Holes { get; }
    
    // Constructeur
    public InternalSurfaceDefinition(Polygon2D outer, double elevation, 
                                   IEnumerable<Polygon2D>? holes = null);
}
```

### Classe MeshingGeometry

Conteneur pour géométrie auxiliaire à préserver dans le maillage.

```csharp
public sealed class MeshingGeometry
{
    // Propriétés
    public ReadOnlyCollection<Vec3> Points { get; }
    public ReadOnlyCollection<Segment3D> Segments { get; }
    
    // Méthodes (interface fluide)
    public MeshingGeometry AddPoint(Vec3 point);
    public MeshingGeometry AddSegment(Segment3D segment);
    public MeshingGeometry AddPoints(IEnumerable<Vec3> points);
    public MeshingGeometry AddSegments(IEnumerable<Segment3D> segments);
}
```

## API d'Export

### Classe ObjExporter

Export de maillages vers format Wavefront OBJ.

```csharp
public static class ObjExporter
{
    public static void Write(IndexedMesh mesh, string filePath);
}
```

**Format de Sortie :**
- Préserve quads et triangles
- Quads : `f v1 v2 v3 v4 v5` (5 sommets, dernier répète premier)
- Triangles : `f v1 v2 v3 v4` (4 sommets, dernier répète premier)

### Classe GltfExporter

Export de maillages vers format glTF 2.0.

```csharp
public static class GltfExporter
{
    public static void Write(IndexedMesh mesh, string filePath);
}
```

**Fonctionnalités :**
- glTF autonome avec données binaires intégrées
- Toute géométrie triangulée
- Compatible avec visualiseurs web et moteurs de jeu

### Classe SvgExporter

Export de topologie de maillage vers format SVG (vue de dessus).

```csharp
public static class SvgExporter
{
    public static void Write(IndexedMesh mesh, string filePath);
}
```

**Fonctionnalités :**
- Projection 2D vue de dessus des arêtes de maillage
- Utile pour déboguer topologie de maillage
- Visualisable dans navigateurs web

## Types Utilitaires

### Classe MeshAdjacency

Information d'adjacence pour analyse de maillage.

```csharp
public sealed class MeshAdjacency
{
    // Propriétés
    public int QuadCount { get; }
    public IReadOnlyList<int[]> Neighbors { get; }  // 4 voisins par quad (-1 si aucun)
    public ReadOnlyCollection<(int a, int b)> BoundaryEdges { get; }
    public ReadOnlyCollection<(int a, int b)> NonManifoldEdges { get; }
    
    // Méthode factory
    public static MeshAdjacency Build(IndexedMesh mesh);
}
```

## Surveillance Performance

### Classe PerformanceMonitor

Utilitaires de surveillance performance statiques.

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

**Exemple d'Usage :**
```csharp
// Réinitialiser compteurs
PerformanceMonitor.Counters.Reset();

// Effectuer opérations...
var mesh = mesher.Mesh(structure, options);

// Obtenir statistiques
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Opérations : {stats.MeshingOperations}");
Console.WriteLine($"Quads moy/op : {stats.AverageQuadsPerOperation:F1}");
Console.WriteLine($"Ratio succès pool : {stats.PoolHitRatio:P1}");
```

## Compatibilité Types

Tous les types géométriques implémentent les interfaces appropriées :
- `IEquatable<T>` pour égalité de valeur
- Implémentations `GetHashCode()` appropriées
- `ToString()` pour débogage

## Sécurité Thread

- **Définitions Structures** : Immutables, complètement thread-safe
- **Mailleurs** : Sans état, thread-safe pour usage concurrent
- **Objets Mesh** : Pas thread-safe (utiliser séparément par thread)
- **IndexedMesh** : Immutable après création, thread-safe pour lecture

Ceci complète la référence API complète. Pour exemples d'usage et patrons, voir le [Guide d'Usage](usage-guide-fr.md).
