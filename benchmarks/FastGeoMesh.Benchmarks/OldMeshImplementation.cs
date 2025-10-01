using System.Collections.Generic;
using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;

namespace FastGeoMesh.Benchmarks
{
    /// <summary>
    /// Simulation of the old Mesh implementation using ReaderWriterLockSlim
    /// for performance comparison purposes.
    /// </summary>
    public sealed class OldMeshImplementation : IDisposable
    {
        private readonly List<Quad> _quads;
        private readonly List<Triangle> _triangles;

        // OLD: Using ReaderWriterLockSlim
        private readonly ReaderWriterLockSlim _lock = new();

        // OLD: Manual cache invalidation
        private ReadOnlyCollection<Quad>? _quadsReadOnly;
        private ReadOnlyCollection<Triangle>? _trianglesReadOnly;

        public OldMeshImplementation()
        {
            _quads = new List<Quad>();
            _triangles = new List<Triangle>();
        }

        /// <summary>Collection of quads - OLD implementation.</summary>
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

        /// <summary>Collection of triangles - OLD implementation.</summary>
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

        /// <summary>Add a quad - OLD implementation.</summary>
        public void AddQuad(Quad quad)
        {
            _lock.EnterWriteLock();
            try
            {
                _quads.Add(quad);
                _quadsReadOnly = null; // Manual cache invalidation
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add multiple quads - OLD implementation.</summary>
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

        /// <summary>Add a triangle - OLD implementation.</summary>
        public void AddTriangle(Triangle tri)
        {
            _lock.EnterWriteLock();
            try
            {
                _triangles.Add(tri);
                _trianglesReadOnly = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Add multiple triangles - OLD implementation.</summary>
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

        /// <summary>Get quad count - OLD implementation (via collection access).</summary>
        public int GetQuadCount() => Quads.Count;

        /// <summary>Get triangle count - OLD implementation (via collection access).</summary>
        public int GetTriangleCount() => Triangles.Count;

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
