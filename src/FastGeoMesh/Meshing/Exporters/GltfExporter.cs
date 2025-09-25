using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;

namespace FastGeoMesh.Meshing.Exporters;

public static class GltfExporter
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    /// <summary>
    /// Writes a minimal glTF 2.0 (.gltf JSON) with embedded base64 buffer.
    /// Quads are triangulated as (v0,v1,v2) and (v0,v2,v3). Positions: FLOAT32. Indices: UINT32.
    /// </summary>
    public static void Write(IndexedMesh mesh, string path)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentException.ThrowIfNullOrEmpty(path);

        // Build buffers
        int vCount = mesh.Vertices.Count;
        int qCount = mesh.Quads.Count;
        int triCount = qCount * 2;

        // positions (float32 * 3 * vCount)
        int posBytes = vCount * 3 * sizeof(float);
        // indices (uint32 * 3 * triCount)
        int idxBytes = triCount * 3 * sizeof(uint);

        using var ms = new MemoryStream(posBytes + idxBytes);
        using var bw = new BinaryWriter(ms);

        // write positions
        float minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, maxZ = float.NegativeInfinity;
        foreach (var v in mesh.Vertices)
        {
            float x = (float)v.X, y = (float)v.Y, z = (float)v.Z;
            bw.Write(x); bw.Write(y); bw.Write(z);
            if (x < minX) minX = x; if (y < minY) minY = y; if (z < minZ) minZ = z;
            if (x > maxX) maxX = x; if (y > maxY) maxY = y; if (z > maxZ) maxZ = z;
        }

        // write indices, triangulating quads
        foreach (var (v0,v1,v2,v3) in mesh.Quads)
        {
            bw.Write((uint)v0); bw.Write((uint)v1); bw.Write((uint)v2);
            bw.Write((uint)v0); bw.Write((uint)v2); bw.Write((uint)v3);
        }

        bw.Flush();
        var bufferBytes = ms.ToArray();
        string dataUri = "data:application/octet-stream;base64," + Convert.ToBase64String(bufferBytes);

        // glTF JSON document
        int bufferByteLength = bufferBytes.Length;
        int posOffset = 0;
        int idxOffset = posBytes;

        var gltf = new
        {
            asset = new { version = "2.0", generator = "FastGeoMesh" },
            buffers = new object[]
            {
                new { uri = dataUri, byteLength = bufferByteLength }
            },
            bufferViews = new object[]
            {
                new { buffer = 0, byteOffset = posOffset, byteLength = posBytes, target = 34962 }, // ARRAY_BUFFER
                new { buffer = 0, byteOffset = idxOffset, byteLength = idxBytes, target = 34963 }, // ELEMENT_ARRAY_BUFFER
            },
            accessors = new object[]
            {
                new {
                    bufferView = 0,
                    componentType = 5126, // FLOAT
                    count = vCount,
                    type = "VEC3",
                    min = new[]{ (double)minX, (double)minY, (double)minZ },
                    max = new[]{ (double)maxX, (double)maxY, (double)maxZ }
                },
                new {
                    bufferView = 1,
                    componentType = 5125, // UNSIGNED_INT
                    count = triCount * 3,
                    type = "SCALAR"
                }
            },
            meshes = new object[]
            {
                new {
                    primitives = new object[]
                    {
                        new {
                            attributes = new { POSITION = 0 },
                            indices = 1,
                            mode = 4 // TRIANGLES
                        }
                    }
                }
            },
            nodes = new object[] { new { mesh = 0 } },
            scenes = new object[] { new { nodes = new[]{ 0 } } },
            scene = 0
        };

        var json = JsonSerializer.Serialize(gltf, Indented);
        File.WriteAllText(path, json);
    }
}
