namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Builder pattern for creating <see cref="MesherOptions"/> with fluent configuration.
    /// Provides methods to configure all meshing parameters with validation and presets.
    /// </summary>
    public sealed class MesherOptionsBuilder
    {
        private readonly MesherOptions _options = new MesherOptions();

        /// <summary>
        /// Sets the target edge length for XY plane discretization.
        /// </summary>
        /// <param name="length">Target edge length in XY direction. Must be positive.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if length is not positive.</exception>
        public MesherOptionsBuilder WithTargetEdgeLengthXY(double length)
        {
            _options.TargetEdgeLengthXY = length;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Sets the target edge length for Z direction discretization.
        /// </summary>
        /// <param name="length">Target edge length in Z direction. Must be positive.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if length is not positive.</exception>
        public MesherOptionsBuilder WithTargetEdgeLengthZ(double length)
        {
            _options.TargetEdgeLengthZ = length;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Configures cap generation for the prism (top and/or bottom faces).
        /// </summary>
        /// <param name="bottom">Whether to generate bottom cap. Default is true.</param>
        /// <param name="top">Whether to generate top cap. Default is true.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder WithCaps(bool bottom = true, bool top = true)
        {
            _options.GenerateBottomCap = bottom;
            _options.GenerateTopCap = top;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Sets the epsilon tolerance for geometric calculations and vertex merging.
        /// </summary>
        /// <param name="epsilon">Tolerance value. Must be positive and finite. Default is 1e-9.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if epsilon is not positive or finite.</exception>
        public MesherOptionsBuilder WithEpsilon(double epsilon)
        {
            _options.Epsilon = epsilon;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Enables mesh refinement near hole boundaries with custom edge length and influence band.
        /// </summary>
        /// <param name="targetLength">Target edge length near holes. Must be positive and less than or equal to base target length.</param>
        /// <param name="band">Distance from hole boundary where refinement applies. Must be non-negative.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if parameters are invalid.</exception>
        /// <exception cref="ArgumentException">Thrown if targetLength exceeds base TargetEdgeLengthXY.</exception>
        public MesherOptionsBuilder WithHoleRefinement(double targetLength, double band)
        {
            _options.TargetEdgeLengthXYNearHoles = targetLength;
            _options.HoleRefineBand = band;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Enables mesh refinement near segment boundaries with custom edge length and influence band.
        /// </summary>
        /// <param name="targetLength">Target edge length near segments. Must be positive and less than or equal to base target length.</param>
        /// <param name="band">Distance from segment where refinement applies. Must be non-negative.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if parameters are invalid.</exception>
        /// <exception cref="ArgumentException">Thrown if targetLength exceeds base TargetEdgeLengthXY.</exception>
        public MesherOptionsBuilder WithSegmentRefinement(double targetLength, double band)
        {
            _options.TargetEdgeLengthXYNearSegments = targetLength;
            _options.SegmentRefineBand = band;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Sets the minimum quality threshold for quad elements in caps.
        /// Quads below this quality will be rejected and optionally replaced with triangles.
        /// </summary>
        /// <param name="quality">Quality threshold between 0.0 and 1.0. Higher values mean better quality.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if quality is not between 0 and 1.</exception>
        public MesherOptionsBuilder WithMinCapQuadQuality(double quality)
        {
            _options.MinCapQuadQuality = quality;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Configures whether to output triangles for rejected low-quality quads in caps.
        /// </summary>
        /// <param name="output">Whether to output rejected cap triangles. Default is true.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder WithRejectedCapTriangles(bool output = true)
        {
            _options.OutputRejectedCapTriangles = output;
            return this;
        }

        /// <summary>
        /// Applies high-quality preset configuration optimized for CAD precision.
        /// Sets aggressive refinement parameters for maximum mesh quality.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder WithHighQualityPreset()
        {
            _options.TargetEdgeLengthXY = 0.5;
            _options.TargetEdgeLengthZ = 0.5;
            _options.MinCapQuadQuality = 0.7;
            _options.OutputRejectedCapTriangles = true;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Applies fast preset configuration optimized for real-time applications.
        /// Sets relaxed parameters for maximum performance with acceptable quality.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder WithFastPreset()
        {
            _options.TargetEdgeLengthXY = 2.0;
            _options.TargetEdgeLengthZ = 2.0;
            _options.MinCapQuadQuality = 0.3;
            _options.OutputRejectedCapTriangles = false;
            _options.ResetValidation();
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="MesherOptions"/>.
        /// </summary>
        /// <returns>A validated <see cref="MesherOptions"/> instance with the configured parameters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any configured parameter is invalid.</exception>
        /// <exception cref="ArgumentException">Thrown if parameter combinations are invalid.</exception>
        public MesherOptions Build()
        {
            _options.Validate();
            return _options;
        }
    }
}
