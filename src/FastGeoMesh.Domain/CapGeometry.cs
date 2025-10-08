namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Represents the geometry of a mesh cap, containing quads and triangles.
    /// </summary>
    /// <param name="Quads">The list of quads forming the cap.</param>
    /// <param name="Triangles">The list of triangles forming the cap.</param>
    public record CapGeometry(IReadOnlyList<Quad> Quads, IReadOnlyList<Triangle> Triangles);
}
