using System.Globalization;
using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure.Exporters
{
    /// <summary>Wavefront OBJ exporter (quads + triangles, geometry only).</summary>
    public static class ObjExporter
    {
        /// <summary>Write mesh as OBJ file (quads). Vertices first then faces.</summary>
        public static void Write(IndexedMesh mesh, string path)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentException.ThrowIfNullOrEmpty(path);
            using var sw = new StreamWriter(path);
            var inv = CultureInfo.InvariantCulture;
            sw.WriteLine("# FastGeoMesh OBJ export");
            foreach (var v in mesh.Vertices)
            {
                sw.WriteLine(string.Format(inv, "v {0} {1} {2}", v.X, v.Y, v.Z));
            }
            foreach (var (v0, v1, v2, v3) in mesh.Quads)
            {
                sw.WriteLine(string.Format(inv, "f {0} {1} {2} {3}", v0 + 1, v1 + 1, v2 + 1, v3 + 1));
            }
            foreach (var (v0, v1, v2) in mesh.Triangles)
            {
                sw.WriteLine(string.Format(inv, "f {0} {1} {2}", v0 + 1, v1 + 1, v2 + 1));
            }
        }
    }
}
