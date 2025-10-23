namespace FastGeoMesh.Domain {
    /// <summary>
    /// Extension methods for IndexedMesh providing domain-specific operations.
    /// File operations have been moved to Infrastructure layer.
    /// </summary>
    public static class IndexedMeshExtensions {
        /// <summary>
        /// Builds adjacency information for the indexed mesh.
        /// </summary>
        /// <param name="mesh">The indexed mesh.</param>
        /// <returns>Mesh adjacency information.</returns>
        public static MeshAdjacency BuildAdjacency(this IndexedMesh mesh) {
            return MeshAdjacency.Build(mesh);
        }
    }
}
