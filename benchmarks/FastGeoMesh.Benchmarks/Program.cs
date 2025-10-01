using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using FastGeoMesh.Performance;

namespace FastGeoMesh.Benchmarks;

public class BenchmarkProgram
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "quick")
        {
            // Run quick performance test
            QuickPerformanceTest.RunComparison();
            return;
        }

        Console.WriteLine("🚀 FastGeoMesh Performance Analysis");
        Console.WriteLine("=====================================");
        Console.WriteLine();
        Console.WriteLine("Analyzing performance improvements from recent optimizations:");
        Console.WriteLine("• Thread safety optimization (ReaderWriterLockSlim → simple lock)");
        Console.WriteLine("• Lazy collection caching enhancements");
        Console.WriteLine("• Direct count property access");
        Console.WriteLine();

        var config = DefaultConfig.Instance
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddDiagnoser(ThreadingDiagnoser.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .AddLogger(ConsoleLogger.Default);

        Console.WriteLine("📊 Running Direct Comparison Benchmarks (Old vs New)...");
        var summary = BenchmarkRunner.Run<DirectComparisonBenchmarks>(config);

        Console.WriteLine();
        Console.WriteLine("✅ Performance analysis completed!");
        Console.WriteLine();
        Console.WriteLine("📈 Key improvements measured:");
        Console.WriteLine("• Thread safety performance");
        Console.WriteLine("• Memory allocation patterns");
        Console.WriteLine("• Collection access optimization");
        Console.WriteLine();
        Console.WriteLine("📁 Results available in BenchmarkDotNet.Artifacts folder");
    }
}
