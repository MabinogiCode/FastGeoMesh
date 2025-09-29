using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        // Use ReaderWriterLockSlim for better read performance
        private readonly ReaderWriterLockSlim _lock = new();

        // Lazy initialization with double-check locking
        private ReadOnlyCollection<Quad>? _quadsReadOnly;
        private ReadOnlyCollection<Triangle>? _trianglesReadOnly;
        private ReadOnlyCollection<Vec3>? _pointsReadOnly;
        private ReadOnlyCollection<Segment3D>? _internalSegmentsReadOnly;

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

        /// <summary>Collection of quads.</summary>
        public ReadOnlyCollection<Quad> Quads
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _quadsReadOnly ??= _quads.AsReadOnly();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>Collection of triangles.</summary>
        public ReadOnlyCollection<Triangle> Triangles
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _trianglesReadOnly ??= _triangles.AsReadOnly();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>Auxiliary points carried through to indexed mesh.</summary>
        public ReadOnlyCollection<Vec3> Points
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _pointsReadOnly ??= _points.AsReadOnly();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>Internal 3D segments preserved in the indexed mesh.</summary>
        public ReadOnlyCollection<Segment3D> InternalSegments
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _internalSegmentsReadOnly ??= _internalSegments.AsReadOnly();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>Add a quad.</summary>
        public void AddQuad(Quad quad)
        {
            _lock.EnterWriteLock();
            try
            {
                _quads.Add(quad);
                _quadsReadOnly = null; // Only invalidate quads cache
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add multiple quads efficiently.</summary>
        /// <param name="quads">Quads to add.</param>
        public void AddQuads(IEnumerable<Quad> quads)
        {
            ArgumentNullException.ThrowIfNull(quads);

            _lock.EnterWriteLock();
            try
            {
                _quads.AddRange(quads);
                _quadsReadOnly = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add a triangle.</summary>
        public void AddTriangle(Triangle tri)
        {
            _lock.EnterWriteLock();
            try
            {
                _triangles.Add(tri);
                _trianglesReadOnly = null; // Only invalidate triangles cache
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add multiple triangles efficiently.</summary>
        /// <param name="triangles">Triangles to add.</param>
        public void AddTriangles(IEnumerable<Triangle> triangles)
        {
            ArgumentNullException.ThrowIfNull(triangles);

            _lock.EnterWriteLock();
            try
            {
                _triangles.AddRange(triangles);
                _trianglesReadOnly = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add an auxiliary point.</summary>
        public void AddPoint(Vec3 p)
        {
            _lock.EnterWriteLock();
            try
            {
                _points.Add(p);
                _pointsReadOnly = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add an internal 3D segment.</summary>
        public void AddInternalSegment(Segment3D s)
        {
            _lock.EnterWriteLock();
            try
            {
                _internalSegments.Add(s);
                _internalSegmentsReadOnly = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Clear all mesh data.</summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _quads.Clear();
                _triangles.Clear();
                _points.Clear();
                _internalSegments.Clear();

                _quadsReadOnly = null;
                _trianglesReadOnly = null;
                _pointsReadOnly = null;
                _internalSegmentsReadOnly = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Dispose of resources.</summary>
        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
