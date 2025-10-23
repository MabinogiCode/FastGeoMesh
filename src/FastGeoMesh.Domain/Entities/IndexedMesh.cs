namespace FastGeoMesh.Domain {
    /// <summary>Spatial index-like mesh with vertex deduplication (explicit shared vertices for export).</summary>
    public sealed class IndexedMesh {
        private readonly IReadOnlyList<Vec3> _vertices;
        private readonly IReadOnlyList<(int a, int b)> _edges;
        private readonly IReadOnlyList<(int, int, int, int)> _quads;
        private readonly IReadOnlyList<(int, int, int)> _triangles;

        /// <summary>Create indexed mesh from explicit data.</summary>
        public IndexedMesh(IReadOnlyList<Vec3> vertices, IReadOnlyList<(int a, int b)> edges, IReadOnlyList<(int, int, int, int)> quads, IReadOnlyList<(int, int, int)> triangles) {
            _vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            _edges = edges ?? throw new ArgumentNullException(nameof(edges));
            _quads = quads ?? throw new ArgumentNullException(nameof(quads));
            _triangles = triangles ?? throw new ArgumentNullException(nameof(triangles));
        }

        /// <summary>Vertex positions.</summary>
        public IReadOnlyList<Vec3> Vertices => _vertices;
        /// <summary>Edge vertex pairs.</summary>
        public IReadOnlyList<(int a, int b)> Edges => _edges;
        /// <summary>Quad vertex indices (CCW).</summary>
        public IReadOnlyList<(int, int, int, int)> Quads => _quads;
        /// <summary>Triangle vertex indices (CCW).</summary>
        public IReadOnlyList<(int, int, int)> Triangles => _triangles;

        /// <summary>Total vertex count.</summary>
        public int VertexCount => _vertices.Count;
        /// <summary>Total quad count.</summary>
        public int QuadCount => _quads.Count;
        /// <summary>Total triangle count.</summary>
        public int TriangleCount => _triangles.Count;

        /// <summary>
        /// Total edge count (unique edges across quads, triangles, and internal segments).
        /// </summary>
        public int EdgeCount => _edges.Count;

        /// <summary>Create indexed mesh from immutable mesh with vertex deduplication.</summary>
        public static IndexedMesh FromMesh(ImmutableMesh mesh, double epsilon = 1e-9) {
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentOutOfRangeException.ThrowIfNegative(epsilon);

            var vertices = new List<Vec3>();
            var vertexMap = new Dictionary<Vec3, int>();
            var edges = new HashSet<(int, int)>();
            var quads = new List<(int, int, int, int)>();
            var triangles = new List<(int, int, int)>();

            int GetOrAddVertex(Vec3 vertex) {
                // Find existing vertex within epsilon tolerance
                foreach (var (existingVertex, index) in vertexMap) {
                    if ((vertex - existingVertex).LengthSquared() <= epsilon * epsilon) {
                        return index;
                    }
                }

                // Add new vertex
                int newIndex = vertices.Count;
                vertices.Add(vertex);
                vertexMap[vertex] = newIndex;
                return newIndex;
            }

            void AddEdge(int a, int b) {
                if (a != b) {
                    edges.Add(a < b ? (a, b) : (b, a));
                }
            }

            // Process quads
            foreach (var quad in mesh.Quads) {
                int v0 = GetOrAddVertex(quad.V0);
                int v1 = GetOrAddVertex(quad.V1);
                int v2 = GetOrAddVertex(quad.V2);
                int v3 = GetOrAddVertex(quad.V3);

                quads.Add((v0, v1, v2, v3));

                // Add edges
                AddEdge(v0, v1);
                AddEdge(v1, v2);
                AddEdge(v2, v3);
                AddEdge(v3, v0);
            }

            // Process triangles
            foreach (var triangle in mesh.Triangles) {
                int v0 = GetOrAddVertex(triangle.V0);
                int v1 = GetOrAddVertex(triangle.V1);
                int v2 = GetOrAddVertex(triangle.V2);

                triangles.Add((v0, v1, v2));

                // Add edges
                AddEdge(v0, v1);
                AddEdge(v1, v2);
                AddEdge(v2, v0);
            }

            // Process standalone points
            foreach (var point in mesh.Points) {
                GetOrAddVertex(point);
            }

            // Process internal segments
            foreach (var segment in mesh.InternalSegments) {
                int v0 = GetOrAddVertex(segment.Start);
                int v1 = GetOrAddVertex(segment.End);
                AddEdge(v0, v1);
            }

            return new IndexedMesh(vertices, edges.ToList(), quads, triangles);
        }
    }
}
