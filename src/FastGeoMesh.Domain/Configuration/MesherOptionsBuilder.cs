namespace FastGeoMesh.Domain {
    /// <summary>
    /// Builder pattern implementation for constructing MesherOptions with fluent API.
    /// </summary>
    public class MesherOptionsBuilder {
        private readonly MesherOptions _options = new MesherOptions();
        private readonly List<Error> _validationErrors = new List<Error>();

        /// <summary>
        /// Sets the target edge length for the XY plane.
        /// </summary>
        /// <param name="value">The target edge length value (must be positive).</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetTargetEdgeLengthXY(double value) {
            try {
                _options.TargetEdgeLengthXY = EdgeLength.From(value);
            }
            catch (ArgumentException ex) {
                _validationErrors.Add(new Error("TargetEdgeLengthXY.Invalid", ex.Message));
            }
            return this;
        }

        /// <summary>
        /// Sets the target edge length for the Z axis.
        /// </summary>
        /// <param name="value">The target edge length value (must be positive).</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetTargetEdgeLengthZ(double value) {
            try {
                _options.TargetEdgeLengthZ = EdgeLength.From(value);
            }
            catch (ArgumentException ex) {
                _validationErrors.Add(new Error("TargetEdgeLengthZ.Invalid", ex.Message));
            }
            return this;
        }

        /// <summary>
        /// Sets whether to generate the bottom cap of the prism.
        /// </summary>
        /// <param name="value">True to generate bottom cap, false otherwise.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetGenerateBottomCap(bool value) {
            _options.GenerateBottomCap = value;
            return this;
        }

        /// <summary>
        /// Sets whether to generate the top cap of the prism.
        /// </summary>
        /// <param name="value">True to generate top cap, false otherwise.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetGenerateTopCap(bool value) {
            _options.GenerateTopCap = value;
            return this;
        }

        /// <summary>
        /// Sets the epsilon tolerance for geometric calculations.
        /// </summary>
        /// <param name="value">The tolerance value (must be positive and small).</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetEpsilon(double value) {
            try {
                _options.Epsilon = Tolerance.From(value);
            }
            catch (ArgumentException ex) {
                _validationErrors.Add(new Error("Epsilon.Invalid", ex.Message));
            }
            return this;
        }

        /// <summary>
        /// Sets the target edge length near holes for mesh refinement.
        /// </summary>
        /// <param name="value">The target edge length near holes, or null to disable refinement.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetTargetEdgeLengthXYNearHoles(double? value) {
            try {
                _options.TargetEdgeLengthXYNearHoles = value.HasValue ? EdgeLength.From(value.Value) : null;
            }
            catch (ArgumentException ex) {
                _validationErrors.Add(new Error("TargetEdgeLengthXYNearHoles.Invalid", ex.Message));
            }
            return this;
        }

        /// <summary>
        /// Sets the band width around holes for mesh refinement.
        /// </summary>
        /// <param name="value">The refinement band width (must be non-negative).</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetHoleRefineBand(double value) {
            _options.HoleRefineBand = value;
            return this;
        }

        /// <summary>
        /// Sets the target edge length near segments for mesh refinement.
        /// </summary>
        /// <param name="value">The target edge length near segments, or null to disable refinement.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetTargetEdgeLengthXYNearSegments(double? value) {
            try {
                _options.TargetEdgeLengthXYNearSegments = value.HasValue ? EdgeLength.From(value.Value) : null;
            }
            catch (ArgumentException ex) {
                _validationErrors.Add(new Error("TargetEdgeLengthXYNearSegments.Invalid", ex.Message));
            }
            return this;
        }

        /// <summary>
        /// Sets the band width around segments for mesh refinement.
        /// </summary>
        /// <param name="value">The refinement band width (must be non-negative).</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetSegmentRefineBand(double value) {
            _options.SegmentRefineBand = value;
            return this;
        }

        /// <summary>
        /// Sets the minimum quality threshold for cap quadrilaterals.
        /// </summary>
        /// <param name="value">The quality threshold (0.0 to 1.0).</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetMinCapQuadQuality(double value) {
            _options.MinCapQuadQuality = value;
            return this;
        }

        /// <summary>
        /// Sets whether to output rejected cap triangles instead of low-quality quads.
        /// </summary>
        /// <param name="value">True to output rejected triangles, false otherwise.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MesherOptionsBuilder SetOutputRejectedCapTriangles(bool value) {
            _options.OutputRejectedCapTriangles = value;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured MesherOptions instance.
        /// Returns a Result containing the validated options or validation errors.
        /// </summary>
        /// <returns>A Result containing the configured MesherOptions or validation errors.</returns>
        public Result<MesherOptions> Build() {
            // Check for validation errors collected during building
            if (_validationErrors.Count > 0) {
                var combinedMessage = string.Join("; ", _validationErrors.Select(e => e.Description));
                return Result<MesherOptions>.Failure(new Error("Builder.ValidationErrors", combinedMessage));
            }

            try {
                // Reset validation state to force fresh validation
                _options.ResetValidation();

                // Validate the options
                var validationResult = _options.Validate();
                if (validationResult.IsFailure) {
                    return Result<MesherOptions>.Failure(validationResult.Error);
                }

                // Return successful result with validated options
                return Result<MesherOptions>.Success(_options);
            }
            catch (ArgumentOutOfRangeException ex) {
                return Result<MesherOptions>.Failure(new Error("Builder.RangeError", ex.Message));
            }
            catch (ArgumentException ex) {
                return Result<MesherOptions>.Failure(new Error("Builder.ValidationError", ex.Message));
            }
        }
    }
}
