using System.Collections.Immutable;

namespace FastGeoMesh.Domain
{
    /// <summary>Immutable mesh composed of quads, triangles, stand-alone points and internal 3D segments.</summary>
    public sealed class Mesh
    {
        /// <summary>An empty mesh.</summary>
        public static readonly Mesh Empty = new();

        private readonly ImmutableList<Quad> _quads;
        private readonly ImmutableList<Triangle> _triangles;
        private readonly ImmutableList<Vec3> _points;
        private readonly ImmutableList<Segment3D> _internalSegments;

        /// <summary>Create an empty mesh.</summary>
        public Mesh()
        {
            _quads = ImmutableList<Quad>.Empty;
            _triangles = ImmutableList<Triangle>.Empty;
            _points = ImmutableList<Vec3>.Empty;
            _internalSegments = ImmutableList<Segment3D>.Empty;
        }

        /// <summary>Initializes a new instance of the <see cref="Mesh"/> class.</summary>
        public Mesh(
            IEnumerable<Quad> quads,
            IEnumerable<Triangle> triangles,
            IEnumerable<Vec3> points,
            IEnumerable<Segment3D> internalSegments)
        {
            _quads = quads.ToImmutableList();
            _triangles = triangles.ToImmutableList();
            _points = points.ToImmutableList();
            _internalSegments = internalSegments.ToImmutableList();
        }

        private Mesh(
            ImmutableList<Quad> quads,
            ImmutableList<Triangle> triangles,
            ImmutableList<Vec3> points,
            ImmutableList<Segment3D> internalSegments)
        {
            _quads = quads;
            _triangles = triangles;
            _points = points;
            _internalSegments = internalSegments;
        }

        /// <summary>Gets the total number of quads in the mesh.</summary>
        public int QuadCount => _quads.Count;

        /// <summary>Gets the total number of triangles in the mesh.</summary>
        public int TriangleCount => _triangles.Count;

        /// <summary>Collection of quads.</summary>
        public IReadOnlyList<Quad> Quads => _quads;

        /// <summary>Collection of triangles.</summary>
        public IReadOnlyList<Triangle> Triangles => _triangles;

        /// <summary>Auxiliary points carried through to indexed mesh.</summary>
        public IReadOnlyList<Vec3> Points => _points;

        /// <summary>Internal 3D segments preserved in the indexed mesh.</summary>
        public IReadOnlyList<Segment3D> InternalSegments => _internalSegments;

        /// <summary>Returns a new mesh with the given quad added.</summary>
        public Mesh AddQuad(Quad quad)
        {
            return new Mesh(_quads.Add(quad), _triangles, _points, _internalSegments);
        }

        /// <summary>Returns a new mesh with the given quads added.</summary>
        /// <param name="quads">Quads to add.</param>
        public Mesh AddQuads(IEnumerable<Quad> quads)
        {
            ArgumentNullException.ThrowIfNull(quads);
            return new Mesh(_quads.AddRange(quads), _triangles, _points, _internalSegments);
        }

        /// <summary>Add multiple quads from span for zero-allocation bulk operations.</summary>
        /// <param name="quads">Quads to add.</param>
        public Mesh AddQuadsSpan(ReadOnlySpan<Quad> quads)
        {
            var builder = _quads.ToBuilder();
            foreach (var quad in quads)
            {
                builder.Add(quad);
            }
            return new Mesh(builder.ToImmutable(), _triangles, _points, _internalSegments);
        }

        /// <summary>Returns a new mesh with the given triangle added.</summary>
        public Mesh AddTriangle(Triangle tri)
        {
            return new Mesh(_quads, _triangles.Add(tri), _points, _internalSegments);
        }

        /// <summary>Returns a new mesh with the given triangles added.</summary>
        /// <param name="triangles">Triangles to add.</param>
        public Mesh AddTriangles(IEnumerable<Triangle> triangles)
        {
            ArgumentNullException.ThrowIfNull(triangles);
            return new Mesh(_quads, _triangles.AddRange(triangles), _points, _internalSegments);
        }

        /// <summary>Add multiple triangles from span for zero-allocation bulk operations.</summary>
        /// <param name="triangles">Triangles to add.</param>
        public Mesh AddTrianglesSpan(ReadOnlySpan<Triangle> triangles)
        {
            var builder = _triangles.ToBuilder();
            foreach (var triangle in triangles)
            {
                builder.Add(triangle);
            }
            return new Mesh(_quads, builder.ToImmutable(), _points, _internalSegments);
        }

        /// <summary>Returns a new mesh with the given point added.</summary>
        public Mesh AddPoint(Vec3 p)
        {
            return new Mesh(_quads, _triangles, _points.Add(p), _internalSegments);
        }

        /// <summary>Returns a new mesh with the given points added.</summary>
        /// <param name="points">Points to add.</param>
        public Mesh AddPoints(IEnumerable<Vec3> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            return new Mesh(_quads, _triangles, _points.AddRange(points), _internalSegments);
        }

        /// <summary>Returns a new mesh with the given internal segment added.</summary>
        public Mesh AddInternalSegment(Segment3D s)
        {
            return new Mesh(_quads, _triangles, _points, _internalSegments.Add(s));
        }
    }
}
