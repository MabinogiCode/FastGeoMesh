namespace FastGeoMesh.Application.Helpers.Meshing
{
    /// <summary>
    /// Structure to store adaptive grid divisions for cap meshing.
    /// </summary>
    internal readonly struct AdaptiveGrid
    {
        /// <summary>
        /// Gets the X-axis division coordinates.
        /// </summary>
        public IReadOnlyList<double> XDivisions { get; init; }

        /// <summary>
        /// Gets the Y-axis division coordinates.
        /// </summary>
        public IReadOnlyList<double> YDivisions { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveGrid"/> struct.
        /// </summary>
        /// <param name="xDivisions">The X-axis division coordinates.</param>
        /// <param name="yDivisions">The Y-axis division coordinates.</param>
        public AdaptiveGrid(IReadOnlyList<double> xDivisions, IReadOnlyList<double> yDivisions)
        {
            XDivisions = xDivisions;
            YDivisions = yDivisions;
        }
    }
}
