using System.Collections.Immutable;

namespace FastGeoMesh.Domain {
    /// <summary>Immutable mesh composed of quads, triangles, stand-alone points and internal 3D segments.</summary>
    public sealed class ImmutableMesh {
        /// <summary>An empty mesh.</summary>
        public static readonly ImmutableMesh Empty = new();

        private readonly ImmutableList<Quad> _quads;
        private readonly ImmutableList<Triangle> _triangles;
        private readonly ImmutableList<Vec3> _points;
        private readonly ImmutableList<Segment3D> _internalSegments;

        /// <summary>Create an empty mesh.</summary>
        public ImmutableMesh() {
            _quads = ImmutableList<Quad>.Empty;
            _triangles = ImmutableList<Triangle>.Empty;
            _points = ImmutableList<Vec3>.Empty;
            _internalSegments = ImmutableList<Segment3D>.Empty;
        }

        /// <summary>Initializes a new instance of the <see cref="ImmutableMesh"/> class.</summary>
        public ImmutableMesh(
            IEnumerable<Quad> quads,
            IEnumerable<Triangle> triangles,
            IEnumerable<Vec3> points,
            IEnumerable<Segment3D> internalSegments) {
            _quads = quads.ToImmutableList();
            _triangles = triangles.ToImmutableList();
            _points = points.ToImmutableList();
            _internalSegments = internalSegments.ToImmutableList();
        }

        private ImmutableMesh(
            ImmutableList<Quad> quads,
            ImmutableList<Triangle> triangles,
            ImmutableList<Vec3> points,
            ImmutableList<Segment3D> internalSegments) {
            _quads = quads;
            _triangles = triangles;
            _points = points;
            _internalSegments = internalSegments;
        }

        /// <summary>Gets the total number of quads in the mesh.</summary>
        public int QuadCount => this._quads.Count;

        /// <summary>Gets the total number of triangles in the mesh.</summary>
        public int TriangleCount => this._triangles.Count;

        /// <summary>Collection of quads.</summary>
        public IReadOnlyList<Quad> Quads => this._quads;

        /// <summary>Collection of triangles.</summary>
        public IReadOnlyList<Triangle> Triangles => this._triangles;

        /// <summary>Auxiliary points carried through to indexed mesh.</summary>
        public IReadOnlyList<Vec3> Points => this._points;

        /// <summary>Internal 3D segments preserved in the indexed mesh.</summary>
        public IReadOnlyList<Segment3D> InternalSegments => this._internalSegments;

        /// <summary>Returns a new mesh with the given quad added.</summary>
        public ImmutableMesh AddQuad(Quad quad) {
            return new ImmutableMesh(this._quads.Add(quad), this._triangles, this._points, this._internalSegments);
        }

        /// <summary>Returns a new mesh with the given quads added.</summary>
        public ImmutableMesh AddQuads(IEnumerable<Quad> quads) {
            ArgumentNullException.ThrowIfNull(quads);
            return new ImmutableMesh(this._quads.AddRange(quads), this._triangles, this._points, this._internalSegments);
        }

        /// <summary>Returns a new mesh with the given triangle added.</summary>
        public ImmutableMesh AddTriangle(Triangle triangle) {
            return new ImmutableMesh(this._quads, this._triangles.Add(triangle), this._points, this._internalSegments);
        }

        /// <summary>Returns a new mesh with the given triangles added.</summary>
        public ImmutableMesh AddTriangles(IEnumerable<Triangle> triangles) {
            ArgumentNullException.ThrowIfNull(triangles);
            return new ImmutableMesh(this._quads, this._triangles.AddRange(triangles), this._points, this._internalSegments);
        }

        /// <summary>Returns a new mesh with the given point added.</summary>
        public ImmutableMesh AddPoint(Vec3 point) {
            return new ImmutableMesh(this._quads, this._triangles, this._points.Add(point), this._internalSegments);
        }

        /// <summary>Returns a new mesh with the given points added.</summary>
        public ImmutableMesh AddPoints(IEnumerable<Vec3> points) {
            ArgumentNullException.ThrowIfNull(points);
            return new ImmutableMesh(this._quads, this._triangles, this._points.AddRange(points), this._internalSegments);
        }

        /// <summary>Returns a new mesh with the given internal segment added.</summary>
        public ImmutableMesh AddInternalSegment(Segment3D segment) {
            return new ImmutableMesh(this._quads, this._triangles, this._points, this._internalSegments.Add(segment));
        }

        /// <summary>Returns a new mesh with the given internal segments added.</summary>
        public ImmutableMesh AddInternalSegments(IEnumerable<Segment3D> segments) {
            ArgumentNullException.ThrowIfNull(segments);
            return new ImmutableMesh(this._quads, this._triangles, this._points, this._internalSegments.AddRange(segments));
        }

        /// <summary>Determines whether two mesh instances are equal.</summary>
        public bool Equals(ImmutableMesh? other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this._quads.SequenceEqual(other._quads)
                   && this._triangles.SequenceEqual(other._triangles)
                   && this._points.SequenceEqual(other._points)
                   && this._internalSegments.SequenceEqual(other._internalSegments);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ImmutableMesh);

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                var hashCode = this._quads.Aggregate(0, (current, quad) => (current * 397) ^ quad.GetHashCode());
                hashCode = this._triangles.Aggregate(hashCode, (current, triangle) => (current * 397) ^ triangle.GetHashCode());
                hashCode = this._points.Aggregate(hashCode, (current, point) => (current * 397) ^ point.GetHashCode());
                hashCode = this._internalSegments.Aggregate(hashCode, (current, segment) => (current * 397) ^ segment.GetHashCode());
                return hashCode;
            }
        }

        /// <summary>Returns a string representation of this mesh.</summary>
        public override string ToString() {
            return $"ImmutableMesh: {this.QuadCount} quads, {this.TriangleCount} triangles, {this.Points.Count} points, {this.InternalSegments.Count} segments";
        }
    }
}
