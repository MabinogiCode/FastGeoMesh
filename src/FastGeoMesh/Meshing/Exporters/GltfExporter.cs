using System.Text.Json.Serialization;

namespace FastGeoMesh.Meshing.Exporters
{
    /// <summary>glTF 2.0 exporter (.gltf JSON with embedded base64 buffer). Quads are triangulated; standalone triangles are exported directly.</summary>
    public static class GltfExporter
    {
        private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

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
                bw.Write((float)v.X);
                bw.Write((float)v.Y);
                bw.Write((float)v.Z);
            }
            int vCount = mesh.Vertices.Count;
            int posBytes = vCount * sizeof(float) * 3;

            // indices (two per quad + one per triangle)
            int quadPairTriCount = mesh.Quads.Count * 2; // each quad becomes 2 triangles
            int extraTriCount = mesh.Triangles.Count;     // already triangles
            int totalTriCount = quadPairTriCount + extraTriCount;
            int idxBytes = totalTriCount * 3 * sizeof(uint);

            var (minX, minY, minZ, maxX, maxY, maxZ) = CalculateBounds(mesh);

            // write quad-derived triangles
            foreach (var (v0, v1, v2, v3) in mesh.Quads)
            {
                bw.Write((uint)v0);
                bw.Write((uint)v1);
                bw.Write((uint)v2);
                bw.Write((uint)v0);
                bw.Write((uint)v2);
                bw.Write((uint)v3);
            }
            // write standalone cap triangles
            foreach (var (v0, v1, v2) in mesh.Triangles)
            {
                bw.Write((uint)v0);
                bw.Write((uint)v1);
                bw.Write((uint)v2);
            }
            bw.Flush();
            byte[] bufferBytes = ms.ToArray();
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
                    new { buffer = 0, byteOffset = idxOffset, byteLength = idxBytes, target = 34963 }
                },
                accessors = new object[]
                {
                    new { bufferView = 0, componentType = 5126, count = vCount, type = "VEC3", min = new[]{ minX, minY, minZ }, max = new[]{ maxX, maxY, maxZ } },
                    new { bufferView = 1, componentType = 5125, count = totalTriCount * 3, type = "SCALAR" }
                },
                meshes = new object[] { new { primitives = new object[] { new { attributes = new { POSITION = 0 }, indices = 1, mode = 4 } } } },
                nodes = new object[] { new { mesh = 0 } },
                scenes = new object[] { new { nodes = new[] { 0 } } },
                scene = 0
            };

            string json = JsonSerializer.Serialize(gltf, Indented);
            File.WriteAllText(path, json);
        }

        private static (double minX, double minY, double minZ, double maxX, double maxY, double maxZ) CalculateBounds(IndexedMesh mesh)
        {
            double minX = double.PositiveInfinity, minY = double.PositiveInfinity, minZ = double.PositiveInfinity;
            double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity, maxZ = double.NegativeInfinity;

            foreach (var v in mesh.Vertices)
            {
                if (v.X < minX)
                {
                    minX = v.X;
                }
                if (v.Y < minY)
                {
                    minY = v.Y;
                }
                if (v.Z < minZ)
                {
                    minZ = v.Z;
                }
                if (v.X > maxX)
                {
                    maxX = v.X;
                }
                if (v.Y > maxY)
                {
                    maxY = v.Y;
                }
                if (v.Z > maxZ)
                {
                    maxZ = v.Z;
                }
            }

            return (minX, minY, minZ, maxX, maxY, maxZ);
        }
    }
}
