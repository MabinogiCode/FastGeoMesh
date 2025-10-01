# FastGeoMesh Documentation Index

**🇬🇧 English** | [🇫🇷 Français](#documentation-française)

---

## English Documentation

Welcome to the comprehensive FastGeoMesh documentation. This high-performance .NET 8 library provides sub-millisecond quad meshing for 2.5D prismatic structures.

### 📚 Documentation Structure

#### 🚀 Getting Started
- **[README](../README.md)** - Project overview, quick start, and basic examples
- **[NuGet Package](../nuget-readme.md)** - Package manager integration guide
- **[Installation & Setup](usage-guide.md#installation)** - Detailed installation instructions

#### 📖 Core Documentation
- **[Usage Guide](usage-guide.md)** - Comprehensive usage examples and patterns
  - Basic prismatic structures
  - Advanced features (holes, constraints, slabs)
  - Common use cases and patterns
  - Troubleshooting guide

- **[API Reference](api-reference.md)** - Complete API documentation
  - Core types and interfaces
  - Geometry primitives
  - Meshing algorithms
  - Export functionality

#### ⚡ Performance & Optimization
- **[Performance Guide](performance-guide.md)** - Optimization strategies and benchmarks
  - Performance benchmarks and scaling
  - Configuration strategies for different use cases
  - Memory optimization techniques
  - Profiling and monitoring

#### 🔧 Migration & Maintenance
- **[Migration Guide](migration-guide.md)** - Version upgrade instructions
  - Breaking changes and compatibility
  - New features and improvements
  - Best practices updates

### 🎯 Quick Access by Use Case

#### Real-Time Applications
- [Performance Guide - Real-Time Configuration](performance-guide.md#real-time-applications--500-μs)
- [Usage Guide - Fast Presets](usage-guide.md#performance-presets)

#### CAD/Precision Applications
- [Performance Guide - High-Precision CAD](performance-guide.md#high-precision-cad--10-ms)
- [Usage Guide - Quality Control](usage-guide.md#quality-control)

#### Architectural Modeling
- [Usage Guide - Building/Architecture Patterns](usage-guide.md#buildingarchitecture-modeling)
- [API Reference - Internal Surfaces](api-reference.md#internalsurfacedefinition-class)

#### GIS/Terrain Applications
- [Usage Guide - Terrain Modeling](usage-guide.md#terrain-modeling)
- [API Reference - Constraint Segments](api-reference.md#constraint-segments)

### 📊 Performance Quick Reference

| Use Case | Target Performance | Memory | Configuration |
|----------|-------------------|---------|---------------|
| Real-time Visualization | < 500 μs | < 100 KB | `WithFastPreset()` |
| Interactive CAD | < 2 ms | < 1 MB | Balanced settings |
| High-Precision | < 10 ms | < 10 MB | `WithHighQualityPreset()` |

### 🔗 External Resources

- **[GitHub Repository](https://github.com/MabinogiCode/FastGeoMesh)** - Source code and issues
- **[NuGet Package](https://www.nuget.org/packages/FastGeoMesh/)** - Package downloads and versions
- **[CI/CD Pipeline](https://github.com/MabinogiCode/FastGeoMesh/actions)** - Build status and tests
- **[Code Coverage](https://codecov.io/gh/MabinogiCode/FastGeoMesh)** - Test coverage reports

### 🛠️ Development Resources

#### Code Examples
- **[Sample Project](../samples/FastGeoMesh.Sample/)** - Complete working examples
- **[Test Suite](../tests/FastGeoMesh.Tests/)** - Comprehensive test examples
- **[Benchmark Suite](../benchmarks/FastGeoMesh.Benchmarks/)** - Performance benchmarks

#### Contributing
- **[Contributing Guidelines](../CONTRIBUTING.md)** - How to contribute to the project
- **[Code of Conduct](../CODE_OF_CONDUCT.md)** - Community guidelines
- **[License](../LICENSE)** - MIT License terms

### 📞 Support

- **Issues**: [GitHub Issues](https://github.com/MabinogiCode/FastGeoMesh/issues)
- **Discussions**: [GitHub Discussions](https://github.com/MabinogiCode/FastGeoMesh/discussions)
- **Email**: Contact through GitHub

---

## Documentation Française

Bienvenue dans la documentation complète de FastGeoMesh. Cette bibliothèque .NET 8 haute performance fournit un maillage de quads sous-milliseconde pour structures prismatiques 2.5D.

### 📚 Structure de la Documentation

#### 🚀 Démarrage
- **[README](../README.md#français)** - Aperçu du projet, démarrage rapide et exemples de base
- **[Package NuGet](../nuget-readme.md#français)** - Guide d'intégration gestionnaire de packages
- **[Installation & Configuration](usage-guide-fr.md#installation)** - Instructions d'installation détaillées

#### 📖 Documentation Principale
- **[Guide d'Usage](usage-guide-fr.md)** - Exemples d'usage complets et patrons
  - Structures prismatiques de base
  - Fonctionnalités avancées (trous, contraintes, dalles)
  - Cas d'usage courants et patrons
  - Guide de dépannage

- **[Référence API](api-reference-fr.md)** - Documentation API complète
  - Types et interfaces principaux
  - Primitives géométriques
  - Algorithmes de maillage
  - Fonctionnalité d'export

#### ⚡ Performance & Optimisation
- **[Guide Performance](performance-guide-fr.md)** - Stratégies d'optimisation et benchmarks
  - Benchmarks performance et mise à l'échelle
  - Stratégies de configuration pour différents cas d'usage
  - Techniques d'optimisation mémoire
  - Profilage et surveillance

#### 🔧 Migration & Maintenance
- **[Guide Migration](migration-guide-fr.md)** - Instructions de mise à niveau de version
  - Changements cassants et compatibilité
  - Nouvelles fonctionnalités et améliorations
  - Mises à jour des bonnes pratiques

### 🎯 Accès Rapide par Cas d'Usage

#### Applications Temps Réel
- [Guide Performance - Configuration Temps Réel](performance-guide-fr.md#applications-temps-réel--500-μs)
- [Guide Usage - Préréglages Rapides](usage-guide-fr.md#préréglages-performance)

#### Applications CAO/Précision
- [Guide Performance - CAO Haute Précision](performance-guide-fr.md#cao-haute-précision--10-ms)
- [Guide Usage - Contrôle Qualité](usage-guide-fr.md#contrôle-qualité)

#### Modélisation Architecturale
- [Guide Usage - Patrons Bâtiment/Architecture](usage-guide-fr.md#modélisation-bâtimentarchitecture)
- [Référence API - Surfaces Internes](api-reference-fr.md#classe-internalsurfacedefinition)

#### Applications SIG/Terrain
- [Guide Usage - Modélisation Terrain](usage-guide-fr.md#modélisation-terrain)
- [Référence API - Segments de Contrainte](api-reference-fr.md#segments-de-contrainte)

### 📊 Référence Rapide Performance

| Cas d'Usage | Performance Cible | Mémoire | Configuration |
|-------------|-------------------|---------|---------------|
| Visualisation Temps Réel | < 500 μs | < 100 Ko | `WithFastPreset()` |
| CAO Interactive | < 2 ms | < 1 Mo | Paramètres équilibrés |
| Haute Précision | < 10 ms | < 10 Mo | `WithHighQualityPreset()` |

### 🔗 Ressources Externes

- **[Dépôt GitHub](https://github.com/MabinogiCode/FastGeoMesh)** - Code source et problèmes
- **[Package NuGet](https://www.nuget.org/packages/FastGeoMesh/)** - Téléchargements et versions
- **[Pipeline CI/CD](https://github.com/MabinogiCode/FastGeoMesh/actions)** - Statut build et tests
- **[Couverture Code](https://codecov.io/gh/MabinogiCode/FastGeoMesh)** - Rapports couverture tests

### 🛠️ Ressources Développement

#### Exemples de Code
- **[Projet d'Exemple](../samples/FastGeoMesh.Sample/)** - Exemples complets fonctionnels
- **[Suite de Tests](../tests/FastGeoMesh.Tests/)** - Exemples de tests complets
- **[Suite Benchmarks](../benchmarks/FastGeoMesh.Benchmarks/)** - Benchmarks performance

#### Contribution
- **[Directives Contribution](../CONTRIBUTING.md)** - Comment contribuer au projet
- **[Code de Conduite](../CODE_OF_CONDUCT.md)** - Directives communauté
- **[Licence](../LICENSE)** - Termes licence MIT

### 📞 Support

- **Problèmes** : [GitHub Issues](https://github.com/MabinogiCode/FastGeoMesh/issues)
- **Discussions** : [GitHub Discussions](https://github.com/MabinogiCode/FastGeoMesh/discussions)
- **Email** : Contact via GitHub

---

## Documentation Navigation Tips

### 🔍 Finding What You Need

1. **New Users**: Start with [README](../README.md) → [Usage Guide](usage-guide.md)
2. **Performance Optimization**: Go directly to [Performance Guide](performance-guide.md)
3. **API Details**: Check [API Reference](api-reference.md)
4. **Upgrading**: See [Migration Guide](migration-guide.md)
5. **Troubleshooting**: [Usage Guide - Troubleshooting](usage-guide.md#troubleshooting)

### 📱 Mobile-Friendly

All documentation is optimized for mobile viewing with:
- Responsive tables
- Collapsible sections
- Quick navigation links
- Search-friendly structure

### 🔄 Staying Updated

- Watch the [GitHub repository](https://github.com/MabinogiCode/FastGeoMesh) for updates
- Check [Releases](https://github.com/MabinogiCode/FastGeoMesh/releases) for version notes
- Follow [Migration Guide](migration-guide.md) for upgrade instructions

**Last Updated**: November 2024  
**Version**: FastGeoMesh 1.1.0
