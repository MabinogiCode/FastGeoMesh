using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Ordering;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FastGeoMesh.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class SpatialPolygonIndexBenchmarks
    {
        private SpatialPolygonIndex _legacyIndex; // created with new GeometryHelperImpl if legacy existed
        private SpatialPolygonIndex _injectedIndex;
        private readonly List<Vec2> _vertices;
        private readonly IGeometryHelper _helper;
        private readonly List<(double x, double y)> _points;

        public SpatialPolygonIndexBenchmarks()
        {
            // Build a moderately complex polygon
            _vertices = new List<Vec2>();
            int n = 200;
            for (int i = 0; i < n; i++)
            {
                double angle = 2 * System.Math.PI * i / n;
                double radius = 50 + 10 * System.Math.Sin(6 * angle);
                _vertices.Add(new Vec2(radius * System.Math.Cos(angle), radius * System.Math.Sin(angle)));
            }

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            _helper = provider.GetRequiredService<IGeometryHelper>();

            // Create many random points to query
            var rnd = new Random(123);
            _points = new List<(double x, double y)>(10000);
            for (int i = 0; i < 10000; i++)
            {
                _points.Add((rnd.NextDouble() * 200 - 100, rnd.NextDouble() * 200 - 100));
            }

            // Create indexes
            _injectedIndex = new SpatialPolygonIndex(_vertices, _helper, gridResolution: 64);
            // Legacy index removed - instead create a second index that would mimic not reusing helper by creating a new helper instance
            _legacyIndex = new SpatialPolygonIndex(_vertices, new GeometryHelperImpl(new GeometryConfigImpl()), gridResolution: 64);
        }

        [Benchmark(Baseline = true)]
        public int InjectedHelperIsInsideAllPoints()
        {
            int cnt = 0;
            foreach (var (x, y) in _points)
            {
                if (_injectedIndex.IsInside(x, y)) cnt++;
            }
            return cnt;
        }

        [Benchmark]
        public int NewHelperInstanceIsInsideAllPoints()
        {
            int cnt = 0;
            foreach (var (x, y) in _points)
            {
                if (_legacyIndex.IsInside(x, y)) cnt++;
            }
            return cnt;
        }
    }
}
