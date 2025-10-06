namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Classification of meshing operation complexity.
    /// </summary>
    public enum MeshingComplexity
    {
        /// <summary>Simple geometry with minimal computational requirements.</summary>
        Trivial,
        /// <summary>Straightforward geometry suitable for synchronous processing.</summary>
        Simple,
        /// <summary>Moderate complexity benefiting from async processing.</summary>
        Moderate,
        /// <summary>Complex geometry requiring parallel processing.</summary>
        Complex,
        /// <summary>Very large or intricate geometry requiring advanced optimization.</summary>
        Extreme
    }
}
