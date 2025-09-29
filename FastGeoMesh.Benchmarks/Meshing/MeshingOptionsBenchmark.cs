using BenchmarkDotNet.Attributes;
using FastGeoMesh.Meshing;

namespace FastGeoMesh.Benchmarks.Meshing;

/// <summary>
/// Benchmarks for MesherOptions creation and validation comparing different patterns.
/// Tests the impact of builder pattern vs direct instantiation and validation performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class MeshingOptionsBenchmark
{
    private const int IterationCount = 10000;

    [Benchmark(Baseline = true)]
    public MesherOptions CreateOptions_BuilderPattern()
    {
        return MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.5)
            .WithTargetEdgeLengthZ(1.0)
            .WithCaps(bottom: true, top: true)
            .WithHoleRefinement(0.5, 2.0)
            .WithSegmentRefinement(0.3, 1.5)
            .WithMinCapQuadQuality(0.6)
            .WithRejectedCapTriangles(true)
            .Build();
    }

    [Benchmark]
    public MesherOptions CreateOptions_DirectInstantiation()
    {
        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 1.5,
            TargetEdgeLengthZ = 1.0,
            GenerateBottomCap = true,
            GenerateTopCap = true,
            TargetEdgeLengthXYNearHoles = 0.5,
            HoleRefineBand = 2.0,
            TargetEdgeLengthXYNearSegments = 0.3,
            SegmentRefineBand = 1.5,
            MinCapQuadQuality = 0.6,
            OutputRejectedCapTriangles = true
        };
        options.Validate();
        return options;
    }

    [Benchmark]
    public MesherOptions CreateOptions_FastPreset()
    {
        return MesherOptions.CreateBuilder()
            .WithFastPreset()
            .Build();
    }

    [Benchmark]
    public MesherOptions CreateOptions_HighQualityPreset()
    {
        return MesherOptions.CreateBuilder()
            .WithHighQualityPreset()
            .Build();
    }

    [Benchmark]
    public MesherOptions[] CreateManyOptions_Builder()
    {
        var results = new MesherOptions[IterationCount];
        
        for (int i = 0; i < IterationCount; i++)
        {
            double scale = 1.0 + (i % 10) * 0.1;
            results[i] = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(scale)
                .WithTargetEdgeLengthZ(scale * 0.8)
                .WithMinCapQuadQuality(0.3 + (i % 7) * 0.1)
                .Build();
        }
        return results;
    }

    [Benchmark]
    public MesherOptions[] CreateManyOptions_Direct()
    {
        var results = new MesherOptions[IterationCount];
        
        for (int i = 0; i < IterationCount; i++)
        {
            double scale = 1.0 + (i % 10) * 0.1;
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = scale,
                TargetEdgeLengthZ = scale * 0.8,
                MinCapQuadQuality = 0.3 + (i % 7) * 0.1
            };
            options.Validate();
            results[i] = options;
        }
        return results;
    }

    [Benchmark]
    public bool[] ValidateOptions_Valid()
    {
        var results = new bool[IterationCount];
        
        for (int i = 0; i < IterationCount; i++)
        {
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0 + (i % 5) * 0.1,
                TargetEdgeLengthZ = 1.0,
                MinCapQuadQuality = 0.3 + (i % 7) * 0.1,
                Epsilon = 1e-9
            };
            
            try
            {
                options.Validate();
                results[i] = true;
            }
            catch
            {
                results[i] = false;
            }
        }
        return results;
    }

    [Benchmark]
    public bool[] ValidateOptions_Mixed()
    {
        var results = new bool[IterationCount];
        
        for (int i = 0; i < IterationCount; i++)
        {
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = (i % 10 == 0) ? -1.0 : 1.0 + (i % 5) * 0.1, // Some invalid
                TargetEdgeLengthZ = 1.0,
                MinCapQuadQuality = (i % 15 == 0) ? 1.5 : 0.3 + (i % 7) * 0.1, // Some invalid
                Epsilon = (i % 20 == 0) ? -1e-9 : 1e-9 // Some invalid
            };
            
            try
            {
                options.Validate();
                results[i] = true;
            }
            catch
            {
                results[i] = false;
            }
        }
        return results;
    }

    [Benchmark]
    public MesherOptions CreateOptionsWithRefinement()
    {
        return MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(2.0)
            .WithTargetEdgeLengthZ(1.5)
            .WithHoleRefinement(0.5, 3.0)
            .WithSegmentRefinement(0.3, 2.0)
            .WithMinCapQuadQuality(0.7)
            .WithRejectedCapTriangles(true)
            .WithEpsilon(1e-12)
            .Build();
    }

    private MesherOptions _options = null!;
    private MesherOptionsBuilder _builder = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = new MesherOptions
        {
            TargetEdgeLengthXY = 1.0,
            TargetEdgeLengthZ = 1.5,
            GenerateBottomCap = true,
            GenerateTopCap = true,
            MinCapQuadQuality = 0.5
        };
        _builder = MesherOptions.CreateBuilder();
    }

    [Benchmark(Baseline = true)]
    public void Validate_WithCaching()
    {
        // First call does validation, subsequent calls are cached
        _options.Validate();
        _options.Validate();
        _options.Validate();
    }

    [Benchmark]
    public void Validate_ForcedRevalidation()
    {
        // Force revalidation every time (worst case)
        _options.ResetValidation();
        _options.Validate();
        _options.ResetValidation();
        _options.Validate();
        _options.ResetValidation();
        _options.Validate();
    }

    [Benchmark]
    public MesherOptions BuilderPattern_Simple()
    {
        return MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.0)
            .WithTargetEdgeLengthZ(1.5)
            .WithMinCapQuadQuality(0.5)
            .Build();
    }

    [Benchmark]
    public MesherOptions BuilderPattern_Complex()
    {
        return MesherOptions.CreateBuilder()
            .WithTargetEdgeLengthXY(1.0)
            .WithTargetEdgeLengthZ(1.5)
            .WithCaps(true, true)
            .WithEpsilon(1e-9)
            .WithHoleRefinement(0.5, 2.0)
            .WithSegmentRefinement(0.3, 1.5)
            .WithMinCapQuadQuality(0.6)
            .WithRejectedCapTriangles(true)
            .Build();
    }

    [Benchmark]
    public MesherOptions BuilderPattern_HighQualityPreset()
    {
        return MesherOptions.CreateBuilder()
            .WithHighQualityPreset()
            .Build();
    }

    [Benchmark]
    public MesherOptions BuilderPattern_FastPreset()
    {
        return MesherOptions.CreateBuilder()
            .WithFastPreset()
            .Build();
    }

    [Benchmark]
    public MesherOptions DirectConstruction()
    {
        var options = new MesherOptions
        {
            TargetEdgeLengthXY = 1.0,
            TargetEdgeLengthZ = 1.5,
            GenerateBottomCap = true,
            GenerateTopCap = true,
            MinCapQuadQuality = 0.5
        };
        options.Validate();
        return options;
    }

    [Benchmark]
    public void ValidationOverhead_RepeatedCalls()
    {
        // Simulate multiple validation calls in hot path
        for (int i = 0; i < 100; i++)
        {
            _options.Validate(); // Should be very fast due to caching
        }
    }

    [Benchmark]
    public void PropertyChanges_WithRevalidation()
    {
        // Simulate changing properties and revalidating
        _options.TargetEdgeLengthXY = 0.8;
        _options.ResetValidation();
        _options.Validate();
        
        _options.MinCapQuadQuality = 0.7;
        _options.ResetValidation();
        _options.Validate();
        
        _options.TargetEdgeLengthZ = 2.0;
        _options.ResetValidation();
        _options.Validate();
    }
}
