using BenchmarkDotNet.Attributes;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Benchmarks.Meshing;

/// <summary>
/// Benchmarks for prism meshing operations comparing different configurations and optimizations.
/// Tests the impact of meshing options and structural complexity on performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class PrismMeshingBenchmark
{
    private PrismStructureDefinition _simpleStructure = null!;
    private PrismStructureDefinition _complexStructure = null!;
    private PrismStructureDefinition _structureWithHoles = null!;
    private MesherOptions _fastOptions = null!;
    private MesherOptions _highQualityOptions = null!;
    private PrismMesher _mesher = null!;

    [GlobalSetup]
    public void Setup()
    {
        _mesher = new PrismMesher();
        
        // Simple rectangular structure
        var simplePolygon = Polygon2D.FromPoints(new[]
        {
            new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10)
        });
        _simpleStructure = new PrismStructureDefinition(simplePolygon, 0, 5);

        // Complex star-shaped structure
        var complexVertices = new List<Vec2>();
        for (int i = 0; i < 16; i++)
        {
            double angle = 2 * Math.PI * i / 16;
            double radius = (i % 2 == 0) ? 25 : 15; // Star shape
            complexVertices.Add(new Vec2(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle)
            ));
        }
        var complexPolygon = Polygon2D.FromPoints(complexVertices);
        _complexStructure = new PrismStructureDefinition(complexPolygon, -2, 8);

        // Structure with holes
        var outerPolygon = Polygon2D.FromPoints(new[]
        {
            new Vec2(-30, -30), new Vec2(30, -30), new Vec2(30, 30), new Vec2(-30, 30)
        });
        _structureWithHoles = new PrismStructureDefinition(outerPolygon, 0, 10);
        
        // Add multiple holes
        var hole1 = Polygon2D.FromPoints(new[]
        {
            new Vec2(-10, -10), new Vec2(-5, -10), new Vec2(-5, -5), new Vec2(-10, -5)
        });
        var hole2 = Polygon2D.FromPoints(new[]
        {
            new Vec2(5, 5), new Vec2(10, 5), new Vec2(10, 10), new Vec2(5, 10)
        });
        _structureWithHoles.AddHole(hole1);
        _structureWithHoles.AddHole(hole2);

        // Meshing options
        _fastOptions = MesherOptions.CreateBuilder()
            .WithFastPreset()
            .Build();

        _highQualityOptions = MesherOptions.CreateBuilder()
            .WithHighQualityPreset()
            .WithHoleRefinement(0.3, 2.0)
            .WithSegmentRefinement(0.3, 1.5)
            .Build();
    }

    [Benchmark(Baseline = true)]
    public int SimpleMeshing_FastOptions()
    {
        var mesh = _mesher.Mesh(_simpleStructure, _fastOptions);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int SimpleMeshing_HighQualityOptions()
    {
        var mesh = _mesher.Mesh(_simpleStructure, _highQualityOptions);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int ComplexMeshing_FastOptions()
    {
        var mesh = _mesher.Mesh(_complexStructure, _fastOptions);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int ComplexMeshing_HighQualityOptions()
    {
        var mesh = _mesher.Mesh(_complexStructure, _highQualityOptions);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingWithHoles_FastOptions()
    {
        var mesh = _mesher.Mesh(_structureWithHoles, _fastOptions);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingWithHoles_HighQualityOptions()
    {
        var mesh = _mesher.Mesh(_structureWithHoles, _highQualityOptions);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingWithRefinement_HoleRefinement()
    {
        var options = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(2.0)
            .WithTargetEdgeLengthZ(1.0)
            .WithHoleRefinement(0.5, 3.0)
            .Build();

        var mesh = _mesher.Mesh(_structureWithHoles, options);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingCapsOnly_BottomCap()
    {
        var options = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.0)
            .WithCaps(bottom: true, top: false)
            .Build();

        var mesh = _mesher.Mesh(_complexStructure, options);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingCapsOnly_TopCap()
    {
        var options = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.0)
            .WithCaps(bottom: false, top: true)
            .Build();

        var mesh = _mesher.Mesh(_complexStructure, options);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingCapsOnly_BothCaps()
    {
        var options = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.0)
            .WithCaps(bottom: true, top: true)
            .Build();

        var mesh = _mesher.Mesh(_complexStructure, options);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }

    [Benchmark]
    public int MeshingWithTriangleOutput()
    {
        var options = MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.0)
            .WithMinCapQuadQuality(0.8) // High threshold to force triangle output
            .WithRejectedCapTriangles(true)
            .Build();

        var mesh = _mesher.Mesh(_complexStructure, options);
        return mesh.Quads.Count + mesh.Triangles.Count;
    }
}
