namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Helper for building mesh adjacency information from indexed meshes.
    /// Actually, MeshAdjacency already has a Build method, so this helper provides extensions.
    /// </summary>
    public static class IndexedMeshAdjacencyHelper
    {
        /// <summary>
        /// Builds adjacency information for the given indexed mesh.
        /// This is a convenience wrapper around MeshAdjacency.Build.
        /// </summary>
        /// <param name="mesh">The indexed mesh to analyze.</param>
        /// <returns>Mesh adjacency information including manifold and non-manifold edges.</returns>
        public static MeshAdjacency BuildAdjacency(IndexedMesh mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            return MeshAdjacency.Build(mesh);
        }
    }
}
