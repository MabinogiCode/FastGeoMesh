# Guide d'Usage FastGeoMesh

Ce guide complet couvre tous les aspects de l'utilisation de FastGeoMesh pour générer des maillages à dominante quadrilatérale haute performance à partir de structures prismatiques 2.5D.

## Table des Matières

1. [Concepts Fondamentaux](#concepts-fondamentaux)
2. [Usage Basique](#usage-basique)
3. [Fonctionnalités Avancées](#fonctionnalités-avancées)
4. [Optimisation Performance](#optimisation-performance)
5. [Formats d'Export](#formats-dexport)
6. [Bonnes Pratiques](#bonnes-pratiques)
7. [Patrons Communs](#patrons-communs)
8. [Dépannage](#dépannage)

## Concepts Fondamentaux

### Structures Prismatiques

FastGeoMesh se spécialise dans les **structures prismatiques 2.5D** - géométries définies par :
- Un polygone d'empreinte 2D (plan XY)
- Une étendue verticale (plage Z)
- Caractéristiques internes optionnelles (trous, contraintes, dalles)

### Types de Maillage

La bibliothèque génère des **maillages à dominante quadrilatérale** avec triangles de secours optionnels :
- **Quads Latéraux** : Faces verticales du prisme
- **Quads de Chapeau** : Faces supérieure/inférieure (si seuil qualité atteint)
- **Triangles de Chapeau** : Secours pour quads de faible qualité (optionnel)

### Système de Coordonnées

- **Plan XY** : Empreinte horizontale
- **Axe Z** : Élévation verticale
- **Enroulement** : Sens anti-horaire (CCW) pour normales sortantes

## Usage Basique

### Prisme Rectangulaire Simple

Le cas d'usage le plus basique - un prisme rectangulaire :

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

// Définir rectangle 20x5 de Z=-10 à Z=10
var rectangle = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(20, 0), 
    new Vec2(20, 5), new Vec2(0, 5)
});

var structure = new PrismStructureDefinition(rectangle, -10, 10);

// Utiliser préréglage rapide pour performance optimale
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh);

Console.WriteLine($"Généré : {indexed.QuadCount} quads, {indexed.TriangleCount} triangles");
```

### Polygone Complexe

Pour des empreintes non-rectangulaires :

```csharp
// Empreinte en forme de L
var formeL = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 4),
    new Vec2(6, 4), new Vec2(6, 8), new Vec2(0, 8)
});

var structure = new PrismStructureDefinition(formeL, 0, 5);

var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)
    .WithTargetEdgeLengthZ(1.0)
    .Build();

var mesh = new PrismMesher().Mesh(structure, options);
```

## Fonctionnalités Avancées

### Segments de Contrainte

Ajouter des segments de contrainte horizontaux à des niveaux Z spécifiques pour créer des éléments structurels intermédiaires :

```csharp
var structure = new PrismStructureDefinition(empreinte, 0, 10);

// Ajouter poutre à Z = 5
structure = structure.AddConstraintSegment(
    new Segment2D(new Vec2(0, 2), new Vec2(20, 2)), 5.0);

// Ajouter contraintes multiples
structure = structure
    .AddConstraintSegment(new Segment2D(new Vec2(5, 0), new Vec2(5, 5)), 3.0)
    .AddConstraintSegment(new Segment2D(new Vec2(15, 0), new Vec2(15, 5)), 7.0);
```

### Trous dans l'Empreinte

Créer des structures avec trous internes :

```csharp
var exterieur = Polygon2D.FromPoints(new[] {
    new Vec2(0, 0), new Vec2(10, 0), 
    new Vec2(10, 6), new Vec2(0, 6)
});

var trou1 = Polygon2D.FromPoints(new[] {
    new Vec2(2, 2), new Vec2(4, 2), 
    new Vec2(4, 4), new Vec2(2, 4)
});

var trou2 = Polygon2D.FromPoints(new[] {
    new Vec2(6, 1), new Vec2(8, 1), 
    new Vec2(8, 3), new Vec2(6, 3)
});

var structure = new PrismStructureDefinition(exterieur, 0, 5)
    .AddHole(trou1)
    .AddHole(trou2);

// Configurer raffinement près des trous
var options = MesherOptions.CreateBuilder()
    .WithHoleRefinement(longueurCible: 0.5, bande: 1.0)
    .Build();
```

### Surfaces Internes (Dalles)

Ajouter surfaces internes horizontales à des élévations spécifiques :

```csharp
// Structure principale
var structure = new PrismStructureDefinition(empreinte, -5, 10);

// Ajouter dalle intermédiaire à Z = 2 avec trou
var contourDalle = Polygon2D.FromPoints(new[] {
    new Vec2(1, 1), new Vec2(19, 1), 
    new Vec2(19, 4), new Vec2(1, 4)
});

var trouDalle = Polygon2D.FromPoints(new[] {
    new Vec2(8, 2), new Vec2(12, 2), 
    new Vec2(12, 3), new Vec2(8, 3)
});

structure = structure.AddInternalSurface(contourDalle, 2.0, trouDalle);
```

### Géométrie Auxiliaire

Ajouter points et segments qui seront préservés dans le maillage de sortie :

```csharp
structure.Geometry
    .AddPoint(new Vec3(10, 2.5, 5))     // Point isolé
    .AddSegment(new Segment3D(          // Segment 3D
        new Vec3(0, 2.5, 0), 
        new Vec3(20, 2.5, 10)))
    .AddPoints(new[] {                  // Points multiples
        new Vec3(5, 1, 3),
        new Vec3(15, 4, 7)
    });
```

## Optimisation Performance

### Préréglages Performance

Choisir le préréglage approprié pour votre cas d'usage :

```csharp
// Préréglage Rapide (~305μs pour géométrie simple)
// - Longueurs d'arête plus grandes pour moins d'éléments
// - Seuils de qualité plus bas
// - Triangles de secours minimaux
var rapide = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .Build();

// Préréglage Haute-Qualité (~1.3ms pour géométrie simple)
// - Longueurs d'arête plus petites pour maillage plus dense
// - Seuils de qualité plus élevés
// - Plus de triangles de secours pour la qualité
var qualite = MesherOptions.CreateBuilder()
    .WithHighQualityPreset()
    .Build();
```

### Réglage Performance Personnalisé

Régler finement la performance pour des exigences spécifiques :

```csharp
var options = MesherOptions.CreateBuilder()
    // Longueurs d'arête primaires (plus petit = plus de détail, plus lent)
    .WithTargetEdgeLengthXY(2.0)        // Rapide: 2.0, Qualité: 0.5
    .WithTargetEdgeLengthZ(2.0)         // Rapide: 2.0, Qualité: 0.5
    
    // Raffinement près des caractéristiques
    .WithHoleRefinement(1.0, 2.0)       // Optionnel: maillage plus fin près des trous
    .WithSegmentRefinement(1.0, 1.5)    // Optionnel: maillage plus fin près des segments
    
    // Contrôle qualité
    .WithMinCapQuadQuality(0.3)         // Rapide: 0.3, Qualité: 0.7
    .WithRejectedCapTriangles(false)    // Rapide: false, Qualité: true
    
    // Génération des chapeaux
    .WithCaps(bottom: true, top: true)
    
    .Build();
```

### Optimisation Mémoire

Pour environnements contraints en mémoire :

```csharp
var options = MesherOptions.CreateBuilder()
    .WithFastPreset()
    .WithTargetEdgeLengthXY(3.0)        // Éléments plus grands = moins de mémoire
    .WithRejectedCapTriangles(false)    // Ignorer génération triangles
    .Build();

// Utiliser epsilon plus grand pour déduplication coordonnées
var indexed = IndexedMesh.FromMesh(mesh, epsilon: 1e-6);
```

### Traitement Asynchrone

Pour opérations non-bloquantes :

```csharp
var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

try
{
    var mesh = await new PrismMesher()
        .MeshAsync(structure, options, cancellationToken);
    
    // Traiter maillage...
}
catch (OperationCanceledException)
{
    Console.WriteLine("Opération de maillage expirée");
}
```

## Formats d'Export

### Format OBJ (Quads + Triangles)

Préserve la structure des quads avec triangles de secours :

```csharp
using FastGeoMesh.Meshing.Exporters;

var indexed = IndexedMesh.FromMesh(mesh);
ObjExporter.Write(indexed, "sortie.obj");

// Le fichier OBJ contiendra :
// - lignes 'v' pour les sommets
// - lignes 'f' avec 5 indices pour les quads
// - lignes 'f' avec 4 indices pour les triangles
```

### Format glTF (Triangulé)

Format standard pour applications 3D :

```csharp
GltfExporter.Write(indexed, "sortie.gltf");

// Crée glTF autonome avec données binaires intégrées
// Tous les quads sont automatiquement triangulés
// Convient pour visualiseurs web, moteurs de jeu, etc.
```

### Format SVG (Vue de Dessus)

Représentation 2D montrant la structure du maillage :

```csharp
SvgExporter.Write(indexed, "sortie.svg");

// Crée SVG montrant :
// - Projection vue de dessus de toutes les arêtes
// - Utile pour déboguer la topologie du maillage
// - Peut être visualisé dans navigateurs web
```

## Bonnes Pratiques

### Définition de Polygone

1. **Assurer enroulement CCW** pour frontières extérieures
2. **Utiliser enroulement CW** pour trous (corrigé automatiquement)
3. **Éviter auto-intersections** et arêtes dégénérées
4. **Valider polygones** avant traitement :

```csharp
if (!Polygon2D.Validate(sommets, out string? erreur))
{
    throw new ArgumentException($"Polygone invalide : {erreur}");
}
```

### Sélection Longueur d'Arête

Choisir longueurs d'arête selon vos exigences :

```csharp
// Pour visualisation (rapide)
var visuel = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(5.0)    // Maillage grossier
    .Build();

// Pour analyse (équilibré)
var analyse = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(1.0)    // Maillage moyen
    .Build();

// Pour haute précision (lent)
var precision = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(0.1)    // Maillage fin
    .Build();
```

### Contrôle Qualité

Équilibrer qualité des quads vs nombre de triangles :

```csharp
// Préférer quads (accepter qualité moindre)
var dominanceQuad = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.2)
    .WithRejectedCapTriangles(false)
    .Build();

// Préférer qualité (plus de triangles)
var hauteQualite = MesherOptions.CreateBuilder()
    .WithMinCapQuadQuality(0.8)
    .WithRejectedCapTriangles(true)
    .Build();
```

## Patrons Communs

### Modélisation Bâtiment/Architecture

```csharp
// Empreinte bâtiment avec cour
var batiment = Polygon2D.FromPoints(/* frontière extérieure */);
var cour = Polygon2D.FromPoints(/* cour intérieure */);

var structure = new PrismStructureDefinition(batiment, 0, 30)  // 30m de haut
    .AddHole(cour);

// Ajouter dalles de plancher tous les 3m
for (double z = 3; z <= 27; z += 3)
{
    structure = structure.AddInternalSurface(batiment, z, cour);
}

var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(2.0)
    .WithTargetEdgeLengthZ(3.0)     // Aligner avec hauteurs d'étage
    .Build();
```

### Excavation/Mines

```csharp
// Fosse d'excavation avec gradins
var fosse = Polygon2D.FromPoints(/* contour fosse */);
var structure = new PrismStructureDefinition(fosse, -20, 0);  // 20m de profondeur

// Ajouter niveaux de gradins tous les 5m
for (double z = -15; z <= -5; z += 5)
{
    var largeurGradin = 2.0;  // Gradins de 2m de large
    var polyGradin = CreerPolygoneDecale(fosse, -largeurGradin);
    
    structure = structure.AddConstraintSegment(
        new Segment2D(polyGradin.Vertices[0], polyGradin.Vertices[1]), z);
}
```

### Modélisation Terrain

```csharp
// Terrain basé sur courbes de niveau
var frontiere = Polygon2D.FromPoints(/* frontière terrain */);
var structure = new PrismStructureDefinition(frontiere, 0, elevationMax);

// Ajouter courbes de niveau comme contraintes
foreach (var courbe in courbesNiveau)
{
    structure = structure.AddConstraintSegment(courbe.Segment, courbe.Elevation);
}

var options = MesherOptions.CreateBuilder()
    .WithTargetEdgeLengthXY(10.0)   // Résolution 10m
    .WithSegmentRefinement(5.0, 20.0)  // Raffiner près des courbes
    .Build();
```

## Dépannage

### Problèmes Courants

1. **Maillage Vide Généré**
   - Vérifier enroulement polygone (doit être CCW)
   - Vérifier plage Z (elevationSup > elevationBase)
   - Assurer que polygone est valide et non-dégénéré

2. **Problèmes de Performance**
   - Utiliser FastPreset pour meilleure performance
   - Augmenter longueurs d'arête cibles
   - Désactiver génération triangles si non nécessaire
   - Vérifier paramètres de raffinement excessifs

3. **Problèmes de Qualité**
   - Augmenter seuil MinCapQuadQuality
   - Activer OutputRejectedCapTriangles
   - Réduire longueurs d'arête cibles pour contrôle plus fin

4. **Usage Mémoire**
   - Utiliser longueurs d'arête plus grandes pour réduire nombre d'éléments
   - Augmenter epsilon pour déduplication coordonnées
   - Désactiver fonctionnalités inutiles (chapeaux, triangles)

### Outils de Débogage

```csharp
// Vérifier statistiques maillage
Console.WriteLine($"Sommets : {indexed.VertexCount}");
Console.WriteLine($"Arêtes : {indexed.EdgeCount}");
Console.WriteLine($"Quads : {indexed.QuadCount}");
Console.WriteLine($"Triangles : {indexed.TriangleCount}");

// Analyser qualité maillage
var adjacence = indexed.BuildAdjacency();
Console.WriteLine($"Arêtes frontière : {adjacence.BoundaryEdges.Count}");
Console.WriteLine($"Arêtes non-manifold : {adjacence.NonManifoldEdges.Count}");

// Export pour inspection visuelle
SvgExporter.Write(indexed, "debug.svg");  // Voir topologie maillage
```

### Surveillance Performance

```csharp
// Activer surveillance performance
using FastGeoMesh.Utils;

// Surveiller opérations
var stats = PerformanceMonitor.Counters.GetStatistics();
Console.WriteLine($"Opérations maillage : {stats.MeshingOperations}");
Console.WriteLine($"Quads moyens par opération : {stats.AverageQuadsPerOperation:F1}");
```

Ceci complète le guide d'usage complet. Pour détails API spécifiques, voir la [Référence API](api-reference-fr.md).
