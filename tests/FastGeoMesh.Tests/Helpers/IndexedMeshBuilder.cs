using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Test helper for building indexed meshes in a fluent manner.
    /// Used for testing mesh adjacency and other mesh operations.
    /// </summary>
    internal sealed class IndexedMeshBuilder
    {
        private readonly List<Vec3> _verts = new();
        private readonly List<(int, int, int, int)> _quads = new();

        /// <summary>Add a vertex to the builder.</summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <returns>This builder for chaining.</returns>
        public IndexedMeshBuilder AddVertex(double x, double y, double z)
        {
            _verts.Add(new Vec3(x, y, z));
            return this;
        }

        /// <summary>Add a quad using vertex indices.</summary>
        /// <param name="v0">Index of first vertex.</param>
        /// <param name="v1">Index of second vertex.</param>
        /// <param name="v2">Index of third vertex.</param>
        /// <param name="v3">Index of fourth vertex.</param>
        /// <returns>This builder for chaining.</returns>
        public IndexedMeshBuilder AddQuad(int v0, int v1, int v2, int v3)
        {
            _quads.Add((v0, v1, v2, v3));
            return this;
        }

        /// <summary>Build the final indexed mesh.</summary>
        /// <returns>An IndexedMesh containing the added vertices and quads.</returns>
        public IndexedMesh Build()
        {
            var mesh = new Mesh();
            var verts = _verts.ToArray();

            foreach (var q in _quads)
            {
                var quad = new Quad(verts[q.Item1], verts[q.Item2], verts[q.Item3], verts[q.Item4]);
                mesh.AddQuad(quad);
            }

            return IndexedMesh.FromMesh(mesh);
        }
    }
}
