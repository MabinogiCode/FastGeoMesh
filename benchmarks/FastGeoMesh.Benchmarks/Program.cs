using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Filters;
using FastGeoMesh.Benchmarks;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>
    /// FastGeoMesh Performance Benchmarks Runner
    /// Tests v1.4.0 performance optimizations and improvements.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("🚀 FastGeoMesh v1.4.0 Performance Benchmarks");
            Console.WriteLine("============================================");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var config = DefaultConfig.Instance;

            switch (args[0].ToLowerInvariant())
            {
                case "--all":
                    Console.WriteLine("Running ALL v1.4.0 performance benchmarks...");
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(config);
                    break;

                case "--sync-vs-async":
                    Console.WriteLine("Running Sync vs Async comparison benchmarks...");
                    var syncAsyncConfig = config.AddFilter(new CategoryFilter("SyncVsAsync"));
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(syncAsyncConfig);
                    break;

                case "--batch":
                    Console.WriteLine("Running Batch Processing performance benchmarks...");
                    var batchConfig = config.AddFilter(new CategoryFilter("Batch"));
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(batchConfig);
                    break;

                case "--monitoring":
                    Console.WriteLine("Running Performance Monitoring overhead benchmarks...");
                    var monitoringConfig = config.AddFilter(new CategoryFilter("Monitoring"));
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(monitoringConfig);
                    break;

                case "--optimization":
                    Console.WriteLine("Running Optimization validation benchmarks...");
                    var optimizationConfig = config.AddFilter(new CategoryFilter("Optimization"));
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(optimizationConfig);
                    break;

                case "--progress":
                    Console.WriteLine("Running Progress Reporting overhead benchmarks...");
                    var progressConfig = config.AddFilter(new CategoryFilter("Progress"));
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(progressConfig);
                    break;

                case "--trivial":
                    Console.WriteLine("Running Trivial Structure optimization benchmarks...");
                    var trivialConfig = config.AddFilter(new CategoryFilter("Trivial"));
                    BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(trivialConfig);
                    break;

                case "--quick":
                    Console.WriteLine("Running Quick Performance Check (trivial + simple)...");
                    RunQuickPerformanceCheck();
                    break;

                case "--validate":
                    Console.WriteLine("Running Validation Suite (correctness + performance)...");
                    RunValidationSuite();
                    break;

                default:
                    Console.WriteLine($"Unknown option: {args[0]}");
                    ShowUsage();
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("✅ Benchmarks completed! Check BenchmarkDotNet.Artifacts for detailed results.");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: FastGeoMesh.Benchmarks [option]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --all               Run all v1.4.0 performance benchmarks");
            Console.WriteLine("  --sync-vs-async     Compare sync vs async performance");
            Console.WriteLine("  --batch             Test batch processing performance");
            Console.WriteLine("  --monitoring        Test performance monitoring overhead");
            Console.WriteLine("  --optimization      Validate optimization effectiveness");
            Console.WriteLine("  --progress          Test progress reporting overhead");
            Console.WriteLine("  --trivial           Test trivial structure optimizations");
            Console.WriteLine("  --quick             Quick performance check (recommended)");
            Console.WriteLine("  --validate          Full validation suite");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  FastGeoMesh.Benchmarks --quick");
            Console.WriteLine("  FastGeoMesh.Benchmarks --sync-vs-async");
            Console.WriteLine("  FastGeoMesh.Benchmarks --batch");
        }

        private static void RunQuickPerformanceCheck()
        {
            Console.WriteLine("Running quick performance validation...");
            
            var config = DefaultConfig.Instance
                .AddFilter(new CategoryFilter("Trivial"))
                .AddFilter(new CategoryFilter("Simple"));
                
            BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(config);
        }

        private static void RunValidationSuite()
        {
            Console.WriteLine("🔍 Running comprehensive validation suite...");
            Console.WriteLine();

            // Test core optimizations
            Console.WriteLine("1. Testing Sync vs Async optimization...");
            var syncAsyncConfig = DefaultConfig.Instance.AddFilter(new CategoryFilter("SyncVsAsync"));
            var syncAsyncSummary = BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(syncAsyncConfig);

            Console.WriteLine("2. Testing Batch processing optimization...");
            var batchConfig = DefaultConfig.Instance.AddFilter(new CategoryFilter("Batch"));
            var batchSummary = BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(batchConfig);

            Console.WriteLine("3. Testing Performance monitoring overhead...");
            var monitoringConfig = DefaultConfig.Instance.AddFilter(new CategoryFilter("Monitoring"));
            var monitoringSummary = BenchmarkRunner.Run<V14PerformanceOptimizationsBenchmarks>(monitoringConfig);

            Console.WriteLine();
            Console.WriteLine("📊 Validation Summary:");
            Console.WriteLine($"   Sync/Async tests: {syncAsyncSummary.Reports.Count} benchmarks");
            Console.WriteLine($"   Batch tests: {batchSummary.Reports.Count} benchmarks");
            Console.WriteLine($"   Monitoring tests: {monitoringSummary.Reports.Count} benchmarks");
            Console.WriteLine();
            Console.WriteLine("✅ Comprehensive validation completed!");
        }
    }
}
