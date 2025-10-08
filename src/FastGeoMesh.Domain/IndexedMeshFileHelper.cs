using System.Globalization;

namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Helper for reading and writing custom mesh file formats.
    /// </summary>
    public static class IndexedMeshFileHelper
    {
        /// <summary>
        /// Reads an indexed mesh from the legacy format file.
        /// Expected format: [count]\n[vertices]\n[edge_count]\n[edges]\n[quad_count]\n[quads]
        /// Where vertices are "index x y z", edges are "index v0 v1", quads are "index v0 v1 v2 v3"
        /// </summary>
        /// <param name="filePath">Path to the legacy mesh file.</param>
        /// <returns>The loaded indexed mesh.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
        public static IndexedMesh ReadCustomTxt(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Mesh file not found: {filePath}");
            }

            var vertices = new List<Vec3>();
            var quads = new List<(int, int, int, int)>();
            var triangles = new List<(int, int, int)>();
            var edges = new List<(int, int)>();

            string[] lines = File.ReadAllLines(filePath);
            int lineIndex = 0;

            try
            {
                // Read vertex count
                int vertexCount = int.Parse(lines[lineIndex++], CultureInfo.InvariantCulture);

                // Read vertices
                for (int i = 0; i < vertexCount; i++)
                {
                    var parts = lines[lineIndex++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        // Format: index x y z
                        double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                        vertices.Add(new Vec3(x, y, z));
                    }
                }

                // Read edge count
                int edgeCount = int.Parse(lines[lineIndex++], CultureInfo.InvariantCulture);

                // Read edges
                for (int i = 0; i < edgeCount; i++)
                {
                    var parts = lines[lineIndex++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        // Format: index v0 v1
                        // Convert from 1-based to 0-based indexing
                        int v0 = int.Parse(parts[1], CultureInfo.InvariantCulture) - 1;
                        int v1 = int.Parse(parts[2], CultureInfo.InvariantCulture) - 1;
                        edges.Add((v0, v1));
                    }
                }

                // Read quad count
                int quadCount = int.Parse(lines[lineIndex++], CultureInfo.InvariantCulture);

                // Read quads
                for (int i = 0; i < quadCount; i++)
                {
                    var parts = lines[lineIndex++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        // Format: index v0 v1 v2 v3
                        // Convert from 1-based to 0-based indexing
                        int v0 = int.Parse(parts[1], CultureInfo.InvariantCulture) - 1;
                        int v1 = int.Parse(parts[2], CultureInfo.InvariantCulture) - 1;
                        int v2 = int.Parse(parts[3], CultureInfo.InvariantCulture) - 1;
                        int v3 = int.Parse(parts[4], CultureInfo.InvariantCulture) - 1;
                        quads.Add((v0, v1, v2, v3));
                    }
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is IndexOutOfRangeException)
            {
                throw new InvalidDataException($"Invalid legacy mesh file format at line {lineIndex}: {ex.Message}", ex);
            }

            return new IndexedMesh(vertices, edges, quads, triangles);
        }

        /// <summary>
        /// Reads an indexed mesh from a custom text format file (alternative format).
        /// Expected format: vertices as "v x y z" lines, quads as "q v0 v1 v2 v3" lines.
        /// </summary>
        /// <param name="filePath">Path to the custom mesh file.</param>
        /// <returns>The loaded indexed mesh.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
        public static IndexedMesh ReadCustomTxtAlternative(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Mesh file not found: {filePath}");
            }

            var vertices = new List<Vec3>();
            var quads = new List<(int, int, int, int)>();
            var triangles = new List<(int, int, int)>();
            var edges = new List<(int, int)>();

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                {
                    continue;
                }

                string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                try
                {
                    switch (parts[0])
                    {
                        case "v" when parts.Length == 4:
                            // Vertex: v x y z
                            vertices.Add(new Vec3(
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture),
                                double.Parse(parts[3], CultureInfo.InvariantCulture)));
                            break;

                        case "q" when parts.Length == 5:
                            // Quad: q v0 v1 v2 v3
                            quads.Add((
                                int.Parse(parts[1], CultureInfo.InvariantCulture),
                                int.Parse(parts[2], CultureInfo.InvariantCulture),
                                int.Parse(parts[3], CultureInfo.InvariantCulture),
                                int.Parse(parts[4], CultureInfo.InvariantCulture)));
                            break;

                        case "t" when parts.Length == 4:
                            // Triangle: t v0 v1 v2
                            triangles.Add((
                                int.Parse(parts[1], CultureInfo.InvariantCulture),
                                int.Parse(parts[2], CultureInfo.InvariantCulture),
                                int.Parse(parts[3], CultureInfo.InvariantCulture)));
                            break;

                        case "e" when parts.Length == 3:
                            // Edge: e v0 v1
                            edges.Add((
                                int.Parse(parts[1], CultureInfo.InvariantCulture),
                                int.Parse(parts[2], CultureInfo.InvariantCulture)));
                            break;
                    }
                }
                catch (FormatException ex)
                {
                    throw new InvalidDataException($"Invalid number format in line: {trimmed}", ex);
                }
                catch (OverflowException ex)
                {
                    throw new InvalidDataException($"Number overflow in line: {trimmed}", ex);
                }
            }

            return new IndexedMesh(vertices, edges, quads, triangles);
        }

        /// <summary>
        /// Writes an indexed mesh to the legacy format file.
        /// Format: [vertex_count]\n[vertices with indices]\n[edge_count]\n[edges with indices]\n[quad_count]\n[quads with indices]
        /// </summary>
        /// <param name="mesh">The mesh to write.</param>
        /// <param name="filePath">Path where to save the file.</param>
        public static void WriteCustomTxt(IndexedMesh mesh, string filePath)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            using var writer = new StreamWriter(filePath);

            // Write vertex count
            writer.WriteLine(mesh.Vertices.Count);

            // Write vertices with 1-based indices
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                writer.WriteLine($"{i + 1} {vertex.X:F6} {vertex.Y:F6} {vertex.Z:F6}");
            }

            // Write edge count
            writer.WriteLine(mesh.Edges.Count);

            // Write edges with 1-based indices
            for (int i = 0; i < mesh.Edges.Count; i++)
            {
                var (v0, v1) = mesh.Edges[i];
                writer.WriteLine($"{i + 1} {v0 + 1} {v1 + 1}");
            }

            // Write quad count
            writer.WriteLine(mesh.Quads.Count);

            // Write quads with 1-based indices
            for (int i = 0; i < mesh.Quads.Count; i++)
            {
                var (v0, v1, v2, v3) = mesh.Quads[i];
                writer.WriteLine($"{i + 1} {v0 + 1} {v1 + 1} {v2 + 1} {v3 + 1}");
            }
        }

        /// <summary>
        /// Writes an indexed mesh to a custom text format file (alternative format).
        /// </summary>
        /// <param name="mesh">The mesh to write.</param>
        /// <param name="filePath">Path where to save the file.</param>
        public static void WriteCustomTxtAlternative(IndexedMesh mesh, string filePath)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            using var writer = new StreamWriter(filePath);

            // Write header
            writer.WriteLine("# Custom mesh format");
            writer.WriteLine($"# Vertices: {mesh.Vertices.Count}");
            writer.WriteLine($"# Quads: {mesh.Quads.Count}");
            writer.WriteLine($"# Triangles: {mesh.Triangles.Count}");
            writer.WriteLine();

            // Write vertices
            foreach (var vertex in mesh.Vertices)
            {
                writer.WriteLine($"v {vertex.X:F6} {vertex.Y:F6} {vertex.Z:F6}");
            }

            // Write quads
            foreach (var (v0, v1, v2, v3) in mesh.Quads)
            {
                writer.WriteLine($"q {v0} {v1} {v2} {v3}");
            }

            // Write triangles
            foreach (var (v0, v1, v2) in mesh.Triangles)
            {
                writer.WriteLine($"t {v0} {v1} {v2}");
            }

            // Writer edges
            foreach (var (v0, v1) in mesh.Edges)
            {
                writer.WriteLine($"e {v0} {v1}");
            }
        }
    }
}
