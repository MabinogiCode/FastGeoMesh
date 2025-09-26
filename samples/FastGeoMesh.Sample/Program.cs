using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

sealed class Program
{
    static void Main()
    {
        double length = 20.0;
        double width = 5.0;
        double z0 = -10.0;
        double z1 = 10.0;

        var poly = Polygon2D.FromPoints(new[] {
            new Vec2(0,0), new Vec2(length,0), new Vec2(length,width), new Vec2(0,width)
        });
        var structure = new PrismStructureDefinition(poly, z0, z1);

        // Example: add a constraint level at Z=2.5 along one edge to force a horizontal slice
        structure.AddConstraintSegment(new Segment2D(new Vec2(0, 0), new Vec2(length, 0)), 2.5);

        // Example: add a generic 3D segment (could represent a guideline) between two points
        structure.Geometry
            .AddPoint(new Vec3(0, 4, 2))
            .AddPoint(new Vec3(20, 4, 4))
            .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 4)));

        var options = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 1.0 };
        var mesh = new PrismMesher().Mesh(structure, options);
        var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

        Console.WriteLine($"Indexed: V={indexed.Vertices.Count}, E={indexed.Edges.Count}, Q={indexed.Quads.Count}");
    }
}
