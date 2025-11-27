using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace FastGeoMesh.Benchmarks
{
    public sealed class BenchmarkDotNetConfig : ManualConfig
    {
        public BenchmarkDotNetConfig()
        {
            Add(Job.Default.WithRuntime(BenchmarkDotNet.Environments.Runtime.Core).WithWarmupCount(3).WithIterationCount(5));
            Add(MemoryDiagnoser.Default);
        }
    }
}
