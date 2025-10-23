namespace FastGeoMesh.Domain {
    /// <summary>2D line segment defined by end points A and B.</summary>
    public readonly record struct Segment2D(Vec2 A, Vec2 B) {
        /// <summary>Euclidean length of the segment.</summary>
        /// <returns>Length of the segment.</returns>
        public double Length() => (B - A).Length();
    }
}
