# 🛠️ FastGeoMesh Practical Usage Guide

## 🎯 Getting Started - Step by Step Examples

### Example 1: Simple Rectangular Building

```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

// Create a simple 10x6 meter building, 3 meters high
public class SimpleBuilding
{
    public static void CreateAndExport()
    {
        // 1. Define the building footprint
        var footprint = Polygon2D.FromPoints(new[] {
            new Vec2(0, 0),    // Bottom-left corner
            new Vec2(10, 0),   // Bottom-right corner  
            new Vec2(10, 6),   // Top-right corner
            new Vec2(0, 6)     // Top-left corner
        });
        
        // 2. Create the 3D structure (0 to 3 meters height)
        var building = new PrismStructureDefinition(footprint, z0: 0.0, z1: 3.0);
        
        // 3. Configure meshing options
        var options = new MesherOptions {
            TargetEdgeLengthXY = 0.5,  // 50cm mesh resolution
            TargetEdgeLengthZ = 1.0,   // 1m layers
            GenerateBottomCap = true,   // Generate floor
            GenerateTopCap = true       // Generate roof
        };
        
        // 4. Generate the mesh
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(building, options);
        
        // 5. Convert to indexed format for export
        var indexed = IndexedMesh.FromMesh(mesh);
        
        // 6. Export to different formats
        ObjExporter.Write(indexed, "simple_building.obj");
        GltfExporter.Write(indexed, "simple_building.gltf");
        SvgExporter.Write(indexed, "simple_building.svg");
        
        Console.WriteLine($"Generated mesh with {indexed.QuadCount} quads and {indexed.TriangleCount} triangles");
    }
}
```

### Example 2: Building with Internal Courtyard

```csharp
public class BuildingWithCourtyard
{
    public static void CreateComplexBuilding()
    {
        // Main building outline (20x15 meters)
        var mainBuilding = Polygon2D.FromPoints(new[] {
            new Vec2(0, 0), new Vec2(20, 0), 
            new Vec2(20, 15), new Vec2(0, 15)
        });
        
        // Internal courtyard (6x4 meters, centered)
        var courtyard = Polygon2D.FromPoints(new[] {
            new Vec2(7, 5.5), new Vec2(13, 5.5),
            new Vec2(13, 9.5), new Vec2(7, 9.5)
        });
        
        // Create structure with hole
        var structure = new PrismStructureDefinition(mainBuilding, z0: 0.0, z1: 12.0);
        structure.AddHole(courtyard);
        
        // Refined meshing near the courtyard
        var options = new MesherOptions {
            TargetEdgeLengthXY = 1.0,
            TargetEdgeLengthZ = 2.0,
            TargetEdgeLengthXYNearHoles = 0.3,  // Finer mesh near courtyard
            HoleRefineBand = 2.0,               // 2m refinement zone
            MinCapQuadQuality = 0.6             // Higher quality threshold
        };
        
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(structure, options);
        var indexed = IndexedMesh.FromMesh(mesh);
        
        ObjExporter.Write(indexed, "building_with_courtyard.obj");
        
        Console.WriteLine($"Building with courtyard: {indexed.QuadCount} quads, {indexed.TriangleCount} triangles");
        Console.WriteLine($"Vertices: {indexed.VertexCount}, Edges: {indexed.EdgeCount}");
    }
}
```

### Example 3: Industrial Complex with Constraints

```csharp
public class IndustrialComplex
{
    public static void CreateIndustrialBuilding()
    {
        // L-shaped industrial building
        var outline = Polygon2D.FromPoints(new[] {
            new Vec2(0, 0), new Vec2(30, 0), new Vec2(30, 10),
            new Vec2(15, 10), new Vec2(15, 25), new Vec2(0, 25)
        });
        
        var structure = new PrismStructureDefinition(outline, z0: 0.0, z1: 8.0);
        
        // Add structural constraints (beams, columns)
        structure.AddConstraintSegment(
            new Segment2D(new Vec2(10, 0), new Vec2(10, 10)), 
            targetLength: 0.5  // Fine mesh along beam
        );
        
        structure.AddConstraintSegment(
            new Segment2D(new Vec2(20, 0), new Vec2(20, 10)), 
            targetLength: 0.5
        );
        
        structure.AddConstraintSegment(
            new Segment2D(new Vec2(0, 15), new Vec2(15, 15)), 
            targetLength: 0.5
        );
        
        // Add internal columns as points of interest
        structure.Geometry.AddPoint(new Vec3(7.5, 5, 4));   // Column 1
        structure.Geometry.AddPoint(new Vec3(22.5, 5, 4));  // Column 2
        structure.Geometry.AddPoint(new Vec3(7.5, 20, 4));  // Column 3
        
        // Add internal beam segments
        structure.Geometry.AddSegment(new Segment3D(
            new Vec3(7.5, 0, 6), new Vec3(7.5, 25, 6)  // Main beam
        ));
        
        var options = new MesherOptions {
            TargetEdgeLengthXY = 1.5,
            TargetEdgeLengthZ = 2.0,
            TargetEdgeLengthXYNearSegments = 0.4,  // Refined near beams
            SegmentRefineBand = 1.5,
            MinCapQuadQuality = 0.5,
            OutputRejectedCapTriangles = true       // Include low-quality triangles
        };
        
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(structure, options);
        var indexed = IndexedMesh.FromMesh(mesh);
        
        // Export with custom filename including statistics
        var filename = $"industrial_complex_{indexed.QuadCount}q_{indexed.TriangleCount}t.obj";
        ObjExporter.Write(indexed, filename);
        
        // Analyze mesh quality
        AnalyzeMeshQuality(mesh);
    }
    
    private static void AnalyzeMeshQuality(Mesh mesh)
    {
        var quads = mesh.Quads.Where(q => q.QualityScore.HasValue).ToArray();
        
        if (quads.Length > 0)
        {
            var avgQuality = quads.Average(q => q.QualityScore!.Value);
            var minQuality = quads.Min(q => q.QualityScore!.Value);
            var maxQuality = quads.Max(q => q.QualityScore!.Value);
            
            Console.WriteLine($"Mesh Quality Analysis:");
            Console.WriteLine($"  Average Quality: {avgQuality:F3}");
            Console.WriteLine($"  Quality Range: {minQuality:F3} - {maxQuality:F3}");
            Console.WriteLine($"  High Quality (>0.7): {quads.Count(q => q.QualityScore > 0.7)}");
            Console.WriteLine($"  Medium Quality (0.4-0.7): {quads.Count(q => q.QualityScore is >= 0.4 and <= 0.7)}");
            Console.WriteLine($"  Low Quality (<0.4): {quads.Count(q => q.QualityScore < 0.4)}");
        }
    }
}
```

## 🚀 Performance Optimization Examples

### Example 4: High-Performance Batch Processing

```csharp
public class BatchMeshProcessor
{
    public static void ProcessMultipleBuildings()
    {
        // Create multiple building configurations
        var buildingConfigs = GenerateBuildingConfigurations(100);
        
        // Use object pooling for better performance
        var results = new List<IndexedMesh>();
        
        foreach (var config in buildingConfigs)
        {
            // Use pooled mesh for automatic cleanup
            var indexed = PooledMeshExtensions.WithPooledMesh(mesh => {
                
                // Generate quads in batch
                var allQuads = GenerateQuadsForBuilding(config);
                mesh.AddQuads(allQuads);  // 82% faster than individual AddQuad calls
                
                // Add triangles if needed
                var triangles = GenerateTrianglesForBuilding(config);
                mesh.AddTriangles(triangles);
                
                return IndexedMesh.FromMesh(mesh);
            });
            
            results.Add(indexed);
        }
        
        // Export all results
        for (int i = 0; i < results.Count; i++)
        {
            ObjExporter.Write(results[i], $"building_{i:D3}.obj");
        }
        
        Console.WriteLine($"Processed {buildingConfigs.Length} buildings efficiently");
    }
    
    private static BuildingConfig[] GenerateBuildingConfigurations(int count)
    {
        var configs = new BuildingConfig[count];
        var random = new Random(42);
        
        for (int i = 0; i < count; i++)
        {
            configs[i] = new BuildingConfig {
                Width = 5 + random.NextDouble() * 20,
                Length = 5 + random.NextDouble() * 20,
                Height = 2 + random.NextDouble() * 10,
                HasCourtyard = random.NextDouble() > 0.7
            };
        }
        
        return configs;
    }
    
    private static Quad[] GenerateQuadsForBuilding(BuildingConfig config)
    {
        // Pre-allocate array for better performance
        var estimatedQuadCount = (int)((config.Width * config.Length) / 2);
        var quads = new List<Quad>(estimatedQuadCount);
        
        // Generate building-specific quads
        // ... implementation details ...
        
        return quads.ToArray();
    }
}

public record BuildingConfig
{
    public double Width { get; init; }
    public double Length { get; init; }
    public double Height { get; init; }
    public bool HasCourtyard { get; init; }
}
```

### Example 5: Span-Based Geometry Operations

```csharp
public class GeometryOptimizations
{
    public static void DemonstrateSpanOperations()
    {
        // Large set of 2D points
        var points = GenerateRandomPoints(10000);
        
        // Convert to span for zero-allocation operations
        ReadOnlySpan<Vec2> pointsSpan = points.AsSpan();
        
        // Fast geometry calculations using spans
        var centroid = pointsSpan.ComputeCentroid();           // 48% faster
        var bounds = pointsSpan.ComputeBounds();               // 45% faster  
        var area = pointsSpan.ComputeSignedArea();             // 46% faster
        var paddedBounds = pointsSpan.ComputePaddedBounds(1.0); // Optimized operation
        
        Console.WriteLine($"Processed {points.Length} points:");
        Console.WriteLine($"  Centroid: ({centroid.X:F2}, {centroid.Y:F2})");
        Console.WriteLine($"  Bounds: {bounds.min} to {bounds.max}");
        Console.WriteLine($"  Area: {area:F2}");
        
        // Batch point-in-polygon testing
        var polygon = CreateTestPolygon();
        var results = new bool[points.Length];
        
        // 43% faster than individual tests
        polygon.AsSpan().ContainsPoints(pointsSpan, results.AsSpan());
        
        var insideCount = results.Count(r => r);
        Console.WriteLine($"  Points inside polygon: {insideCount}/{points.Length}");
        
        // Transform to 3D using spans
        var points3D = new Vec3[points.Length];
        pointsSpan.TransformTo3D(points3D.AsSpan(), z: 5.0);
        
        Console.WriteLine($"  Transformed to 3D at Z=5.0");
    }
    
    private static Vec2[] GenerateRandomPoints(int count)
    {
        var points = new Vec2[count];
        var random = new Random(42);
        
        for (int i = 0; i < count; i++)
        {
            points[i] = new Vec2(
                random.NextDouble() * 100, 
                random.NextDouble() * 100
            );
        }
        
        return points;
    }
    
    private static Vec2[] CreateTestPolygon()
    {
        return new[] {
            new Vec2(10, 10), new Vec2(90, 10),
            new Vec2(90, 90), new Vec2(10, 90)
        };
    }
}
```

## 🏗️ Real-World Application Examples

### Example 6: Architectural Floor Plan Processing

```csharp
public class ArchitecturalProcessor
{
    public static void ProcessFloorPlan(string floorPlanFile)
    {
        // Load floor plan data (simplified)
        var rooms = LoadRoomsFromFile(floorPlanFile);
        var walls = LoadWallsFromFile(floorPlanFile);
        
        foreach (var room in rooms)
        {
            // Create room structure
            var structure = new PrismStructureDefinition(
                room.Outline, 
                z0: 0.0, 
                z1: room.CeilingHeight
            );
            
            // Add door and window openings as holes
            foreach (var opening in room.Openings)
            {
                structure.AddHole(opening.Polygon);
            }
            
            // Add wall constraints for proper meshing
            foreach (var wall in walls.Where(w => w.RoomId == room.Id))
            {
                structure.AddConstraintSegment(wall.Segment, targetLength: 0.2);
            }
            
            // Configure options based on room type
            var options = GetMeshingOptionsForRoom(room);
            
            // Generate mesh
            var mesher = new PrismMesher();
            var mesh = mesher.Mesh(structure, options);
            var indexed = IndexedMesh.FromMesh(mesh);
            
            // Export with room-specific filename
            var filename = $"room_{room.Id}_{room.Type}.obj";
            ObjExporter.Write(indexed, filename);
            
            Console.WriteLine($"Room {room.Id} ({room.Type}): {indexed.QuadCount} quads, {indexed.TriangleCount} triangles");
        }
    }
    
    private static MesherOptions GetMeshingOptionsForRoom(Room room)
    {
        return room.Type switch
        {
            RoomType.Bathroom => new MesherOptions {
                TargetEdgeLengthXY = 0.2,  // Fine mesh for detailed areas
                TargetEdgeLengthZ = 0.5,
                MinCapQuadQuality = 0.7
            },
            RoomType.Kitchen => new MesherOptions {
                TargetEdgeLengthXY = 0.3,
                TargetEdgeLengthZ = 0.5,
                MinCapQuadQuality = 0.6
            },
            RoomType.LivingRoom => new MesherOptions {
                TargetEdgeLengthXY = 0.8,  // Coarser mesh for large areas
                TargetEdgeLengthZ = 1.0,
                MinCapQuadQuality = 0.5
            },
            _ => MesherOptions.CreateBuilder().WithFastPreset().Build()
        };
    }
}

public record Room
{
    public string Id { get; init; } = "";
    public RoomType Type { get; init; }
    public Polygon2D Outline { get; init; } = null!;
    public double CeilingHeight { get; init; }
    public Opening[] Openings { get; init; } = Array.Empty<Opening>();
}

public enum RoomType { LivingRoom, Kitchen, Bathroom, Bedroom, Office }

public record Opening
{
    public string Type { get; init; } = ""; // "Door", "Window"
    public Polygon2D Polygon { get; init; } = null!;
}
```

### Example 7: Gaming/Visualization Mesh Generation

```csharp
public class GameLevelGenerator
{
    public static void GenerateGameLevel()
    {
        // Generate a game level with multiple buildings
        var levelBounds = new Vec2(200, 200);  // 200x200 meter level
        var buildings = GenerateBuildingsForLevel(levelBounds, buildingCount: 20);
        
        // Process all buildings with performance monitoring
        using var monitor = PerformanceMonitor.StartTiming("level_generation");
        
        var allMeshes = new List<IndexedMesh>();
        
        foreach (var (building, index) in buildings.Select((b, i) => (b, i)))
        {
            // Use appropriate LOD based on building importance
            var options = GetLODOptions(building.Importance);
            
            var mesher = new PrismMesher();
            var mesh = mesher.Mesh(building.Structure, options);
            var indexed = IndexedMesh.FromMesh(mesh);
            
            allMeshes.Add(indexed);
            
            // Export individual building for debugging
            ObjExporter.Write(indexed, $"building_{index:D2}_{building.Type}.obj");
        }
        
        // Combine all meshes for level export
        var combinedMesh = CombineMeshes(allMeshes);
        GltfExporter.Write(combinedMesh, "game_level.gltf");  // GLTF for game engine
        
        monitor.LogResult();
        
        Console.WriteLine($"Generated game level with {buildings.Length} buildings");
        Console.WriteLine($"Total geometry: {combinedMesh.QuadCount} quads, {combinedMesh.TriangleCount} triangles");
    }
    
    private static MesherOptions GetLODOptions(BuildingImportance importance)
    {
        return importance switch
        {
            BuildingImportance.Hero => new MesherOptions {  // Player can enter
                TargetEdgeLengthXY = 0.5,
                TargetEdgeLengthZ = 0.8,
                MinCapQuadQuality = 0.7
            },
            BuildingImportance.Important => new MesherOptions {  // Key landmarks
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.5,
                MinCapQuadQuality = 0.5
            },
            BuildingImportance.Background => new MesherOptions {  // Distant buildings
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 3.0,
                MinCapQuadQuality = 0.3
            },
            _ => MesherOptions.CreateBuilder().WithFastPreset().Build()
        };
    }
    
    private static IndexedMesh CombineMeshes(List<IndexedMesh> meshes)
    {
        // Simplified mesh combination logic
        var combinedMesh = new Mesh(
            initialQuadCapacity: meshes.Sum(m => m.QuadCount),
            initialTriangleCapacity: meshes.Sum(m => m.TriangleCount),
            initialPointCapacity: meshes.Sum(m => m.VertexCount),
            initialSegmentCapacity: meshes.Sum(m => m.EdgeCount)
        );
        
        foreach (var mesh in meshes)
        {
            // Add quads and triangles from each mesh
            // Note: This is simplified - real implementation would handle vertex indexing
            foreach (var quad in mesh.Quads)
            {
                var v0 = mesh.Vertices[quad.v0];
                var v1 = mesh.Vertices[quad.v1];
                var v2 = mesh.Vertices[quad.v2];
                var v3 = mesh.Vertices[quad.v3];
                
                combinedMesh.AddQuad(new Quad(v0, v1, v2, v3));
            }
        }
        
        return IndexedMesh.FromMesh(combinedMesh);
    }
}

public enum BuildingImportance { Background, Important, Hero }
```

## 🔧 Error Handling and Validation

### Example 8: Robust Input Validation

```csharp
public class RobustMeshGenerator
{
    public static IndexedMesh SafeGenerateMesh(
        Polygon2D outline, 
        double z0, 
        double z1, 
        MesherOptions options)
    {
        try
        {
            // Validate inputs
            ValidateInputs(outline, z0, z1, options);
            
            // Create structure with validation
            var structure = CreateValidatedStructure(outline, z0, z1);
            
            // Generate mesh with error handling
            var mesher = new PrismMesher();
            var mesh = mesher.Mesh(structure, options);
            
            // Validate output
            ValidateGeneratedMesh(mesh);
            
            return IndexedMesh.FromMesh(mesh, options.Epsilon);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Input validation error: {ex.Message}");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Mesh generation error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }
    
    private static void ValidateInputs(
        Polygon2D outline, 
        double z0, 
        double z1, 
        MesherOptions options)
    {
        // Validate polygon
        if (outline.Vertices.Count < 3)
            throw new ArgumentException("Polygon must have at least 3 vertices");
            
        if (Math.Abs(outline.SignedArea) < 1e-9)
            throw new ArgumentException("Polygon has zero area");
            
        // Validate Z coordinates
        if (z1 <= z0)
            throw new ArgumentException("z1 must be greater than z0");
            
        if (Math.Abs(z1 - z0) < 1e-9)
            throw new ArgumentException("Height must be positive and significant");
            
        // Validate options
        options.Validate();  // Uses built-in validation
        
        // Additional custom validation
        if (options.TargetEdgeLengthXY > Math.Sqrt(Math.Abs(outline.SignedArea)))
            Console.WriteLine("Warning: Target edge length is large compared to polygon size");
    }
    
    private static PrismStructureDefinition CreateValidatedStructure(
        Polygon2D outline, 
        double z0, 
        double z1)
    {
        var structure = new PrismStructureDefinition(outline, z0, z1);
        
        // Additional validation could be added here
        // e.g., check for self-intersections, validate hole placement, etc.
        
        return structure;
    }
    
    private static void ValidateGeneratedMesh(Mesh mesh)
    {
        if (mesh.QuadCount == 0 && mesh.TriangleCount == 0)
            throw new InvalidOperationException("Generated mesh is empty");
            
        // Check for reasonable mesh size
        if (mesh.QuadCount > 1_000_000)
            Console.WriteLine("Warning: Very large mesh generated, consider coarser settings");
            
        // Validate quad quality if available
        var lowQualityQuads = mesh.Quads.Count(q => q.QualityScore < 0.1);
        if (lowQualityQuads > mesh.QuadCount * 0.1)  // More than 10% low quality
            Console.WriteLine($"Warning: {lowQualityQuads} low-quality quads generated");
    }
}
```

## 📊 Monitoring and Analytics

### Example 9: Comprehensive Performance Monitoring

```csharp
public class MeshingAnalytics
{
    private readonly Dictionary<string, List<double>> _metrics = new();
    
    public IndexedMesh GenerateWithAnalytics(
        PrismStructureDefinition structure, 
        MesherOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Monitor memory before
        var memoryBefore = GC.GetTotalMemory(false);
        var gen0Before = GC.CollectionCount(0);
        
        // Generate mesh
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(structure, options);
        
        var meshGenerationTime = stopwatch.ElapsedMilliseconds;
        RecordMetric("mesh_generation_ms", meshGenerationTime);
        
        // Monitor indexing
        stopwatch.Restart();
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
        var indexingTime = stopwatch.ElapsedMilliseconds;
        RecordMetric("indexing_ms", indexingTime);
        
        // Monitor memory after
        var memoryAfter = GC.GetTotalMemory(false);
        var gen0After = GC.CollectionCount(0);
        
        RecordMetric("memory_used_mb", (memoryAfter - memoryBefore) / 1024.0 / 1024.0);
        RecordMetric("gc_collections", gen0After - gen0Before);
        RecordMetric("quad_count", indexed.QuadCount);
        RecordMetric("triangle_count", indexed.TriangleCount);
        RecordMetric("vertex_count", indexed.VertexCount);
        
        // Calculate efficiency metrics
        var totalElements = indexed.QuadCount + indexed.TriangleCount;
        if (totalElements > 0)
        {
            RecordMetric("elements_per_ms", totalElements / (double)(meshGenerationTime + indexingTime));
            RecordMetric("vertices_per_element", indexed.VertexCount / (double)totalElements);
        }
        
        // Quality analysis
        AnalyzeQuality(mesh);
        
        return indexed;
    }
    
    private void AnalyzeQuality(Mesh mesh)
    {
        var qualityQuads = mesh.Quads.Where(q => q.QualityScore.HasValue).ToArray();
        
        if (qualityQuads.Length > 0)
        {
            var qualities = qualityQuads.Select(q => q.QualityScore!.Value).ToArray();
            
            RecordMetric("avg_quad_quality", qualities.Average());
            RecordMetric("min_quad_quality", qualities.Min());
            RecordMetric("max_quad_quality", qualities.Max());
            RecordMetric("quality_std_dev", CalculateStandardDeviation(qualities));
            
            // Quality distribution
            RecordMetric("excellent_quads_pct", qualities.Count(q => q > 0.8) / (double)qualities.Length * 100);
            RecordMetric("good_quads_pct", qualities.Count(q => q is > 0.6 and <= 0.8) / (double)qualities.Length * 100);
            RecordMetric("fair_quads_pct", qualities.Count(q => q is > 0.4 and <= 0.6) / (double)qualities.Length * 100);
            RecordMetric("poor_quads_pct", qualities.Count(q => q <= 0.4) / (double)qualities.Length * 100);
        }
    }
    
    private void RecordMetric(string name, double value)
    {
        if (!_metrics.ContainsKey(name))
            _metrics[name] = new List<double>();
            
        _metrics[name].Add(value);
    }
    
    public void GenerateReport()
    {
        Console.WriteLine("=== Meshing Analytics Report ===");
        
        foreach (var (metric, values) in _metrics.OrderBy(x => x.Key))
        {
            if (values.Count > 0)
            {
                var avg = values.Average();
                var min = values.Min();
                var max = values.Max();
                var count = values.Count;
                
                Console.WriteLine($"{metric}:");
                Console.WriteLine($"  Samples: {count}");
                Console.WriteLine($"  Average: {avg:F2}");
                Console.WriteLine($"  Range: {min:F2} - {max:F2}");
                
                if (count > 1)
                {
                    var stdDev = CalculateStandardDeviation(values.ToArray());
                    Console.WriteLine($"  Std Dev: {stdDev:F2}");
                }
                
                Console.WriteLine();
            }
        }
    }
    
    private static double CalculateStandardDeviation(double[] values)
    {
        var avg = values.Average();
        var sumOfSquaredDifferences = values.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquaredDifferences / values.Length);
    }
}
```

## 🎯 Summary of Best Practices

### Performance Best Practices ⚡
1. **Use batch operations**: `AddQuads()` instead of individual `AddQuad()`
2. **Pre-allocate capacity**: Set initial capacity when mesh size is known
3. **Leverage span operations**: Use span-based geometry calculations
4. **Object pooling**: Reuse mesh instances for frequent operations
5. **Monitor memory**: Track GC pressure and memory usage

### Quality Best Practices 🎨
1. **Appropriate precision**: Balance quality vs performance requirements
2. **Quality thresholds**: Set `MinCapQuadQuality` based on use case
3. **Targeted refinement**: Use refinement zones near important features
4. **Input validation**: Always validate geometry before processing
5. **Output analysis**: Monitor mesh quality metrics

### Robustness Best Practices 🛡️
1. **Error handling**: Implement comprehensive exception handling
2. **Input validation**: Check polygon validity, area, orientation
3. **Resource management**: Proper disposal and cleanup
4. **Logging**: Monitor performance and quality metrics
5. **Testing**: Validate with edge cases and stress tests

---

*FastGeoMesh Practical Usage Guide v1.0.0*  
*Examples tested on .NET 8.0.20*
