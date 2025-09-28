using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>Benchmarks comparing rectangle fast-path vs generic tessellation for cap generation.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class RectangleVsTessellationBenchmarks
    {
        private PrismStructureDefinition _rectangleStructure = null!;
        private PrismStructureDefinition _nonRectangleStructure = null!;
        private MesherOptions _options = null!;
        private PrismMesher _mesher = null!;

        [GlobalSetup]
        public void Setup()
        {
            // Rectangle (should use fast-path grid generation)
            var rectangle = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 10), new Vec2(0, 10)
            });
            _rectangleStructure = new PrismStructureDefinition(rectangle, 0, 5);

            // Non-rectangle (forces tessellation + quadification)
            var hexagon = Polygon2D.FromPoints(new[]
            {
                new Vec2(10, 0), new Vec2(17.32, 5), new Vec2(17.32, 15), 
                new Vec2(10, 20), new Vec2(2.68, 15), new Vec2(2.68, 5)
            });
            _nonRectangleStructure = new PrismStructureDefinition(hexagon, 0, 5);

            _options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            _mesher = new PrismMesher();
        }

        [Benchmark(Baseline = true)]
        public Mesh RectangleFastPath()
        {
            return _mesher.Mesh(_rectangleStructure, _options);
        }

        [Benchmark]
        public Mesh HexagonTessellation()
        {
            return _mesher.Mesh(_nonRectangleStructure, _options);
        }
    }

    /// <summary>Benchmarks for different mesh sizes and target edge lengths.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class MeshSizeBenchmarks
    {
        private PrismMesher _mesher = null!;

        [GlobalSetup]
        public void Setup()
        {
            _mesher = new PrismMesher();
        }

        [Params(1.0, 0.5, 0.25)]
        public double TargetEdgeLength { get; set; }

        [Params(10, 50, 100)]
        public int GeometrySize { get; set; }

        [Benchmark]
        public Mesh MeshRectangle()
        {
            var rectangle = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(GeometrySize, 0), 
                new Vec2(GeometrySize, GeometrySize), new Vec2(0, GeometrySize)
            });
            
            var structure = new PrismStructureDefinition(rectangle, 0, GeometrySize / 2);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = TargetEdgeLength,
                TargetEdgeLengthZ = TargetEdgeLength,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };

            return _mesher.Mesh(structure, options);
        }
    }

    /// <summary>Benchmarks for quad quality scoring and triangulation fallback.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class QuadQualityBenchmarks
    {
        private PrismStructureDefinition _complexStructure = null!;
        private PrismMesher _mesher = null!;

        [GlobalSetup]
        public void Setup()
        {
            // L-shaped polygon likely to generate varied quad qualities
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(15, 0), new Vec2(15, 5),
                new Vec2(5, 5), new Vec2(5, 15), new Vec2(0, 15)
            });
            _complexStructure = new PrismStructureDefinition(lShape, 0, 3);
            _mesher = new PrismMesher();
        }

        [Params(0.0, 0.5, 0.8, 0.95)]
        public double MinCapQuadQuality { get; set; }

        [Benchmark]
        public Mesh MeshWithQualityThreshold()
        {
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 0.8,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = MinCapQuadQuality,
                OutputRejectedCapTriangles = true
            };

            return _mesher.Mesh(_complexStructure, options);
        }
    }

    /// <summary>Benchmarks for IndexedMesh conversion and adjacency building.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class IndexedMeshBenchmarks
    {
        private Mesh _smallMesh = null!;
        private Mesh _largeMesh = null!;

        [GlobalSetup]
        public void Setup()
        {
            var mesher = new PrismMesher();

            // Small mesh
            var smallRect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
            });
            var smallStructure = new PrismStructureDefinition(smallRect, 0, 2);
            var smallOptions = new MesherOptions { TargetEdgeLengthXY = 1.0, TargetEdgeLengthZ = 1.0 };
            _smallMesh = mesher.Mesh(smallStructure, smallOptions);

            // Large mesh
            var largeRect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(50, 0), new Vec2(50, 50), new Vec2(0, 50)
            });
            var largeStructure = new PrismStructureDefinition(largeRect, 0, 10);
            var largeOptions = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 0.5 };
            _largeMesh = mesher.Mesh(largeStructure, largeOptions);
        }

        [Benchmark]
        public IndexedMesh ConvertSmallMesh()
        {
            return IndexedMesh.FromMesh(_smallMesh);
        }

        [Benchmark]
        public IndexedMesh ConvertLargeMesh()
        {
            return IndexedMesh.FromMesh(_largeMesh);
        }

        [Benchmark]
        public MeshAdjacency BuildSmallAdjacency()
        {
            var indexed = IndexedMesh.FromMesh(_smallMesh);
            return indexed.BuildAdjacency();
        }

        [Benchmark]
        public MeshAdjacency BuildLargeAdjacency()
        {
            var indexed = IndexedMesh.FromMesh(_largeMesh);
            return indexed.BuildAdjacency();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("FastGeoMesh Benchmarks");
                Console.WriteLine("Usage: FastGeoMesh.Benchmarks [benchmark-class-name]");
                Console.WriteLine("");
                Console.WriteLine("Available benchmarks:");
                Console.WriteLine("  RectangleVsTessellation - Compare rectangle fast-path vs generic tessellation");
                Console.WriteLine("  MeshSize - Performance vs geometry size and target edge length");
                Console.WriteLine("  QuadQuality - Quad quality scoring and triangulation fallback impact");
                Console.WriteLine("  IndexedMesh - IndexedMesh conversion and adjacency building");
                Console.WriteLine("  All - Run all benchmarks");
                return;
            }

            var benchmarkName = args[0].ToLowerInvariant();
            switch (benchmarkName)
            {
                case "rectanglevstessellation":
                case "rectangle":
                    BenchmarkRunner.Run<RectangleVsTessellationBenchmarks>();
                    break;
                case "meshsize":
                case "size":
                    BenchmarkRunner.Run<MeshSizeBenchmarks>();
                    break;
                case "quadquality":
                case "quality":
                    BenchmarkRunner.Run<QuadQualityBenchmarks>();
                    break;
                case "indexedmesh":
                case "indexed":
                    BenchmarkRunner.Run<IndexedMeshBenchmarks>();
                    break;
                case "all":
                    BenchmarkRunner.Run<RectangleVsTessellationBenchmarks>();
                    BenchmarkRunner.Run<MeshSizeBenchmarks>();
                    BenchmarkRunner.Run<QuadQualityBenchmarks>();
                    BenchmarkRunner.Run<IndexedMeshBenchmarks>();
                    break;
                default:
                    Console.WriteLine($"Unknown benchmark: {benchmarkName}");
                    break;
            }
        }
    }
}