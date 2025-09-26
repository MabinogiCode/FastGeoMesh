using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastGeoMesh.Meshing.Exporters;

/// <summary>glTF 2.0 exporter (.gltf JSON with embedded base64 buffer). Quads are triangulated.</summary>
public static class GltfExporter
{
    private static readonly JsonSerializerOptions Indented = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    /// <summary>Write indexed mesh as glTF file (positions + indices only).</summary>
    public static void Write(IndexedMesh mesh, string path)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentException.ThrowIfNullOrEmpty(path);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // positions
        foreach (var v in mesh.Vertices)
        {
            bw.Write((float)v.X); bw.Write((float)v.Y); bw.Write((float)v.Z);
        }
        int vCount = mesh.Vertices.Count;
        int posBytes = vCount * sizeof(float) * 3;

        // indices
        int triCount = mesh.Quads.Count * 2;
        int idxBytes = triCount * 3 * sizeof(uint);
        double minX = double.PositiveInfinity, minY = double.PositiveInfinity, minZ = double.PositiveInfinity;
        double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity, maxZ = double.NegativeInfinity;
        foreach (var v in mesh.Vertices)
        {
            double x = v.X, y = v.Y, z = v.Z;
            if (x < minX) minX = x; if (y < minY) minY = y; if (z < minZ) minZ = z;
            if (x > maxX) maxX = x; if (y > maxY) maxY = y; if (z > maxZ) maxZ = z;
        }
        foreach (var (v0,v1,v2,v3) in mesh.Quads)
        {
            bw.Write((uint)v0); bw.Write((uint)v1); bw.Write((uint)v2);
            bw.Write((uint)v0); bw.Write((uint)v2); bw.Write((uint)v3);
        }
        bw.Flush();
        var bufferBytes = ms.ToArray();
        string dataUri = "data:application/octet-stream;base64," + Convert.ToBase64String(bufferBytes);
        int bufferByteLength = bufferBytes.Length;
        int posOffset = 0;
        int idxOffset = posBytes;

        var gltf = new
        {
            asset = new { version = "2.0", generator = "FastGeoMesh" },
            buffers = new object[] { new { uri = dataUri, byteLength = bufferByteLength } },
            bufferViews = new object[]
            {
                new { buffer = 0, byteOffset = posOffset, byteLength = posBytes, target = 34962 },
                new { buffer = 0, byteOffset = idxOffset, byteLength = idxBytes, target = 34963 },
            },
            accessors = new object[]
            {
                new { bufferView = 0, componentType = 5126, count = vCount, type = "VEC3", min = new[]{ (double)minX, (double)minY, (double)minZ }, max = new[]{ (double)maxX, (double)maxY, (double)maxZ } },
                new { bufferView = 1, componentType = 5125, count = triCount * 3, type = "SCALAR" }
            },
            meshes = new object[] { new { primitives = new object[] { new { attributes = new { POSITION = 0 }, indices = 1, mode = 4 } } } },
            nodes = new object[] { new { mesh = 0 } },
            scenes = new object[] { new { nodes = new[]{ 0 } } },
            scene = 0
        };

        var json = JsonSerializer.Serialize(gltf, Indented);
        File.WriteAllText(path, json);
    }
}
