using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing
{
    /// <summary>Mutable mesh composed of quads, triangles, stand-alone points and internal 3D segments.</summary>
    public sealed class Mesh : IDisposable
    {
        private readonly List<Quad> _quads;
        private readonly List<Triangle> _triangles;
        private readonly List<Vec3> _points;
        private readonly List<Segment3D> _internalSegments;

        // Use simple lock for better performance than ReaderWriterLockSlim
        private readonly object _lock = new();

        // 🚀 OPTIMIZATION: Simple caching that's proven to work without overhead
        private ReadOnlyCollection<Quad>? _quadsReadOnly;
        private ReadOnlyCollection<Triangle>? _trianglesReadOnly;
        private ReadOnlyCollection<Vec3>? _pointsReadOnly;
        private ReadOnlyCollection<Segment3D>? _segmentsReadOnly;

        /// <summary>Create mesh with default initial capacity.</summary>
        public Mesh() : this(initialQuadCapacity: 16, initialTriangleCapacity: 16, initialPointCapacity: 8, initialSegmentCapacity: 8)
        {
        }

        /// <summary>Create mesh with specified initial capacities to reduce allocations.</summary>
        /// <param name="initialQuadCapacity">Initial capacity for quads collection.</param>
        /// <param name="initialTriangleCapacity">Initial capacity for triangles collection.</param>
        /// <param name="initialPointCapacity">Initial capacity for points collection.</param>
        /// <param name="initialSegmentCapacity">Initial capacity for segments collection.</param>
        public Mesh(int initialQuadCapacity, int initialTriangleCapacity, int initialPointCapacity, int initialSegmentCapacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(initialQuadCapacity);
            ArgumentOutOfRangeException.ThrowIfNegative(initialTriangleCapacity);
            ArgumentOutOfRangeException.ThrowIfNegative(initialPointCapacity);
            ArgumentOutOfRangeException.ThrowIfNegative(initialSegmentCapacity);

            _quads = new List<Quad>(initialQuadCapacity);
            _triangles = new List<Triangle>(initialTriangleCapacity);
            _points = new List<Vec3>(initialPointCapacity);
            _internalSegments = new List<Segment3D>(initialSegmentCapacity);
        }

        /// <summary>Gets the total number of quads in the mesh.</summary>
        public int QuadCount
        {
            get
            {
                lock (_lock)
                {
                    return _quads.Count;
                }
            }
        }

        /// <summary>Gets the total number of triangles in the mesh.</summary>
        public int TriangleCount
        {
            get
            {
                lock (_lock)
                {
                    return _triangles.Count;
                }
            }
        }

        /// <summary>Collection of quads.</summary>
        public ReadOnlyCollection<Quad> Quads
        {
            get
            {
                if (_quadsReadOnly != null)
                {
                    return _quadsReadOnly;
                }

                lock (_lock)
                {
                    return _quadsReadOnly ??= _quads.AsReadOnly();
                }
            }
        }

        /// <summary>Collection of triangles.</summary>
        public ReadOnlyCollection<Triangle> Triangles
        {
            get
            {
                if (_trianglesReadOnly != null)
                {
                    return _trianglesReadOnly;
                }

                lock (_lock)
                {
                    return _trianglesReadOnly ??= _triangles.AsReadOnly();
                }
            }
        }

        /// <summary>Auxiliary points carried through to indexed mesh.</summary>
        public ReadOnlyCollection<Vec3> Points
        {
            get
            {
                if (_pointsReadOnly != null)
                {
                    return _pointsReadOnly;
                }

                lock (_lock)
                {
                    return _pointsReadOnly ??= _points.AsReadOnly();
                }
            }
        }

        /// <summary>Internal 3D segments preserved in the indexed mesh.</summary>
        public ReadOnlyCollection<Segment3D> InternalSegments
        {
            get
            {
                if (_segmentsReadOnly != null)
                {
                    return _segmentsReadOnly;
                }

                lock (_lock)
                {
                    return _segmentsReadOnly ??= _internalSegments.AsReadOnly();
                }
            }
        }

        /// <summary>Add a quad.</summary>
        public void AddQuad(Quad quad)
        {
            lock (_lock)
            {
                _quads.Add(quad);
                _quadsReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add multiple quads efficiently.</summary>
        /// <param name="quads">Quads to add.</param>
        public void AddQuads(IEnumerable<Quad> quads)
        {
            ArgumentNullException.ThrowIfNull(quads);

            lock (_lock)
            {
                _quads.AddRange(quads);
                _quadsReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add multiple quads from span for zero-allocation bulk operations.</summary>
        /// <param name="quads">Quads to add.</param>
        public void AddQuadsSpan(ReadOnlySpan<Quad> quads)
        {
            lock (_lock)
            {
                foreach (var quad in quads)
                {
                    _quads.Add(quad);
                }
                _quadsReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add a triangle.</summary>
        public void AddTriangle(Triangle tri)
        {
            lock (_lock)
            {
                _triangles.Add(tri);
                _trianglesReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add multiple triangles efficiently.</summary>
        /// <param name="triangles">Triangles to add.</param>
        public void AddTriangles(IEnumerable<Triangle> triangles)
        {
            ArgumentNullException.ThrowIfNull(triangles);

            lock (_lock)
            {
                _triangles.AddRange(triangles);
                _trianglesReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add multiple triangles from span for zero-allocation bulk operations.</summary>
        /// <param name="triangles">Triangles to add.</param>
        public void AddTrianglesSpan(ReadOnlySpan<Triangle> triangles)
        {
            lock (_lock)
            {
                foreach (var triangle in triangles)
                {
                    _triangles.Add(triangle);
                }
                _trianglesReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add an auxiliary point.</summary>
        public void AddPoint(Vec3 p)
        {
            lock (_lock)
            {
                _points.Add(p);
                _pointsReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add multiple points efficiently.</summary>
        /// <param name="points">Points to add.</param>
        public void AddPoints(IEnumerable<Vec3> points)
        {
            ArgumentNullException.ThrowIfNull(points);

            lock (_lock)
            {
                _points.AddRange(points);
                _pointsReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Add an internal 3D segment.</summary>
        public void AddInternalSegment(Segment3D s)
        {
            lock (_lock)
            {
                _internalSegments.Add(s);
                _segmentsReadOnly = null; // Invalidate cache
            }
        }

        /// <summary>Clear all mesh data efficiently.</summary>
        public void Clear()
        {
            lock (_lock)
            {
                _quads.Clear();
                _triangles.Clear();
                _points.Clear();
                _internalSegments.Clear();

                // Simple cache invalidation
                _quadsReadOnly = null;
                _trianglesReadOnly = null;
                _pointsReadOnly = null;
                _segmentsReadOnly = null;
            }
        }

        /// <summary>Dispose of resources.</summary>
        public void Dispose()
        {
            // Simple lock doesn't need disposal
            GC.SuppressFinalize(this);
        }
    }
}
