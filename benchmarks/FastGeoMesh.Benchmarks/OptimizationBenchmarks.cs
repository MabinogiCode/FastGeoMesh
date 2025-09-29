using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>Benchmarks measuring the impact of our performance optimizations.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class OptimizationImpactBenchmarks
    {
        private PrismStructureDefinition _complexStructureWithHoles = null!;
        private PrismStructureDefinition _largeRectangleStructure = null!;
        private MesherOptions _standardOptions = null!;
        private MesherOptions _highResolutionOptions = null!;
        private PrismMesher _mesher = null!;

        [GlobalSetup]
        public void Setup()
        {
            _mesher = new PrismMesher();

            // Complex structure with holes to stress-test spatial indexing
            var outer = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(100, 0), new Vec2(100, 60), new Vec2(0, 60)
            });
            
            var complexStructure = new PrismStructureDefinition(outer, -5, 15);
            
            // Add multiple holes to trigger O(n²) behavior in old implementation
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    double x = 15 + i * 20;
                    double y = 15 + j * 15;
                    var hole = Polygon2D.FromPoints(new[]
                    {
                        new Vec2(x, y), new Vec2(x + 5, y), 
                        new Vec2(x + 5, y + 5), new Vec2(x, y + 5)
                    });
                    complexStructure.AddHole(hole);
                }
            }
            _complexStructureWithHoles = complexStructure;

            // Large rectangle for high-resolution meshing
            var largeRect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(200, 0), new Vec2(200, 150), new Vec2(0, 150)
            });
            _largeRectangleStructure = new PrismStructureDefinition(largeRect, 0, 20);

            _standardOptions = new MesherOptions
            {
                TargetEdgeLengthXY = 2.0,
                TargetEdgeLengthZ = 2.0,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.75
            };

            _highResolutionOptions = new MesherOptions
            {
                TargetEdgeLengthXY = 0.5,
                TargetEdgeLengthZ = 0.5,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.75
            };
        }

        [Benchmark(Description = "Complex geometry with holes (spatial indexing test)")]
        public Mesh ComplexGeometryWithHoles()
        {
            return _mesher.Mesh(_complexStructureWithHoles, _standardOptions);
        }

        [Benchmark(Description = "Large high-resolution rectangle")]
        public Mesh LargeHighResolutionRectangle()
        {
            return _mesher.Mesh(_largeRectangleStructure, _highResolutionOptions);
        }

        [Benchmark(Description = "Complex to IndexedMesh conversion")]
        public IndexedMesh ComplexToIndexedMesh()
        {
            var mesh = _mesher.Mesh(_complexStructureWithHoles, _standardOptions);
            return IndexedMesh.FromMesh(mesh);
        }

        [Benchmark(Description = "Repeated meshing (object pool test)")]
        public Mesh[] RepeatedMeshing()
        {
            var results = new Mesh[10];
            for (int i = 0; i < 10; i++)
            {
                results[i] = _mesher.Mesh(_complexStructureWithHoles, _standardOptions);
            }
            return results;
        }
    }

    /// <summary>Benchmarks for memory allocation patterns.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class MemoryAllocationBenchmarks
    {
        private PrismStructureDefinition _structure = null!;
        private MesherOptions _options = null!;
        private PrismMesher _mesher = null!;

        [GlobalSetup]
        public void Setup()
        {
            _mesher = new PrismMesher();
            
            // Medium complexity structure
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(30, 0), new Vec2(30, 20), 
                new Vec2(20, 20), new Vec2(20, 10), new Vec2(10, 10),
                new Vec2(10, 20), new Vec2(0, 20)
            });
            _structure = new PrismStructureDefinition(polygon, 0, 5);
            
            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(5, 5), new Vec2(8, 5), new Vec2(8, 8), new Vec2(5, 8)
            });
            _structure.AddHole(hole);

            _options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                OutputRejectedCapTriangles = true
            };
        }

        [Benchmark(Description = "Single meshing operation")]
        public Mesh SingleMeshingOperation()
        {
            return _mesher.Mesh(_structure, _options);
        }

        [Benchmark(Description = "Mesh + IndexedMesh conversion")]
        public IndexedMesh MeshPlusConversion()
        {
            var mesh = _mesher.Mesh(_structure, _options);
            return IndexedMesh.FromMesh(mesh);
        }

        [Benchmark(Description = "Multiple mesh instances")]
        public Mesh[] MultipleMeshInstances()
        {
            var results = new Mesh[5];
            for (int i = 0; i < 5; i++)
            {
                results[i] = _mesher.Mesh(_structure, _options);
            }
            return results;
        }
    }

    /// <summary>Benchmarks comparing struct vs class performance for primitives.</summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class StructVsClassBenchmarks
    {
        private PrismStructureDefinition _structure = null!;
        private MesherOptions _options = null!;
        private PrismMesher _mesher = null!;

        [GlobalSetup]
        public void Setup()
        {
            _mesher = new PrismMesher();
            
            var rectangle = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(50, 0), new Vec2(50, 30), new Vec2(0, 30)
            });
            _structure = new PrismStructureDefinition(rectangle, 0, 10);

            _options = new MesherOptions
            {
                TargetEdgeLengthXY = 1.0,
                TargetEdgeLengthZ = 1.0,
                GenerateBottomCap = true,
                GenerateTopCap = true
            };
        }

        [Benchmark(Description = "Large mesh generation (struct benefits)")]
        public Mesh LargeMeshGeneration()
        {
            return _mesher.Mesh(_structure, _options);
        }

        [Benchmark(Description = "Quad iteration and processing")]
        public int ProcessAllQuads()
        {
            var mesh = _mesher.Mesh(_structure, _options);
            int count = 0;
            
            // Simulate processing each quad (struct should be faster)
            foreach (var quad in mesh.Quads)
            {
                var center = new Vec3(
                    (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) * 0.25,
                    (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) * 0.25,
                    (quad.V0.Z + quad.V1.Z + quad.V2.Z + quad.V3.Z) * 0.25
                );
                
                if (center.Z > 5.0)
                {
                    count++;
                }
            }
            
            return count;
        }
    }
}