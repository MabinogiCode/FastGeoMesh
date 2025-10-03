using BenchmarkDotNet.Filters;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>
    /// Category filter for BenchmarkDotNet to run specific benchmark groups.
    /// </summary>
    public class CategoryFilter : IFilter
    {
        private readonly string _category;

        public CategoryFilter(string category)
        {
            _category = category;
        }

        public bool Predicate(BenchmarkCase benchmarkCase)
        {
            return benchmarkCase.Descriptor.Categories.Contains(_category, StringComparer.OrdinalIgnoreCase);
        }
    }
}
