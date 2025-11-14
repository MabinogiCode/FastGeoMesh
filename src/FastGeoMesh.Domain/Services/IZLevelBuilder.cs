namespace FastGeoMesh.Domain.Services
{
    /// <summary>
    /// Service responsible for building Z-level subdivisions for prism meshing.
    /// Generates sorted, distinct Z-levels based on target edge lengths and geometry constraints.
    /// </summary>
    public interface IZLevelBuilder
    {
        /// <summary>
        /// Builds a sorted, distinct list of Z levels for prism subdivision.
        /// </summary>
        /// <param name="z0">Bottom elevation.</param>
        /// <param name="z1">Top elevation.</param>
        /// <param name="options">Meshing options containing target edge lengths and tolerance.</param>
        /// <param name="structure">Prism structure definition containing geometry and constraints.</param>
        /// <returns>Sorted list of Z levels for meshing.</returns>
        IReadOnlyList<double> BuildZLevels(double z0, double z1, MesherOptions options, PrismStructureDefinition structure);
    }
}
