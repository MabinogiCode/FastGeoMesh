namespace FastGeoMesh.Domain
{
    /// <summary>Internal horizontal surface (plate) with local holes at a specific elevation.</summary>
    public sealed class InternalSurfaceDefinition
    {
        /// <summary>Outer boundary polygon (CCW enforced by Polygon2D).</summary>
        public Polygon2D Outer { get; }
        /// <summary>Inner hole polygons (each CCW, not extruded).</summary>
        public IReadOnlyList<Polygon2D> Holes { get; }
        /// <summary>Elevation (Z) at which the surface lies.</summary>
        public double Elevation { get; }
        /// <summary>Create an internal surface.</summary>
        /// <param name="outer">Outer polygon.</param>
        /// <param name="elevation">Z elevation (must be strictly between prism base/top).</param>
        /// <param name="holes">Optional hole polygons.</param>
        public InternalSurfaceDefinition(Polygon2D outer, double elevation, IEnumerable<Polygon2D>? holes = null)
        {
            Outer = outer ?? throw new ArgumentNullException(nameof(outer));
            Elevation = elevation;
            Holes = holes is null ? Array.Empty<Polygon2D>() : new List<Polygon2D>(holes);
        }
    }
}
