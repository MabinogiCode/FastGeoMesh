using System.Collections.ObjectModel;
using FastGeoMesh.Meshing;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Mesh implementation WITHOUT intelligent caching for performance comparison.
    /// This represents the behavior before optimization.
    /// </summary>
    internal sealed class NoCacheMesh : IDisposable
    {
        private readonly List<Quad> _quads;
        private readonly List<Triangle> _triangles;
        private readonly object _lock = new();

        public NoCacheMesh()
        {
            _quads = new List<Quad>();
            _triangles = new List<Triangle>();
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

        /// <summary>Collection of quads - NO CACHING (creates new ReadOnlyCollection every time).</summary>
        public ReadOnlyCollection<Quad> Quads
        {
            get
            {
                lock (_lock)
                {
                    // NO CACHING - create new ReadOnlyCollection every access
                    return _quads.AsReadOnly();
                }
            }
        }

        /// <summary>Collection of triangles - NO CACHING.</summary>
        public ReadOnlyCollection<Triangle> Triangles
        {
            get
            {
                lock (_lock)
                {
                    // NO CACHING - create new ReadOnlyCollection every access
                    return _triangles.AsReadOnly();
                }
            }
        }

        /// <summary>Add a quad.</summary>
        public void AddQuad(Quad quad)
        {
            lock (_lock)
            {
                _quads.Add(quad);
            }
        }

        /// <summary>Add multiple quads efficiently.</summary>
        public void AddQuads(IEnumerable<Quad> quads)
        {
            ArgumentNullException.ThrowIfNull(quads);

            lock (_lock)
            {
                _quads.AddRange(quads);
            }
        }

        /// <summary>Add a triangle.</summary>
        public void AddTriangle(Triangle tri)
        {
            lock (_lock)
            {
                _triangles.Add(tri);
            }
        }

        /// <summary>Add multiple triangles efficiently.</summary>
        public void AddTriangles(IEnumerable<Triangle> triangles)
        {
            ArgumentNullException.ThrowIfNull(triangles);

            lock (_lock)
            {
                _triangles.AddRange(triangles);
            }
        }

        /// <summary>Clear all mesh data.</summary>
        public void Clear()
        {
            lock (_lock)
            {
                _quads.Clear();
                _triangles.Clear();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
