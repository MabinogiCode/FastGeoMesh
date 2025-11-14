# 🔄 Migration Guide - Dependency Injection

## ✅ La BONNE Façon d'utiliser FastGeoMesh (Clean Architecture)

### **Option 1 : Avec DI Container (RECOMMANDÉ)** 🎯

#### **ASP.NET Core / Modern .NET Applications**

```csharp
using FastGeoMesh;
using Microsoft.Extensions.DependencyInjection;

// 1. Dans Program.cs ou Startup.cs
var builder = WebApplication.CreateBuilder(args);

// Enregistrer tous les services FastGeoMesh
builder.Services.AddFastGeoMesh();

// OU avec monitoring de performance
builder.Services.AddFastGeoMeshWithMonitoring();

var app = builder.Build();

// 2. Dans vos controllers/services
public class MeshingController : ControllerBase
{
    private readonly IPrismMesher _mesher;

    // ✅ Injection via constructeur
    public MeshingController(IPrismMesher mesher)
    {
        _mesher = mesher;
    }

    [HttpPost("mesh")]
    public async Task<IActionResult> GenerateMesh([FromBody] MeshRequest request)
    {
        var structure = new PrismStructureDefinition(
            request.Footprint,
            request.BaseZ,
            request.TopZ
        );

        var options = MesherOptions.CreateBuilder()
            .WithFastPreset()
            .Build();

        if (options.IsFailure)
            return BadRequest(options.Error);

        var result = await _mesher.MeshAsync(structure, options.Value);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}
```

#### **Console Applications / Workers**

```csharp
using FastGeoMesh;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // ✅ Enregistrer FastGeoMesh
        services.AddFastGeoMesh();

        // Vos autres services
        services.AddHostedService<MeshingWorker>();
    })
    .Build();

await host.RunAsync();

// Dans votre Worker
public class MeshingWorker : BackgroundService
{
    private readonly IPrismMesher _mesher;

    public MeshingWorker(IPrismMesher mesher)
    {
        _mesher = mesher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Utiliser _mesher...
    }
}
```

---

### **Option 2 : Sans Framework DI (Applications simples)**

Si vous n'utilisez pas de framework avec DI intégré :

```csharp
using FastGeoMesh.Infrastructure.Services;
using FastGeoMesh.Application.Services;
using FastGeoMesh.Application.Strategies;
using FastGeoMesh.Domain.Services;

// ✅ Créer les services manuellement (en respectant les dépendances)
IGeometryService geometryService = new GeometryService();
IClock clock = new SystemClock();
IPerformanceMonitor monitor = new NullPerformanceMonitor();

ICapMeshingStrategy capStrategy = new DefaultCapMeshingStrategy(geometryService);

IPrismMesher mesher = new PrismMesher(capStrategy, monitor, geometryService);

// Utiliser le mesher
var structure = new PrismStructureDefinition(...);
var options = MesherOptions.CreateBuilder().Build();

var result = await mesher.MeshAsync(structure, options.Value);
```

---

### **Option 3 : Constructeur Sans Paramètres (DÉCONSEILLÉ)** ⚠️

```csharp
// ⚠️ Fonctionne mais utilise réflexion (hack interne)
var mesher = new PrismMesher();

// Problèmes :
// - Utilise réflexion (lent)
// - Pas testable facilement
// - Couplage caché
// - Ne respecte pas pleinement Clean Architecture
```

**À utiliser UNIQUEMENT pour** :
- Prototypage rapide
- Scripts simples
- Tests de compatibilité

**À remplacer par Option 1 ou 2 en production !**

---

## 📊 Comparaison des Approches

| Critère | DI Container | Manuel | Sans Param |
|---------|--------------|--------|------------|
| **Clean Architecture** | ✅ Parfait | ✅ Bon | ⚠️ Acceptable |
| **Testabilité** | ✅ Excellente | ✅ Bonne | ❌ Difficile |
| **Performance** | ✅ Optimal | ✅ Optimal | ⚠️ Réflexion |
| **Maintenabilité** | ✅ Excellente | 🟡 Moyenne | ❌ Faible |
| **Recommandé pour** | Production | Apps simples | Prototypes |

---

## 🎯 Services Disponibles

Lorsque vous appelez `AddFastGeoMesh()`, ces services sont enregistrés :

| Interface | Implémentation | Lifetime |
|-----------|----------------|----------|
| `IGeometryService` | `GeometryService` | Singleton |
| `IClock` | `SystemClock` | Singleton |
| `IPerformanceMonitor` | `NullPerformanceMonitor` | Transient |
| `ICapMeshingStrategy` | `DefaultCapMeshingStrategy` | Transient |
| `IPrismMesher` | `PrismMesher` | Transient |
| `IAsyncMesher` | `PrismMesher` | Transient |

### **Avec Monitoring**

Si vous utilisez `AddFastGeoMeshWithMonitoring()` :

| Interface | Implémentation | Lifetime |
|-----------|----------------|----------|
| `IPerformanceMonitor` | `PerformanceMonitorService` | **Singleton** |

---

## 🧪 Tests Unitaires

Avec DI, les tests deviennent triviaux :

```csharp
using Moq;
using Xunit;

public class MeshingTests
{
    [Fact]
    public async Task Should_Generate_Mesh_Successfully()
    {
        // Arrange - Mock des dépendances
        var mockGeometryService = new Mock<IGeometryService>();
        mockGeometryService
            .Setup(x => x.PolygonArea(It.IsAny<ReadOnlySpan<Vec2>>()))
            .Returns(100.0);

        var mockMonitor = new Mock<IPerformanceMonitor>();
        var mockStrategy = new Mock<ICapMeshingStrategy>();

        var mesher = new PrismMesher(
            mockStrategy.Object,
            mockMonitor.Object,
            mockGeometryService.Object
        );

        // Act
        var structure = new PrismStructureDefinition(...);
        var options = MesherOptions.CreateBuilder().Build();
        var result = await mesher.MeshAsync(structure, options.Value);

        // Assert
        Assert.True(result.IsSuccess);
        mockGeometryService.Verify(x => x.PolygonArea(...), Times.AtLeastOnce);
    }
}
```

---

## ⚙️ Configuration Avancée

### **Remplacer des Services**

```csharp
services.AddFastGeoMesh();

// Remplacer le monitoring par votre implémentation
services.Replace(ServiceDescriptor.Singleton<IPerformanceMonitor, MyCustomMonitor>());

// Remplacer la stratégie de cap meshing
services.Replace(ServiceDescriptor.Transient<ICapMeshingStrategy, MyCustomCapStrategy>());
```

### **Ajouter des Services Supplémentaires**

```csharp
services.AddFastGeoMesh();

// Ajouter vos propres services qui dépendent de FastGeoMesh
services.AddScoped<IMeshValidator>(sp =>
{
    var geometryService = sp.GetRequiredService<IGeometryService>();
    return new MeshValidator(geometryService);
});
```

---

## 🚀 Exemples Complets

### **API REST avec validation**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastGeoMesh();
builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();
app.Run();

// MeshController.cs
[ApiController]
[Route("api/[controller]")]
public class MeshController : ControllerBase
{
    private readonly IPrismMesher _mesher;
    private readonly ILogger<MeshController> _logger;

    public MeshController(IPrismMesher mesher, ILogger<MeshController> logger)
    {
        _mesher = mesher;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<MeshResponse>> GenerateMesh(
        [FromBody] MeshRequest request,
        CancellationToken cancellationToken)
    {
        var structure = MapToStructure(request);
        var optionsResult = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(request.EdgeLength)
            .WithFastPreset()
            .Build();

        if (optionsResult.IsFailure)
        {
            _logger.LogWarning("Invalid options: {Error}", optionsResult.Error.Description);
            return BadRequest(new { Error = optionsResult.Error.Description });
        }

        var meshResult = await _mesher.MeshAsync(structure, optionsResult.Value, cancellationToken);

        if (meshResult.IsFailure)
        {
            _logger.LogError("Meshing failed: {Error}", meshResult.Error.Description);
            return StatusCode(500, new { Error = meshResult.Error.Description });
        }

        return Ok(MapToResponse(meshResult.Value));
    }
}
```

### **Worker Service avec batch processing**

```csharp
public class MeshingWorker : BackgroundService
{
    private readonly IPrismMesher _mesher;
    private readonly ILogger<MeshingWorker> _logger;

    public MeshingWorker(IPrismMesher mesher, ILogger<MeshingWorker> logger)
    {
        _mesher = mesher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var structures = await LoadPendingStructures();

            if (structures.Any())
            {
                var options = MesherOptions.CreateBuilder()
                    .WithHighQualityPreset()
                    .Build().Value;

                // Batch processing parallèle
                var asyncMesher = (IAsyncMesher)_mesher;
                var result = await asyncMesher.MeshBatchAsync(
                    structures,
                    options,
                    maxDegreeOfParallelism: 4,
                    cancellationToken: stoppingToken
                );

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Processed {Count} meshes", result.Value.Count);
                    await SaveResults(result.Value);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

---

## ❓ FAQ

**Q: Dois-je migrer mon code existant ?**
A: Pas immédiatement, mais c'est **fortement recommandé** pour :
- Meilleure testabilité
- Respect Clean Architecture
- Performance (éviter réflexion)
- Maintenabilité future

**Q: Le constructeur sans paramètres va-t-il disparaître ?**
A: Il sera marqué `[Obsolete]` dans une future version mais restera disponible pour compatibilité.

**Q: Puis-je mélanger DI et instanciation manuelle ?**
A: Oui, mais ce n'est pas recommandé. Choisissez une approche cohérente.

**Q: Quid des applications non-DI (.NET Framework) ?**
A: Utilisez l'Option 2 (création manuelle des services) ou un container tiers (Autofac, Ninject, etc.)

---

**Auteur** : Claude (Sonnet 4.5)
**Date** : 2025-11-14
**Version** : FastGeoMesh v2.1+
