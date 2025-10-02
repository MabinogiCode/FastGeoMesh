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
    /// <summary>
    /// Main entry point for the sample application.
    /// Demonstrates mesh generation and export capabilities.
    /// </summary>
    /// <param name="args">Command line arguments for controlling export formats (--obj, --gltf, --svg).</param>
    static void Main(string[] args)
    {
        // Test notre correction de PointInPolygon
        TestPointInPolygon();

        bool exportObj = args.Contains("--obj", StringComparer.OrdinalIgnoreCase);
        bool exportGltf = args.Contains("--gltf", StringComparer.OrdinalIgnoreCase);
        bool exportSvg = args.Contains("--svg", StringComparer.OrdinalIgnoreCase);
        bool exportAll = !exportObj && !exportGltf && !exportSvg;
        double length = 20.0;
        double width = 5.0;
        double z0 = -10.0;
        double z1 = 10.0;
        var poly = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(length, 0), new Vec2(length, width), new Vec2(0, width) });
        var structure = new PrismStructureDefinition(poly, z0, z1);
        structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(length, 0)), 2.5);
        structure.Geometry
            .AddPoint(new Vec3(0, 4, 2))
            .AddPoint(new Vec3(20, 4, 4))
            .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 2)));
        var options = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 1.0 };
        var mesh = new PrismMesher().Mesh(structure, options);
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
        Console.WriteLine($"Indexed: V={indexed.Vertices.Count}, E={indexed.Edges.Count}, Q={indexed.Quads.Count}");
        string prefix = "sample_mesh";
        if (exportAll || exportObj)
        {
            ObjExporter.Write(indexed, prefix + ".obj");
        }
        if (exportAll || exportGltf)
        {
            GltfExporter.Write(indexed, prefix + ".gltf");
        }
        if (exportAll || exportSvg)
        {
            SvgExporter.Write(indexed, prefix + ".svg");
        }
    }

    /// <summary>
    /// Tests the PointInPolygon functionality to verify the fix is working correctly.
    /// Validates point-in-polygon calculations for various test cases.
    /// </summary>
    static void TestPointInPolygon()
    {
        Console.WriteLine("=== Testing PointInPolygon Fix ===");

        // Test du carré
        var square = new Vec2[]
        {
            new(0, 0), new(10, 0), new(10, 10), new(0, 10)
        };

        // Test du point central (5,5) - doit être TRUE
        bool centerInside = GeometryHelper.PointInPolygon(square, 5, 5);
        Console.WriteLine($"Point (5,5) inside square: {centerInside} {(centerInside ? "✅" : "❌")}");

        // Test de points sur les bords - doivent être TRUE
        bool cornerInside = GeometryHelper.PointInPolygon(square, 0, 0);
        Console.WriteLine($"Point (0,0) on corner: {cornerInside} {(cornerInside ? "✅" : "❌")}");

        bool edgeInside = GeometryHelper.PointInPolygon(square, 5, 0);
        Console.WriteLine($"Point (5,0) on edge: {edgeInside} {(edgeInside ? "✅" : "❌")}");

        // Test de points à l'extérieur - doivent être FALSE
        bool outsideLeft = GeometryHelper.PointInPolygon(square, -1, 5);
        Console.WriteLine($"Point (-1,5) outside left: {outsideLeft} {(!outsideLeft ? "✅" : "❌")}");

        bool outsideRight = GeometryHelper.PointInPolygon(square, 11, 5);
        Console.WriteLine($"Point (11,5) outside right: {outsideRight} {(!outsideRight ? "✅" : "❌")}");

        // Test SpatialPolygonIndex aussi
        var spatialIndex = new SpatialPolygonIndex(square);
        bool spatialCenterInside = spatialIndex.IsInside(5, 5);
        Console.WriteLine($"SpatialIndex (5,5): {spatialCenterInside} {(spatialCenterInside ? "✅" : "❌")}");

        Console.WriteLine("=== Point-in-Polygon Test Complete ===\n");
    }
}
