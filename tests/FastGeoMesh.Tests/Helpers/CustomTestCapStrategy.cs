using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests.Helpers
{
    internal sealed class CustomTestCapStrategy : ICapMeshingStrategy
    {
        /// <summary>
        /// Property WasCalled used in tests.
        /// </summary>
        public bool WasCalled { get; private set; }
        /// <summary>
        /// Runs test GenerateCaps.
        /// </summary>
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
