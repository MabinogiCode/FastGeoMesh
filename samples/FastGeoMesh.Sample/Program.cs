using System;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Meshing.Exporters;

sealed class Program
{
    static void Main(string[] args)
    {
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
}
