namespace FastGeoMesh.Domain {
    /// <summary>
    /// Core meshing interface for converting structure definitions into geometric meshes.
    /// Follows .NET 8 async best practices with ValueTask for better performance.
    /// </summary>
    /// <typeparam name="TStructure">The type of structure definition to mesh.</typeparam>
    public interface IMesher<in TStructure>
        where TStructure : notnull {
        /// <summary>
        /// Generates a mesh from the given structure definition using the specified options.
        /// </summary>
        /// <param name="input">The structure definition to convert into a mesh.</param>
        /// <param name="options">Configuration options controlling the meshing process.</param>
        /// <returns>The generated mesh result containing vertices, quads, and triangles or an error.</returns>
        Result<ImmutableMesh> Mesh(TStructure input, MesherOptions options);

        /// <summary>
        /// Asynchronously generates a mesh from the given structure definition.
        /// </summary>
        /// <param name="input">The structure definition to convert into a mesh.</param>
        /// <param name="options">Configuration options controlling the meshing process.</param>
        /// <param name="cancellationToken">Token to cancel the meshing operation.</param>
        /// <returns>A ValueTask containing the generated mesh result.</returns>
        ValueTask<Result<ImmutableMesh>> MeshAsync(TStructure input, MesherOptions options, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Specialized meshing interface for prism structure definitions.
    /// </summary>
    public interface IPrismMesher : IMesher<PrismStructureDefinition> {
    }
}
