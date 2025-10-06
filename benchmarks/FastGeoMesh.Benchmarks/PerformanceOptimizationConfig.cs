using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>
    /// Custom benchmark configuration for performance optimization testing.
    /// </summary>
    public class PerformanceOptimizationConfig : ManualConfig
    {
        public PerformanceOptimizationConfig()
        {
            AddJob(Job.Default
                .WithRuntime(CoreRuntime.Core80)
                .WithPlatform(Platform.X64)
                .WithJit(Jit.RyuJit)
                .WithGcMode(new GcMode { Server = true })
                .WithId("FastGeoMesh_v1.4.0_Optimized"));

            AddDiagnoser(MemoryDiagnoser.Default);
            AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 3)));
            
            AddExporter(DefaultExporters.Html);
            AddExporter(DefaultExporters.Markdown);
            AddExporter(DefaultExporters.Csv);

            AddColumnProvider(DefaultColumnProviders.Descriptor);
            AddColumnProvider(DefaultColumnProviders.Job);
            AddColumnProvider(DefaultColumnProviders.Statistics);
            AddColumnProvider(DefaultColumnProviders.Params);
            
            WithSummaryStyle(BenchmarkDotNet.Reports.SummaryStyle.Default
                .WithRatioStyle(RatioStyle.Trend)
                .WithSizeUnit(SizeUnit.KB)
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Microsecond));
        }
    }
}
