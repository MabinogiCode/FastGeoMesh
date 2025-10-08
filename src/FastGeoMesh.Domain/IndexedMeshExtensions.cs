namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Extension methods for IndexedMesh to provide backward compatibility and convenience methods.
    /// </summary>
    public static class IndexedMeshExtensions
    {
        /// <summary>
        /// Builds adjacency information for the indexed mesh.
        /// </summary>
        /// <param name="mesh">The indexed mesh.</param>
        /// <returns>Mesh adjacency information.</returns>
        public static MeshAdjacency BuildAdjacency(this IndexedMesh mesh)
        {
            return MeshAdjacency.Build(mesh);
        }

        /// <summary>
        /// Writes the indexed mesh to a custom text format file.
        /// </summary>
        /// <param name="mesh">The mesh to write.</param>
        /// <param name="filePath">Path where to save the file.</param>
        public static void WriteCustomTxt(this IndexedMesh mesh, string filePath)
        {
            IndexedMeshFileHelper.WriteCustomTxt(mesh, filePath);
        }
    }

    /// <summary>
    /// Static helper methods for IndexedMesh to maintain API compatibility.
    /// </summary>
    public static class IndexedMeshHelper
    {
        /// <summary>
        /// Reads an indexed mesh from a custom text format file.
        /// </summary>
        /// <param name="filePath">Path to the custom mesh file.</param>
        /// <returns>The loaded indexed mesh.</returns>
        public static IndexedMesh ReadCustomTxt(string filePath)
        {
            return IndexedMeshFileHelper.ReadCustomTxt(filePath);
        }
    }
}
