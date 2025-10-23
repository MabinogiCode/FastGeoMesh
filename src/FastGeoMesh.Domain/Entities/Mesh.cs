using System.Collections.ObjectModel;

namespace FastGeoMesh.Domain {
    /// <summary>
    /// Mutable mesh implementation for efficient construction of geometry.
    /// Provides a builder pattern for assembling quads, triangles, and points before conversion to immutable forms.
    /// </summary>
    public sealed class Mesh : IDisposable {
        private readonly List<Quad> _quads;
        private readonly List<Triangle> _triangles;
        private readonly List<Vec3> _points;
        private readonly List<Segment3D> _internalSegments;
        private ReadOnlyCollection<Quad>? _quadsReadOnly;
        private ReadOnlyCollection<Triangle>? _trianglesReadOnly;
        private ReadOnlyCollection<Vec3>? _pointsReadOnly;
        private ReadOnlyCollection<Segment3D>? _segmentsReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh() {
            _quads = new List<Quad>();
            _triangles = new List<Triangle>();
            _points = new List<Vec3>();
            _internalSegments = new List<Segment3D>();
        }

        /// <summary>
        /// Gets the collection of quads in the mesh.
        /// </summary>
        public ReadOnlyCollection<Quad> Quads => _quadsReadOnly ??= _quads.AsReadOnly();

        /// <summary>
        /// Gets the collection of triangles in the mesh.
        /// </summary>
        public ReadOnlyCollection<Triangle> Triangles => _trianglesReadOnly ??= _triangles.AsReadOnly();

        /// <summary>
        /// Gets the collection of points in the mesh.
        /// </summary>
        public ReadOnlyCollection<Vec3> Points => _pointsReadOnly ??= _points.AsReadOnly();

        /// <summary>
        /// Gets the collection of internal segments in the mesh.
        /// </summary>
        public ReadOnlyCollection<Segment3D> InternalSegments => _segmentsReadOnly ??= _internalSegments.AsReadOnly();

        /// <summary>
        /// Gets the number of quads in the mesh.
        /// </summary>
        public int QuadCount => _quads.Count;

        /// <summary>
        /// Gets the number of triangles in the mesh.
        /// </summary>
        public int TriangleCount => _triangles.Count;

        /// <summary>
        /// Gets the number of points in the mesh.
        /// </summary>
        public int PointCount => _points.Count;

        /// <summary>
        /// Gets the number of internal segments in the mesh.
        /// </summary>
        public int InternalSegmentCount => _internalSegments.Count;

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        /// <param name="quad">The quad to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddQuad(Quad quad) {
            _quads.Add(quad);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds multiple quads to the mesh efficiently.
        /// </summary>
        /// <param name="quads">The quads to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddQuads(IEnumerable<Quad> quads) {
            ArgumentNullException.ThrowIfNull(quads);
            _quads.AddRange(quads);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        /// <param name="triangle">The triangle to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddTriangle(Triangle triangle) {
            _triangles.Add(triangle);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds multiple triangles to the mesh efficiently.
        /// </summary>
        /// <param name="triangles">The triangles to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddTriangles(IEnumerable<Triangle> triangles) {
            ArgumentNullException.ThrowIfNull(triangles);
            _triangles.AddRange(triangles);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds a point to the mesh.
        /// </summary>
        /// <param name="point">The point to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddPoint(Vec3 point) {
            _points.Add(point);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds multiple points to the mesh efficiently.
        /// </summary>
        /// <param name="points">The points to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddPoints(IEnumerable<Vec3> points) {
            ArgumentNullException.ThrowIfNull(points);
            _points.AddRange(points);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds an internal segment to the mesh.
        /// </summary>
        /// <param name="segment">The segment to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddInternalSegment(Segment3D segment) {
            _internalSegments.Add(segment);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Adds multiple internal segments to the mesh efficiently.
        /// </summary>
        /// <param name="segments">The segments to add.</param>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh AddInternalSegments(IEnumerable<Segment3D> segments) {
            ArgumentNullException.ThrowIfNull(segments);
            _internalSegments.AddRange(segments);
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Converts this mutable mesh to an immutable mesh.
        /// </summary>
        /// <returns>An immutable mesh containing the same geometry.</returns>
        public ImmutableMesh ToImmutableMesh() {
            var immutable = new ImmutableMesh();

            foreach (var quad in _quads) {
                immutable = immutable.AddQuad(quad);
            }

            foreach (var triangle in _triangles) {
                immutable = immutable.AddTriangle(triangle);
            }

            foreach (var point in _points) {
                immutable = immutable.AddPoint(point);
            }

            foreach (var segment in _internalSegments) {
                immutable = immutable.AddInternalSegment(segment);
            }

            return immutable;
        }

        /// <summary>
        /// Clears all geometry from the mesh.
        /// </summary>
        /// <returns>This mesh instance for method chaining.</returns>
        public Mesh Clear() {
            _quads.Clear();
            _triangles.Clear();
            _points.Clear();
            _internalSegments.Clear();
            InvalidateCache();
            return this;
        }

        /// <summary>
        /// Invalidates cached read-only collections when the mesh is modified.
        /// </summary>
        private void InvalidateCache() {
            _quadsReadOnly = null;
            _trianglesReadOnly = null;
            _pointsReadOnly = null;
            _segmentsReadOnly = null;
        }

        /// <summary>
        /// Disposes the mesh and releases resources.
        /// </summary>
        public void Dispose() {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}
