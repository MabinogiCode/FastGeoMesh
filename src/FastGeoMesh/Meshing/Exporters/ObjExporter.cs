using System.Globalization;

namespace FastGeoMesh.Meshing.Exporters;

public static class ObjExporter
{
    /// <summary>
    /// Writes an OBJ file with quads (f v0 v1 v2 v3). Indices are 1-based as per OBJ spec.
    /// Only geometry is exported (no normals/uvs/materials).
    /// </summary>
    public static void Write(IndexedMesh mesh, string path)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var sw = new StreamWriter(path);
        var inv = CultureInfo.InvariantCulture;

        sw.WriteLine("# FastGeoMesh OBJ export");
        sw.WriteLine($"# vertices {mesh.Vertices.Count}");
        sw.WriteLine($"# quads {mesh.Quads.Count}");

        foreach (var v in mesh.Vertices)
        {
            sw.WriteLine(string.Format(inv, "v {0} {1} {2}", v.X, v.Y, v.Z));
        }

        foreach (var (v0, v1, v2, v3) in mesh.Quads)
        {
            // OBJ is 1-based
            sw.WriteLine(string.Format(inv, "f {0} {1} {2} {3}", v0 + 1, v1 + 1, v2 + 1, v3 + 1));
        }
    }
}
