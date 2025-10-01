# Guide Performance FastGeoMesh

Ce guide fournit des informations complètes sur l'optimisation des performances de FastGeoMesh pour différents cas d'usage, des applications temps réel aux flux de travail CAO haute précision.

## Table des Matières

1. [Aperçu Performance](#aperçu-performance)
2. [Résultats Benchmarks](#résultats-benchmarks)
3. [Stratégies Configuration](#stratégies-configuration)
4. [Optimisation Mémoire](#optimisation-mémoire)
5. [Patrons Mise à l'Échelle](#patrons-mise-à-léchelle)
6. [Profilage et Surveillance](#profilage-et-surveillance)
7. [Bonnes Pratiques](#bonnes-pratiques)

## Aperçu Performance

FastGeoMesh est conçu pour des **performances de maillage sous-milliseconde** sur du matériel moderne. La bibliothèque y parvient grâce à :

- **Optimisations .NET 8** : Inlining agressif, opérations span, potentiel SIMD
- **Chemins rapides intelligents** : Détection rectangle pour maillage O(1) vs tessellation O(n log n)
- **Efficacité mémoire** : Pool d'objets, allocations minimales, structures de données optimisées
- **Optimisation algorithmique** : Indexation spatiale, tessellation optimisée, opérations par lots

### Objectifs Performance

| Cas d'Usage | Performance Cible | Usage Mémoire | Niveau Qualité |
|-------------|-------------------|---------------|----------------|
| Visualisation Temps Réel | < 500 μs | < 100 Ko | Moyen |
| CAO Interactive | < 2 ms | < 1 Mo | Élevé |
| Traitement Lot | < 10 ms | < 10 Mo | Très Élevé |
| Génération Arrière-plan | Quelconque | Minimiser | Variable |

## Résultats Benchmarks

### Configuration Matérielle
- **CPU** : Intel i7-12700K (3,6 GHz base, 5,0 GHz boost)
- **RAM** : 32 Go DDR4-3200
- **Runtime** : .NET 8.0.20 sur Windows 11
- **JIT** : X64 RyuJIT avec support AVX2

### Micro-benchmarks

#### Rectangle Simple (20×5m, Z=-10 à 10)
```
| Préréglage    | Temps   | Mémoire | Quads | Triangles |
|---------------|---------|---------|-------|-----------|
| Rapide        | 305 μs  | 87 Ko   | 162   | 0         |
| Haute-Qualité | 1,2 ms  | 340 Ko  | 648   | 0         |
| Personnalisé  | 3,8 ms  | 1,1 Mo  | 2 047 | 0         |
```

#### Forme L Complexe
```
| Préréglage    | Temps   | Mémoire | Quads | Triangles |
|---------------|---------|---------|-------|-----------|
| Rapide        | 340 μs  | 87 Ko   | 178   | 0         |
| Haute-Qualité | 1,4 ms  | 420 Ko  | 712   | 0         |
```

#### Rectangle avec Trou Central
```
| Préréglage    | Temps   | Mémoire | Quads | Triangles |
|---------------|---------|---------|-------|-----------|
| Rapide        | 907 μs  | 1,3 Mo  | 485   | 24        |
| Haute-Qualité | 4,2 ms  | 8,7 Mo  | 1 947 | 156       |
```

### Caractéristiques Mise à l'Échelle

#### Impact Longueur Arête (rectangle 20×5m)
```
| LongueurArêteXY | Temps   | Mémoire | Sommets | Quads |
|-----------------|---------|---------|---------|-------|
| 5,0             | 285 μs  | 45 Ko   | 45      | 42    |
| 2,0             | 305 μs  | 87 Ko   | 117     | 162   |
| 1,0             | 420 μs  | 180 Ko  | 273     | 432   |
| 0,5             | 1,2 ms  | 340 Ko  | 819     | 1 248 |
| 0,2             | 7,8 ms  | 2,1 Mo  | 4 563   | 7 632 |
```

#### Mise à l'Échelle Complexité
```
| Sommets Polygone | Temps Rapide | Mémoire | Temps Haute-Qualité |
|------------------|--------------|---------|---------------------|
| 4 (Rectangle)    | 305 μs       | 87 Ko   | 1,2 ms             |
| 8 (Octogone)     | 380 μs       | 120 Ko  | 1,6 ms             |
| 16 (Complexe)    | 520 μs       | 210 Ko  | 2,8 ms             |
| 32 (Très Complexe)| 890 μs      | 450 Ko  | 6,1 ms             |
```

## Stratégies Configuration

### Applications Temps Réel (< 500 μs)

Optimiser pour latence minimale avec qualité acceptable :

```csharp
var tempsReel = MesherOptions.CreateBuilder()
    .WithFastPreset()                    // Configuration rapide de base
    .WithTargetEdgeLengthXY(3.0)        // Plus grand que défaut (2,0)
    .WithTargetEdgeLengthZ(3.0)         // Assortir XY pour taille uniforme
    .WithCaps(bottom: false, top: true) // Seulement chapeau supérieur si nécessaire
    .WithRejectedCapTriangles(false)    // Ignorer génération triangles
    .WithEpsilon(1e-6)                  // Epsilon plus grand pour dédup plus rapide
    .Build();

// Attendu : ~200-300 μs, ~50-80 Ko mémoire
```

### CAO Interactive (< 2 ms)

Équilibrer qualité et performance pour interaction utilisateur :

```csharp
var interactif = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)        // Bon niveau de détail
    .WithTargetEdgeLengthZ(1.0)
    .WithCaps(bottom: true, top: true)  // Chapeaux complets
    .WithMinCapQuadQuality(0.5)         // Qualité équilibrée
    .WithRejectedCapTriangles(true)     // Inclure triangles
    .WithHoleRefinement(0.7, 1.5)       // Raffinement trous modéré
    .Build();

// Attendu : ~800-1 500 μs, ~200-500 Ko mémoire
```

### CAO Haute Précision (< 10 ms)

Maximiser qualité pour sortie finale :

```csharp
var precision = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()            // Config haute qualité de base
    .WithTargetEdgeLengthXY(0.3)        // Détail fin
    .WithTargetEdgeLengthZ(0.3)
    .WithMinCapQuadQuality(0.8)         // Seuil qualité élevé
    .WithHoleRefinement(0.2, 1.0)       // Raffinement trous agressif
    .WithSegmentRefinement(0.2, 0.8)    // Raffinement segments agressif
    .Build();

// Attendu : ~3-8 ms, ~1-5 Mo mémoire
```

### Traitement Lot

Optimiser pour débit sur latence individuelle :

```csharp
var lot = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.5)        // Résolution équilibrée
    .WithTargetEdgeLengthZ(1.5)
    .WithCaps(bottom: true, top: true)
    .WithMinCapQuadQuality(0.6)
    .WithRejectedCapTriangles(true)
    .WithEpsilon(1e-8)                  // Déduplication précise
    .Build();

// Traiter plusieurs structures efficacement
var resultats = new List<IndexedMesh>();
var mesher = new PrismMesher();

foreach (var structure in structures)
{
    var mesh = mesher.Mesh(structure, lot);
    var indexed = IndexedMesh.FromMesh(mesh, lot.Epsilon);
    resultats.Add(indexed);
}
```

## Optimisation Mémoire

### Comprendre Usage Mémoire

L'usage mémoire FastGeoMesh consiste en :

1. **Traitement Entrée** : ~10% du total
2. **Données Tessellation** : ~60% du total (temporaire)
3. **Maillage Sortie** : ~30% du total (persistant)

### Stratégies Réduction Mémoire

#### 1. Augmenter Longueurs Arête
```csharp
// Grandes longueurs arête = moins éléments = moins mémoire
var baisseMemoire = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(4.0)        // 4x plus grand = ~16x moins quads
    .WithTargetEdgeLengthZ(4.0)
    .Build();
```

#### 2. Désactiver Fonctionnalités Inutiles
```csharp
var minimal = MesherOptions.CreateBuilder()
    .WithCaps(bottom: false, top: false)    // Ignorer chapeaux si pas nécessaire
    .WithRejectedCapTriangles(false)        // Ignorer triangles
    .Build();
```

#### 3. Optimiser Précision Coordonnées
```csharp
// Epsilon plus grand = plus déduplication coordonnées = moins sommets
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh, epsilon: 1e-6);  // vs défaut 1e-9
```

#### 4. Pré-dimensionner Collections
```csharp
// Estimer taille maillage pour éviter réallocations
var quadsEstimes = EstimerNombreQuads(structure, options);
var mesh = new Mesh(
    initialQuadCapacity: quadsEstimes,
    initialTriangleCapacity: quadsEstimes / 10,
    initialPointCapacity: 0,
    initialSegmentCapacity: 0);
```

### Patrons Usage Mémoire

#### Mise à l'Échelle Linéaire avec Densité Maillage
```
LongueurArête = 2,0 : ~87 Ko
LongueurArête = 1,0 : ~340 Ko  (4x densité → 4x mémoire)
LongueurArête = 0,5 : ~1,3 Mo  (16x densité → 15x mémoire)
```

#### Compromis Mémoire vs Qualité
```csharp
// Efficace mémoire (préférer quads, ignorer triangles)
var efficace = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.2)         // Accepter qualité moindre
    .WithRejectedCapTriangles(false)    // Pas de triangles
    .Build();

// Axé qualité (plus triangles, mémoire plus élevée)
var qualite = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.8)         // Exigence qualité élevée
    .WithRejectedCapTriangles(true)     // Inclure triangles
    .Build();
```

## Patrons Mise à l'Échelle

### Traitement Asynchrone

Tirer parti d'async/await pour opérations non-bloquantes :

```csharp
public async Task<List<IndexedMesh>> TraiterStructuresAsync(
    IEnumerable<PrismStructureDefinition> structures,
    MesherOptions options,
    CancellationToken cancellationToken = default)
{
    var mesher = new PrismMesher();
    var taches = structures.Select(async structure =>
    {
        var mesh = await mesher.MeshAsync(structure, options, cancellationToken);
        return IndexedMesh.FromMesh(mesh, options.Epsilon);
    });
    
    return (await Task.WhenAll(taches)).ToList();
}
```

### Traitement Parallèle

Utiliser Parallel.ForEach pour opérations lot CPU-intensives :

```csharp
public List<IndexedMesh> TraiterStructuresParallele(
    IList<PrismStructureDefinition> structures,
    MesherOptions options)
{
    var resultats = new IndexedMesh[structures.Count];
    
    Parallel.For(0, structures.Count, i =>
    {
        var mesher = new PrismMesher();  // Thread-safe, créer par thread
        var mesh = mesher.Mesh(structures[i], options);
        resultats[i] = IndexedMesh.FromMesh(mesh, options.Epsilon);
    });
    
    return resultats.ToList();
}
```

### Niveau de Détail Progressif

Générer plusieurs LODs pour différents cas d'usage :

```csharp
public class EnsembleMaillagesLod
{
    public IndexedMesh DetailEleve { get; set; }
    public IndexedMesh DetailMoyen { get; set; }
    public IndexedMesh DetailFaible { get; set; }
}

public EnsembleMaillagesLod GenererEnsembleLod(PrismStructureDefinition structure)
{
    var mesher = new PrismMesher();
    
    // Détail élevé (travail précision)
    var optionsElevees = MesherOptions.CreateBuilder()
        .WithTargetEdgeLengthXY(0.5)
        .WithHighQualityPreset()
        .Build();
    
    // Détail moyen (usage général)
    var optionsMoyennes = MesherOptions.CreateBuilder()
        .WithTargetEdgeLengthXY(1.0)
        .WithMinCapQuadQuality(0.5)
        .Build();
    
    // Détail faible (aperçu/preview)
    var optionsFaibles = MesherOptions.CreateBuilder()
        .WithTargetEdgeLengthXY(3.0)
        .WithFastPreset()
        .Build();
    
    return new EnsembleMaillagesLod
    {
        DetailEleve = IndexedMesh.FromMesh(mesher.Mesh(structure, optionsElevees)),
        DetailMoyen = IndexedMesh.FromMesh(mesher.Mesh(structure, optionsMoyennes)),
        DetailFaible = IndexedMesh.FromMesh(mesher.Mesh(structure, optionsFaibles))
    };
}
```

## Profilage et Surveillance

### Surveillance Performance Intégrée

FastGeoMesh inclut des compteurs performance :

```csharp
using FastGeoMesh.Utils;

// Réinitialiser compteurs avant test
PerformanceMonitor.Counters.Reset();

// Effectuer opérations
for (int i = 0; i < 100; i++)
{
    var mesh = mesher.Mesh(structure, options);
    var indexed = IndexedMesh.FromMesh(mesh);
}

// Analyser résultats
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Opérations totales : {stats.MeshingOperations}");
Console.WriteLine($"Quads moyens/op : {stats.AverageQuadsPerOperation:F1}");
Console.WriteLine($"Triangles moyens/op : {stats.AverageTrianglesPerOperation:F1}");
Console.WriteLine($"Ratio succès pool : {stats.PoolHitRatio:P1}");
```

### Profilage Personnalisé

Implémenter chronométrage personnalisé pour scénarios spécifiques :

```csharp
public class ProfileurMaillage
{
    private readonly List<ResultatMaillage> _resultats = new();
    
    public ResultatMaillage ProfilerMaillage(PrismStructureDefinition structure, MesherOptions options)
    {
        var sw = Stopwatch.StartNew();
        var memoireInitiale = GC.GetTotalMemory(false);
        
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(structure, options);
        var tempsMaillage = sw.Elapsed;
        
        sw.Restart();
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
        var tempsIndexation = sw.Elapsed;
        
        var memoireFinale = GC.GetTotalMemory(false);
        var memoireUtilisee = memoireFinale - memoireInitiale;
        
        var resultat = new ResultatMaillage
        {
            TempsMaillage = tempsMaillage,
            TempsIndexation = tempsIndexation,
            TempsTotal = tempsMaillage + tempsIndexation,
            MemoireUtilisee = memoireUtilisee,
            NombreQuads = indexed.QuadCount,
            NombreTriangles = indexed.TriangleCount,
            NombreSommets = indexed.VertexCount
        };
        
        _resultats.Add(resultat);
        return resultat;
    }
    
    public void AfficherStatistiques()
    {
        var tempsMaillageMove = _resultats.Average(r => r.TempsMaillage.TotalMicroseconds);
        var memoireMoyenne = _resultats.Average(r => r.MemoireUtilisee);
        var quadsMoyens = _resultats.Average(r => r.NombreQuads);
        
        Console.WriteLine($"Temps maillage moyen : {tempsMaillageMove:F0} μs");
        Console.WriteLine($"Usage mémoire moyen : {memoireMoyenne / 1024:F0} Ko");
        Console.WriteLine($"Nombre quads moyen : {quadsMoyens:F0}");
    }
}

public class ResultatMaillage
{
    public TimeSpan TempsMaillage { get; set; }
    public TimeSpan TempsIndexation { get; set; }
    public TimeSpan TempsTotal { get; set; }
    public long MemoireUtilisee { get; set; }
    public int NombreQuads { get; set; }
    public int NombreTriangles { get; set; }
    public int NombreSommets { get; set; }
}
```

### Tests Régression

Configurer tests régression performance automatisés :

```csharp
[Fact]
public void RegressionPerformanceMaillage()
{
    var structure = CreerStructureTestStandard();
    var options = MesherOptions.CreateBuilder().WithFastPreset().Build();
    
    var sw = Stopwatch.StartNew();
    var mesh = new PrismMesher().Mesh(structure, options);
    var ecoule = sw.Elapsed;
    
    // Asserter que performance n'a pas régressé
    ecoule.Should().BeLessThan(TimeSpan.FromMilliseconds(1.0), 
        "Préréglage rapide devrait compléter en moins de 1ms pour structure test standard");
    
    // Asserter usage mémoire
    var indexed = IndexedMesh.FromMesh(mesh);
    var estimationMemoire = indexed.VertexCount * 24 + indexed.QuadCount * 16; // Estimation approximative
    estimationMemoire.Should().BeLessThan(200_000, "Usage mémoire devrait être sous 200Ko");
}
```

## Bonnes Pratiques

### 1. Choisir Préréglages Appropriés

Commencer avec préréglages et personnaliser seulement si nécessaire :

```csharp
// Bon : Commencer avec préréglage
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(1.5)  // Personnaliser seulement ce qui est nécessaire
    .Build();

// Éviter : Construire from scratch inutilement
var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(2.0)
    .WithTargetEdgeLengthZ(2.0)
    .WithMinCapQuadQuality(0.3)
    // ... paramétrer chaque option manuellement
    .Build();
```

### 2. Profiler Tôt et Souvent

Ne pas supposer - mesurer performance dans votre environnement spécifique :

```csharp
// Profiler avec vos données réelles
var profileur = new ProfileurMaillage();
foreach (var structure in vosStructuresReelles.Take(10))
{
    profileur.ProfilerMaillage(structure, vosOptions);
}
profileur.AfficherStatistiques();
```

### 3. Considérer Votre Cas d'Usage

Optimiser pour votre scénario spécifique :

```csharp
// Visualisation temps réel : prioriser vitesse
var optionsTempsReel = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(3.0)
    .Build();

// Export final : prioriser qualité
var optionsExport = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .WithTargetEdgeLengthXY(0.2)
    .Build();
```

### 4. Valider Suppositions Performance

Tester cas limites et valider comportement mise à l'échelle :

```csharp
// Tester avec niveaux complexité variés
var taillesTest = new[] { 4, 8, 16, 32, 64 };
foreach (var taille in taillesTest)
{
    var structure = CreerPolygoneAvecSommets(taille);
    var resultat = profileur.ProfilerMaillage(structure, options);
    Console.WriteLine($"Sommets : {taille}, Temps : {resultat.TempsTotal.TotalMilliseconds:F1}ms");
}
```

### 5. Surveiller en Production

Inclure surveillance performance dans applications production :

```csharp
public class MailleurProduction
{
    private readonly ILogger<MailleurProduction> _logger;
    
    public IndexedMesh GenererMaillage(PrismStructureDefinition structure, MesherOptions options)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            var mesh = new PrismMesher().Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
            
            _logger.LogInformation("Maillage complété : {NombreQuads} quads, {Temps}ms", 
                indexed.QuadCount, sw.Elapsed.TotalMilliseconds);
            
            return indexed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maillage échoué après {Temps}ms", sw.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
```

Ce guide performance complet devrait vous aider à optimiser FastGeoMesh pour votre cas d'usage spécifique. Rappelez-vous que la meilleure configuration dépend de vos exigences spécifiques pour vitesse, qualité et usage mémoire.
