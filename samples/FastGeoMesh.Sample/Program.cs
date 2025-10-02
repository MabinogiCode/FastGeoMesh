using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;

/// <summary>
/// Sample application demonstrating FastGeoMesh library usage with various export formats.
/// </summary>
sealed class Program
{
    // Sample configuration constants
    private const double SampleLength = 20.0;
    private const double SampleWidth = 5.0;
    private const double SampleBottomZ = -10.0;
    private const double SampleTopZ = 10.0;
    private const double SampleConstraintZ = 2.5;
    private const double SampleTargetEdgeLengthXY = 0.5;
    private const double SampleTargetEdgeLengthZ = 1.0;
    private const string SampleMeshPrefix = "sample_mesh";

    /// <summary>
    /// Main entry point for the sample application.
    /// Demonstrates mesh generation and export capabilities.
    /// </summary>
    /// <param name="args">Command line arguments for controlling export formats (--obj, --gltf, --svg).</param>
    static void Main(string[] args)
    {
        // Test our PointInPolygon fix
        TestPointInPolygon();

        bool exportObj = args.Contains("--obj", StringComparer.OrdinalIgnoreCase);
        bool exportGltf = args.Contains("--gltf", StringComparer.OrdinalIgnoreCase);
        bool exportSvg = args.Contains("--svg", StringComparer.OrdinalIgnoreCase);
        bool exportAll = !exportObj && !exportGltf && !exportSvg;
        
        var poly = Polygon2D.FromPoints(new[] { 
            new Vec2(0, 0), 
            new Vec2(SampleLength, 0), 
            new Vec2(SampleLength, SampleWidth), 
            new Vec2(0, SampleWidth) 
        });
        var structure = new PrismStructureDefinition(poly, SampleBottomZ, SampleTopZ);
        structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(SampleLength, 0)), SampleConstraintZ);
        structure.Geometry
            .AddPoint(new Vec3(0, 4, 2))
            .AddPoint(new Vec3(SampleLength, 4, 4))
            .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(SampleLength, 4, 2)));
        var options = new MesherOptions { 
            TargetEdgeLengthXY = SampleTargetEdgeLengthXY, 
            TargetEdgeLengthZ = SampleTargetEdgeLengthZ 
        };
        var mesh = new PrismMesher().Mesh(structure, options);
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
        Console.WriteLine($"Indexed: V={indexed.Vertices.Count}, E={indexed.Edges.Count}, Q={indexed.Quads.Count}");
        
        if (exportAll || exportObj)
        {
            ObjExporter.Write(indexed, SampleMeshPrefix + ".obj");
        }
        if (exportAll || exportGltf)
        {
            GltfExporter.Write(indexed, SampleMeshPrefix + ".gltf");
        }
        if (exportAll || exportSvg)
        {
            SvgExporter.Write(indexed, SampleMeshPrefix + ".svg");
        }
    }

    /// <summary>
    /// Tests the PointInPolygon functionality to verify the fix is working correctly.
    /// Validates point-in-polygon calculations for various test cases.
    /// </summary>
    static void TestPointInPolygon()
    {
        Console.WriteLine("=== Testing PointInPolygon Fix ===");

        // Test square dimensions
        const double squareSize = 10.0;
        const double centerPoint = 5.0;
        const double edgePoint = 0.0;
        const double outsidePoint = -1.0;
        const double farOutsidePoint = 11.0;

        // Test square
        var square = new Vec2[]
        {
            new(0, 0), new(squareSize, 0), new(squareSize, squareSize), new(0, squareSize)
        };

        // Test center point (5,5) - should be TRUE
        bool centerInside = GeometryHelper.PointInPolygon(square, centerPoint, centerPoint);
        Console.WriteLine($"Point ({centerPoint},{centerPoint}) inside square: {centerInside} {(centerInside ? "✅" : "❌")}");

        // Test points on edges - should be TRUE
        bool cornerInside = GeometryHelper.PointInPolygon(square, edgePoint, edgePoint);
        Console.WriteLine($"Point ({edgePoint},{edgePoint}) on corner: {cornerInside} {(cornerInside ? "✅" : "❌")}");

        bool edgeInside = GeometryHelper.PointInPolygon(square, centerPoint, edgePoint);
        Console.WriteLine($"Point ({centerPoint},{edgePoint}) on edge: {edgeInside} {(edgeInside ? "✅" : "❌")}");

        // Test points outside - should be FALSE
        bool outsideLeft = GeometryHelper.PointInPolygon(square, outsidePoint, centerPoint);
        Console.WriteLine($"Point ({outsidePoint},{centerPoint}) outside left: {outsideLeft} {(!outsideLeft ? "✅" : "❌")}");

        bool outsideRight = GeometryHelper.PointInPolygon(square, farOutsidePoint, centerPoint);
        Console.WriteLine($"Point ({farOutsidePoint},{centerPoint}) outside right: {outsideRight} {(!outsideRight ? "✅" : "❌")}");

        // Test SpatialPolygonIndex as well
        var spatialIndex = new SpatialPolygonIndex(square);
        bool spatialCenterInside = spatialIndex.IsInside(centerPoint, centerPoint);
        Console.WriteLine($"SpatialIndex ({centerPoint},{centerPoint}): {spatialCenterInside} {(spatialCenterInside ? "✅" : "❌")}");

        Console.WriteLine("=== Point-in-Polygon Test Complete ===\n");
    }
}
