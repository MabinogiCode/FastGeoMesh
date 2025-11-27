using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests.Helpers
{
    internal sealed class IndexedMeshBuilder
    {
        private readonly List<Vec3> _verts = new();
        private readonly List<(int, int, int, int)> _quads = new();
        /// <summary>
        /// Runs test AddVertex.
        /// </summary>
        public IndexedMeshBuilder AddVertex(double x, double y, double z)
        {
            _verts.Add(new Vec3(x, y, z));
            return this;
        }
        /// <summary>
        /// Runs test AddQuad.
        /// </summary>
        public IndexedMeshBuilder AddQuad(int v0, int v1, int v2, int v3)
        {
            _quads.Add((v0, v1, v2, v3));
            return this;
        }
        /// <summary>
        /// Runs test Build.
        /// </summary>
        public IndexedMesh Build()
        {
            using var mesh = new Mesh();
            var verts = _verts.ToArray();

            foreach (var q in _quads)
            {
                var quad = new Quad(verts[q.Item1], verts[q.Item2], verts[q.Item3], verts[q.Item4]);
                mesh.AddQuad(quad);
            }

            return IndexedMesh.FromMesh(mesh.ToImmutableMesh());
        }
    }
}
