namespace FastGeoMesh.Domain {
    /// <summary>2D segment tagged with a Z elevation level.</summary>
    public sealed class SegmentAtZ : IElement {
        /// <summary>Gets the discriminator value.</summary>
        public string Kind => nameof(SegmentAtZ);

        /// <summary>Gets the 2D segment in footprint plane.</summary>
        public Segment2D Segment { get; }

        /// <summary>Gets the Z elevation associated to the segment.</summary>
        public double Z { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentAtZ"/> class.
        /// </summary>
        public SegmentAtZ(Segment2D segment, double z) {
            this.Segment = segment;
            this.Z = z;
        }
    }
}
