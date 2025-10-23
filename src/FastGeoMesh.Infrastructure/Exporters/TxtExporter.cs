using System.Globalization;
using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure {
    /// <summary>Position for count placement in the file.</summary>
    public enum CountPlacement {
        /// <summary>Count appears at the top of each section.</summary>
        Top,
        /// <summary>Count appears at the bottom of each section.</summary>
        Bottom,
        /// <summary>No count is written.</summary>
        None
    }

    /// <summary>Configuration for element formatting in TXT export.</summary>
    public sealed class ElementConfig {
        /// <summary>Prefix to add before each element line.</summary>
        public string Prefix { get; }

        /// <summary>Where to place the element count (top, bottom, or none).</summary>
        public CountPlacement CountPlacement { get; }

        /// <summary>Whether to include element index in the output.</summary>
        public bool IndexBased { get; }

        /// <summary>Create a new element configuration.</summary>
        /// <param name="prefix">Prefix to add before each element line.</param>
        /// <param name="countPlacement">Where to place the element count.</param>
        /// <param name="indexBased">Whether to include element index in the output.</param>
        public ElementConfig(string prefix, CountPlacement countPlacement, bool indexBased) {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            CountPlacement = countPlacement;
            IndexBased = indexBased;
        }
    }

    /// <summary>Builder for configurable TXT export.</summary>
    public sealed class TxtExportBuilder {
        private readonly IndexedMesh _mesh;
        private ElementConfig? _pointsConfig;
        private ElementConfig? _edgesConfig;
        private ElementConfig? _quadsConfig;
        private ElementConfig? _trianglesConfig;

        internal TxtExportBuilder(IndexedMesh mesh) {
            _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        }

        /// <summary>Configure points export.</summary>
        public TxtExportBuilder WithPoints(string prefix, CountPlacement countPlacement, bool indexBased) {
            _pointsConfig = new ElementConfig(prefix, countPlacement, indexBased);
            return this;
        }

        /// <summary>Configure edges export.</summary>
        public TxtExportBuilder WithEdges(string prefix, CountPlacement countPlacement, bool indexBased) {
            _edgesConfig = new ElementConfig(prefix, countPlacement, indexBased);
            return this;
        }

        /// <summary>Configure quads export.</summary>
        public TxtExportBuilder WithQuads(string prefix, CountPlacement countPlacement, bool indexBased) {
            _quadsConfig = new ElementConfig(prefix, countPlacement, indexBased);
            return this;
        }

        /// <summary>Configure triangles export.</summary>
        public TxtExportBuilder WithTriangles(string prefix, CountPlacement countPlacement, bool indexBased) {
            _trianglesConfig = new ElementConfig(prefix, countPlacement, indexBased);
            return this;
        }

        /// <summary>Export to file with the configured format.</summary>
        public void ToFile(string filePath) {
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            using var writer = new StreamWriter(filePath);
            var culture = CultureInfo.InvariantCulture;

            // Export points
            if (_pointsConfig != null) {
                WriteSection(writer, culture, _pointsConfig, _mesh.Vertices.Count, (w, c, cfg, i) => {
                    var vertex = _mesh.Vertices[i];
                    if (cfg.IndexBased) {
                        w.WriteLine($"{cfg.Prefix} {i + 1} {vertex.X.ToString("F6", c)} {vertex.Y.ToString("F6", c)} {vertex.Z.ToString("F6", c)}");
                    }
                    else {
                        w.WriteLine($"{cfg.Prefix} {vertex.X.ToString("F6", c)} {vertex.Y.ToString("F6", c)} {vertex.Z.ToString("F6", c)}");
                    }
                });
            }

            // Export edges
            if (_edgesConfig != null) {
                WriteSection(writer, culture, _edgesConfig, _mesh.Edges.Count, (w, c, cfg, i) => {
                    var (v0, v1) = _mesh.Edges[i];
                    if (cfg.IndexBased) {
                        w.WriteLine($"{cfg.Prefix} {i + 1} {v0 + 1} {v1 + 1}");
                    }
                    else {
                        w.WriteLine($"{cfg.Prefix} {v0 + 1} {v1 + 1}");
                    }
                });
            }

            // Export quads
            if (_quadsConfig != null) {
                WriteSection(writer, culture, _quadsConfig, _mesh.Quads.Count, (w, c, cfg, i) => {
                    var (v0, v1, v2, v3) = _mesh.Quads[i];
                    if (cfg.IndexBased) {
                        w.WriteLine($"{cfg.Prefix} {i + 1} {v0 + 1} {v1 + 1} {v2 + 1} {v3 + 1}");
                    }
                    else {
                        w.WriteLine($"{cfg.Prefix} {v0 + 1} {v1 + 1} {v2 + 1} {v3 + 1}");
                    }
                });
            }

            // Export triangles
            if (_trianglesConfig != null) {
                WriteSection(writer, culture, _trianglesConfig, _mesh.Triangles.Count, (w, c, cfg, i) => {
                    var (v0, v1, v2) = _mesh.Triangles[i];
                    if (cfg.IndexBased) {
                        w.WriteLine($"{cfg.Prefix} {i + 1} {v0 + 1} {v1 + 1} {v2 + 1}");
                    }
                    else {
                        w.WriteLine($"{cfg.Prefix} {v0 + 1} {v1 + 1} {v2 + 1}");
                    }
                });
            }
        }

        private static void WriteSection(StreamWriter writer, CultureInfo culture, ElementConfig config, int count, Action<StreamWriter, CultureInfo, ElementConfig, int> writeElement) {
            // Write top count if needed
            if (config.CountPlacement == CountPlacement.Top) {
                writer.WriteLine(count.ToString(culture));
            }

            // Write elements
            for (int i = 0; i < count; i++) {
                writeElement(writer, culture, config, i);
            }

            // Write bottom count if needed
            if (config.CountPlacement == CountPlacement.Bottom) {
                writer.WriteLine(count.ToString(culture));
            }
        }
    }

    /// <summary>Flexible TXT exporter with configurable format.</summary>
    public static class TxtExporter {
        /// <summary>Start building a TXT export configuration.</summary>
        public static TxtExportBuilder ExportTxt(this IndexedMesh mesh) {
            return new TxtExportBuilder(mesh);
        }

        /// <summary>Export with OBJ-like format (no counts, prefixed elements).</summary>
        public static void WriteObjLike(IndexedMesh mesh, string filePath) {
            mesh.ExportTxt()
                .WithPoints("v", CountPlacement.None, false)
                .WithEdges("l", CountPlacement.None, false)
                .WithQuads("f", CountPlacement.None, false)
                .WithTriangles("f", CountPlacement.None, false)
                .ToFile(filePath);
        }
    }
}
