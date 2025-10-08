namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Backward-compatibility fluent extension methods (v1.3 style) for <see cref="MesherOptionsBuilder"/>.
    /// </summary>
    public static class MesherOptionsBuilderExtensions
    {
        /// <summary>Sets the target edge length in XY plane (fluent alias).</summary>
        /// <param name="b">Builder.</param>
        /// <param name="value">Edge length (&gt; 0).</param>
        public static MesherOptionsBuilder WithTargetEdgeLengthXY(this MesherOptionsBuilder b, double value)
            => b.SetTargetEdgeLengthXY(value);

        /// <summary>Sets the target edge length along Z (fluent alias).</summary>
        /// <param name="b">Builder.</param>
        /// <param name="value">Edge length (&gt; 0).</param>
        public static MesherOptionsBuilder WithTargetEdgeLengthZ(this MesherOptionsBuilder b, double value)
            => b.SetTargetEdgeLengthZ(value);

        /// <summary>Sets whether to generate the bottom cap (fluent alias).</summary>
        /// <param name="b">Builder.</param>
        /// <param name="value">True to generate bottom cap.</param>
        public static MesherOptionsBuilder WithGenerateBottomCap(this MesherOptionsBuilder b, bool value)
            => b.SetGenerateBottomCap(value);

        /// <summary>Sets whether to generate the top cap (fluent alias).</summary>
        /// <param name="b">Builder.</param>
        /// <param name="value">True to generate top cap.</param>
        public static MesherOptionsBuilder WithGenerateTopCap(this MesherOptionsBuilder b, bool value)
            => b.SetGenerateTopCap(value);

        /// <summary>Enables/disables bottom &amp; top caps.</summary>
        /// <param name="b">Builder.</param>
        /// <param name="bottom">Generate bottom cap.</param>
        /// <param name="top">Generate top cap.</param>
        public static MesherOptionsBuilder WithCaps(this MesherOptionsBuilder b, bool bottom, bool top)
            => b.SetGenerateBottomCap(bottom).SetGenerateTopCap(top);

        /// <summary>Configures hole refinement target edge length and band.</summary>
        /// <param name="b">Builder.</param>
        /// <param name="targetEdgeLengthNearHoles">Refined edge length near holes.</param>
        /// <param name="band">Refinement influence band.</param>
        public static MesherOptionsBuilder WithHoleRefinement(this MesherOptionsBuilder b, double targetEdgeLengthNearHoles, double band)
            => b.SetTargetEdgeLengthXYNearHoles(targetEdgeLengthNearHoles).SetHoleRefineBand(band);

        /// <summary>Configures segment refinement target edge length and band.</summary>
        /// <param name="b">Builder.</param>
        /// <param name="targetEdgeLengthNearSegments">Refined edge length near segments.</param>
        /// <param name="band">Refinement influence band.</param>
        public static MesherOptionsBuilder WithSegmentRefinement(this MesherOptionsBuilder b, double targetEdgeLengthNearSegments, double band)
            => b.SetTargetEdgeLengthXYNearSegments(targetEdgeLengthNearSegments).SetSegmentRefineBand(band);

        /// <summary>Sets minimum cap quad quality threshold.</summary>
        /// <param name="b">Builder.</param>
        /// <param name="value">Quality threshold [0,1].</param>
        public static MesherOptionsBuilder WithMinCapQuadQuality(this MesherOptionsBuilder b, double value)
            => b.SetMinCapQuadQuality(value);

        /// <summary>Enables output of rejected cap triangles.</summary>
        /// <param name="b">Builder.</param>
        /// <param name="value">True to output triangles.</param>
        public static MesherOptionsBuilder WithRejectedCapTriangles(this MesherOptionsBuilder b, bool value)
            => b.SetOutputRejectedCapTriangles(value);

        /// <summary>Fast (default) preset: coarser edge lengths, lower quality threshold.</summary>
        /// <param name="b">Builder.</param>
        public static MesherOptionsBuilder WithFastPreset(this MesherOptionsBuilder b)
            => b.WithTargetEdgeLengthXY(2.0)
                 .WithTargetEdgeLengthZ(2.0)
                 .WithMinCapQuadQuality(0.3)
                 .WithRejectedCapTriangles(false);

        /// <summary>High quality preset: finer edge lengths, higher quality threshold, keep rejected triangles.</summary>
        /// <param name="b">Builder.</param>
        public static MesherOptionsBuilder WithHighQualityPreset(this MesherOptionsBuilder b)
            => b.WithTargetEdgeLengthXY(0.5)
                 .WithTargetEdgeLengthZ(0.5)
                 .WithMinCapQuadQuality(0.7)
                 .WithRejectedCapTriangles(true);
    }
}
