namespace FastGeoMesh.Domain {
    /// <summary>Interface for geometry-only elements that may inform meshing.</summary>
    public interface IElement {
        /// <summary>Semantic kind of the element (e.g., SegmentAtZ, Beam, Tie).</summary>
        string Kind { get; }
    }
}
