using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using System.Globalization;
using FastGeoMesh.Meshing.Exporters;

sealed class Program
{
    static void Main()
    {
        // Paramètres
        double length = 20.0; // X
        double width = 5.0;   // Y
        double z0 = -10.0;
        double z1 = 10.0;

        // Footprint: rectangle axis-aligned, 20m (X) by 5m (Y)
        var poly = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(length,0), new Vec2(length,width), new Vec2(0,width)
        });

        // Structure
        var structure = new PrismStructureDefinition(poly, z0, z1);

        // Lierne à Z = 2.5 m (prise en compte par la subdivision Z)
        structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(length, 0)), 2.5);

        // Poutre entre A(0,4,2) et B(20,4,4)
        structure.Geometry
            .AddPoint(new Vec3(0, 4, 2))
            .AddPoint(new Vec3(20, 4, 4))
            .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 4)));

        // Meshing: 0.5m in XY, 1.0m in Z
        var options = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 1.0, GenerateTopAndBottomCaps = true };
        var mesher = new PrismMesher();
        var mesh = mesher.Mesh(structure, options);

        // Convert to indexed mesh (points/edges/quads indexés) similar to provided format
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

        Console.WriteLine($"Indexed: V={indexed.Vertices.Count}, E={indexed.Edges.Count}, Q={indexed.Quads.Count}");

        // Minimal checks: ensure CCW order for quads (projection XY for side faces is adequate here)
        foreach (var q in mesh.Quads)
        {
            var ax = q.V1.X - q.V0.X;
            var ay = q.V1.Y - q.V0.Y;
            var bx = q.V2.X - q.V1.X;
            var by = q.V2.Y - q.V1.Y;
            double cross = ax*by - ay*bx;
            if (cross < -1e-9) throw new InvalidOperationException("Found non-CCW quad");
        }

        // Export: write custom text and OBJ
        string outDir = Path.Combine(AppContext.BaseDirectory, "out");
        Directory.CreateDirectory(outDir);
        var txtPath = Path.Combine(outDir, "mesh.txt");
        var objPath = Path.Combine(outDir, "mesh.obj");
        indexed.WriteCustomTxt(txtPath);
        ObjExporter.Write(indexed, objPath);
        Console.WriteLine($"Wrote: {txtPath}\nWrote: {objPath}");

        // Example: read back sample file for comparison (if available)
        var samplePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tests", "FastGeoMesh.Tests", "0_maill.txt");
        if (File.Exists(samplePath))
        {
            var refMesh = IndexedMesh.ReadCustomTxt(samplePath);
            Console.WriteLine($"Reference: V={refMesh.Vertices.Count}, E={refMesh.Edges.Count}, Q={refMesh.Quads.Count}");
        }
    }
}
