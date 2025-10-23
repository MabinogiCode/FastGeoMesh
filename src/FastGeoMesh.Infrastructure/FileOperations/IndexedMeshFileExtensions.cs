using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure.FileOperations {
    /// <summary>
    /// Extension methods for IndexedMesh providing file I/O operations.
    /// These operations belong in Infrastructure layer as they involve external file system.
    /// </summary>
    public static class IndexedMeshFileExtensions {
        /// <summary>
        /// Writes the indexed mesh to a custom text format file.
        /// </summary>
        /// <param name="mesh">The mesh to write.</param>
        /// <param name="filePath">Path where to save the file.</param>
        public static void WriteCustomTxt(this IndexedMesh mesh, string filePath) {
            IndexedMeshFileOperations.WriteCustomTxt(mesh, filePath);
        }

        /// <summary>
        /// Reads an indexed mesh from a custom text format file.
        /// </summary>
        /// <param name="mesh">Unused parameter for extension method.</param>
        /// <param name="filePath">Path to the custom mesh file.</param>
        /// <returns>The loaded indexed mesh.</returns>
        public static IndexedMesh ReadCustomTxt(this IndexedMesh mesh, string filePath) {
            return IndexedMeshFileOperations.ReadCustomTxt(filePath);
        }
    }

    /// <summary>
    /// Static helper methods for IndexedMesh file operations.
    /// Provides direct access to file operations without needing an instance.
    /// </summary>
    public static class IndexedMeshFileOperations {
        /// <summary>
        /// Reads an indexed mesh from a custom text format file.
        /// </summary>
        /// <param name="filePath">Path to the custom mesh file.</param>
        /// <returns>The loaded indexed mesh.</returns>
        public static IndexedMesh ReadCustomTxt(string filePath) {
            return IndexedMeshFileHelper.ReadCustomTxt(filePath);
        }

        /// <summary>
        /// Writes an indexed mesh to a custom text format file.
        /// </summary>
        /// <param name="mesh">The mesh to write.</param>
        /// <param name="filePath">Path where to save the file.</param>
        public static void WriteCustomTxt(IndexedMesh mesh, string filePath) {
            IndexedMeshFileHelper.WriteCustomTxt(mesh, filePath);
        }

        /// <summary>
        /// Reads an indexed mesh from an alternative custom text format file.
        /// </summary>
        /// <param name="filePath">Path to the custom mesh file.</param>
        /// <returns>The loaded indexed mesh.</returns>
        public static IndexedMesh ReadCustomTxtAlternative(string filePath) {
            return IndexedMeshFileHelper.ReadCustomTxtAlternative(filePath);
        }

        /// <summary>
        /// Writes an indexed mesh to an alternative custom text format file.
        /// </summary>
        /// <param name="mesh">The mesh to write.</param>
        /// <param name="filePath">Path where to save the file.</param>
        public static void WriteCustomTxtAlternative(IndexedMesh mesh, string filePath) {
            IndexedMeshFileHelper.WriteCustomTxtAlternative(mesh, filePath);
        }
    }
}
