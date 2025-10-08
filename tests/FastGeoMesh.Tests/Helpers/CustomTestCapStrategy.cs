using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Custom test cap strategy implementation for testing PrismMesher with dependency injection.
    /// Provides a simple test implementation that tracks whether it was called during meshing operations.
    /// </summary>
    internal sealed class CustomTestCapStrategy : ICapMeshingStrategy
    {
        /// <summary>
        /// Gets a value indicating whether the strategy was called during cap generation.
        /// </summary>
        public bool WasCalled { get; private set; }

        /// <summary>
        /// Generates cap geometry for testing purposes.
        /// Returns empty geometry but tracks that the method was invoked.
        /// </summary>
        /// <param name="definition">The prism structure definition.</param>
        /// <param name="options">The meshing options.</param>
        /// <param name="z0">The base elevation.</param>
        /// <param name="z1">The top elevation.</param>
        /// <returns>Empty cap geometry for testing purposes.</returns>
        public CapGeometry GenerateCaps(PrismStructureDefinition definition, MesherOptions options, double z0, double z1)
        {
            WasCalled = true;
            // Simple implementation for testing - return empty cap geometry
            return new CapGeometry(
                Array.Empty<Quad>(),
                Array.Empty<Triangle>()
            );
        }
    }
}
