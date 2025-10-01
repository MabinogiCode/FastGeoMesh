using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>
    /// Direct comparison between old and new Mesh implementations.
    /// Measures the impact of thread safety improvements.
    /// </summary>
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
    public class DirectComparisonBenchmarks
    {
        private List<Quad> _testQuads = null!;
        private List<Triangle> _testTriangles = null!;

        [GlobalSetup]
        public void Setup()
        {
            _testQuads = new List<Quad>();
            _testTriangles = new List<Triangle>();

            var random = new Random(42);

            // Generate 1000 test quads
            for (int i = 0; i < 1000; i++)
            {
                var v0 = new Vec3(random.NextDouble() * 100, random.NextDouble() * 100, random.NextDouble() * 10);
                var v1 = new Vec3(v0.X + 1, v0.Y, v0.Z);
                var v2 = new Vec3(v0.X + 1, v0.Y + 1, v0.Z);
                var v3 = new Vec3(v0.X, v0.Y + 1, v0.Z);

                _testQuads.Add(new Quad(v0, v1, v2, v3));
            }

            // Generate 500 test triangles
            for (int i = 0; i < 500; i++)
            {
                var v0 = new Vec3(random.NextDouble() * 100, random.NextDouble() * 100, random.NextDouble() * 10);
                var v1 = new Vec3(v0.X + 1, v0.Y, v0.Z);
                var v2 = new Vec3(v0.X + 0.5, v0.Y + 1, v0.Z);

                _testTriangles.Add(new Triangle(v0, v1, v2));
            }
        }

        // SEQUENTIAL ADDITION BENCHMARKS

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("SequentialAddition")]
        public void OldImplementation_AddQuads_Sequential()
        {
            using var mesh = new OldMeshImplementation();
            foreach (var quad in _testQuads)
            {
                mesh.AddQuad(quad);
            }
        }

        [Benchmark]
        [BenchmarkCategory("SequentialAddition")]
        public void NewImplementation_AddQuads_Sequential()
        {
            using var mesh = new Mesh();
            foreach (var quad in _testQuads)
            {
                mesh.AddQuad(quad);
            }
        }

        [Benchmark]
        [BenchmarkCategory("SequentialAddition")]
        public void NewImplementation_AddQuads_Batch()
        {
            using var mesh = new Mesh();
            mesh.AddQuads(_testQuads);
        }

        // COLLECTION ACCESS BENCHMARKS

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("CollectionAccess")]
        public int OldImplementation_MultipleAccess()
        {
            using var mesh = new OldMeshImplementation();
            mesh.AddQuads(_testQuads);
            mesh.AddTriangles(_testTriangles);

            int total = 0;

            // Simulate multiple accesses to collections
            for (int i = 0; i < 100; i++)
            {
                total += mesh.Quads.Count;
                total += mesh.Triangles.Count;
            }

            return total;
        }

        [Benchmark]
        [BenchmarkCategory("CollectionAccess")]
        public int NewImplementation_MultipleAccess()
        {
            using var mesh = new Mesh();
            mesh.AddQuads(_testQuads);
            mesh.AddTriangles(_testTriangles);

            int total = 0;

            // Simulate multiple accesses to collections
            for (int i = 0; i < 100; i++)
            {
                total += mesh.Quads.Count;
                total += mesh.Triangles.Count;
            }

            return total;
        }

        [Benchmark]
        [BenchmarkCategory("CollectionAccess")]
        public int NewImplementation_OptimizedCountAccess()
        {
            using var mesh = new Mesh();
            mesh.AddQuads(_testQuads);
            mesh.AddTriangles(_testTriangles);

            int total = 0;

            // Use optimized count properties
            for (int i = 0; i < 100; i++)
            {
                total += mesh.QuadCount;
                total += mesh.TriangleCount;
            }

            return total;
        }

        // MIXED OPERATION BENCHMARKS

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("MixedOperations")]
        public void OldImplementation_MixedReadWrite()
        {
            using var mesh = new OldMeshImplementation();

            // Add some initial data
            foreach (var quad in _testQuads.Take(100))
            {
                mesh.AddQuad(quad);
            }

            // Mix reads and writes
            for (int i = 0; i < 50; i++)
            {
                mesh.AddQuad(_testQuads[100 + i]);
                _ = mesh.GetQuadCount(); // Read operation
                mesh.AddTriangle(_testTriangles[i]);
                _ = mesh.GetTriangleCount(); // Read operation
            }
        }

        [Benchmark]
        [BenchmarkCategory("MixedOperations")]
        public void NewImplementation_MixedReadWrite()
        {
            using var mesh = new Mesh();

            // Add some initial data
            foreach (var quad in _testQuads.Take(100))
            {
                mesh.AddQuad(quad);
            }

            // Mix reads and writes
            for (int i = 0; i < 50; i++)
            {
                mesh.AddQuad(_testQuads[100 + i]);
                _ = mesh.QuadCount; // Read operation
                mesh.AddTriangle(_testTriangles[i]);
                _ = mesh.TriangleCount; // Read operation
            }
        }

        // MEMORY ALLOCATION COMPARISON

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("MemoryAllocation")]
        public void OldImplementation_CreateAndPopulate()
        {
            using var mesh = new OldMeshImplementation();
            mesh.AddQuads(_testQuads);
            mesh.AddTriangles(_testTriangles);

            // Access collections to trigger cache creation
            _ = mesh.Quads.Count;
            _ = mesh.Triangles.Count;
        }

        [Benchmark]
        [BenchmarkCategory("MemoryAllocation")]
        public void NewImplementation_CreateAndPopulate()
        {
            using var mesh = new Mesh();
            mesh.AddQuads(_testQuads);
            mesh.AddTriangles(_testTriangles);

            // Access collections to trigger cache creation
            _ = mesh.Quads.Count;
            _ = mesh.Triangles.Count;
        }
    }
}
