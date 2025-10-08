namespace FastGeoMesh.Infrastructure
{
    /// <summary>Represents the classification result of a spatial grid cell for polygon queries.</summary>
    internal enum CellResult : byte
    {
        /// <summary>Cell classification unknown, requires detailed computation.</summary>
        Unknown = 0,
        /// <summary>Cell is entirely inside the polygon.</summary>
        Inside = 1,
        /// <summary>Cell is entirely outside the polygon.</summary>
        Outside = 2,
        /// <summary>Cell crosses the polygon boundary.</summary>
        Boundary = 3
    }
}
