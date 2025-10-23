namespace FastGeoMesh.Domain {
    /// <summary>
    /// Defines a strategy for generating top and bottom caps for a prism structure.
    /// </summary>
    public interface ICapMeshingStrategy {
        /// <summary>
        /// Generates the cap geometry for a prism structure.
        /// </summary>
        /// <param name="definition">The prism structure definition.</param>
        /// <param name="options">The meshing options.</param>
        /// <param name="z0">The base elevation.</param>
        /// <param name="z1">The top elevation.</param>
        /// <returns>The generated cap geometry.</returns>
        CapGeometry GenerateCaps(PrismStructureDefinition definition, MesherOptions options, double z0, double z1);
    }
}
