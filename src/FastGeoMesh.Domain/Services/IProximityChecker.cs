namespace FastGeoMesh.Domain.Services
{
    /// <summary>
    /// Service for checking proximity of points to geometric features (holes, segments, etc.).
    /// Used for adaptive mesh refinement near important features.
    /// </summary>
    public interface IProximityChecker
    {
        /// <summary>
        /// Checks if a point is near any hole boundary within the given distance.
        /// </summary>
        /// <param name="structure">Prism structure containing holes.</param>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point.</param>
        /// <param name="band">Maximum distance to consider "near".</param>
        /// <param name="geometryService">Geometry service for distance calculations.</param>
        /// <returns>True if the point is within band distance of any hole boundary.</returns>
        bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService);

        /// <summary>
        /// Checks if a point is near any internal segment within the given distance.
        /// </summary>
        /// <param name="structure">Prism structure containing internal segments.</param>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point.</param>
        /// <param name="band">Maximum distance to consider "near".</param>
        /// <param name="geometryService">Geometry service for distance calculations.</param>
        /// <returns>True if the point is within band distance of any segment.</returns>
        bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService);

        /// <summary>
        /// Checks if a point is inside any hole using standard polygon test.
        /// </summary>
        /// <param name="structure">Prism structure containing holes.</param>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point.</param>
        /// <param name="geometryService">Geometry service for point-in-polygon test.</param>
        /// <returns>True if the point is inside any hole.</returns>
        bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y, IGeometryService geometryService);
    }
}
