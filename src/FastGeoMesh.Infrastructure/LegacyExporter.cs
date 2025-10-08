using System.Globalization;
using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure.Exporters
{
    /// <summary>
    /// Legacy format exporter for FastGeoMesh.
    /// Exports meshes in the original format: [vertex_count]\n[vertices]\n[edge_count]\n[edges]\n[quad_count]\n[quads]
    /// This is the primary format for the FastGeoMesh library.
    /// </summary>
    public static class LegacyExporter
    {
        /// <summary>
        /// Write mesh in legacy format (the primary format for FastGeoMesh).
        /// Format: vertex count, vertices with indices, edge count, edges with indices, quad count, quads with indices.
        /// Uses 1-based indexing as expected by the legacy format.
        /// </summary>
        /// <param name="mesh">The indexed mesh to export.</param>
        /// <param name="filePath">Path where to save the legacy format file.</param>
        public static void Write(IndexedMesh mesh, string filePath)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            using var writer = new StreamWriter(filePath);
            var culture = CultureInfo.InvariantCulture;

            // Write vertex count
            writer.WriteLine(mesh.Vertices.Count.ToString(culture));

            // Write vertices with 1-based indices
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                writer.WriteLine($"{i + 1} {vertex.X.ToString("F6", culture)} {vertex.Y.ToString("F6", culture)} {vertex.Z.ToString("F6", culture)}");
            }

            // Write edge count
            writer.WriteLine(mesh.Edges.Count.ToString(culture));

            // Write edges with 1-based indices
            for (int i = 0; i < mesh.Edges.Count; i++)
            {
                var (v0, v1) = mesh.Edges[i];
                writer.WriteLine($"{i + 1} {v0 + 1} {v1 + 1}");
            }

            // Write quad count
            writer.WriteLine(mesh.Quads.Count.ToString(culture));

            // Write quads with 1-based indices
            for (int i = 0; i < mesh.Quads.Count; i++)
            {
                var (v0, v1, v2, v3) = mesh.Quads[i];
                writer.WriteLine($"{i + 1} {v0 + 1} {v1 + 1} {v2 + 1} {v3 + 1}");
            }
        }

        /// <summary>
        /// Write mesh in legacy format with custom filename based on the legacy convention (0_maill.txt).
        /// </summary>
        /// <param name="mesh">The indexed mesh to export.</param>
        /// <param name="directory">Directory where to save the file.</param>
        /// <param name="filename">Optional custom filename. Defaults to "0_maill.txt".</param>
        public static void WriteWithLegacyName(IndexedMesh mesh, string directory, string filename = "0_maill.txt")
        {
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentException.ThrowIfNullOrEmpty(directory);
            ArgumentException.ThrowIfNullOrEmpty(filename);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, filename);
            Write(mesh, filePath);
        }
    }
}
