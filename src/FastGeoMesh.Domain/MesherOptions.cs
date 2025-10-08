namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Configuration options for mesh generation operations.
    /// Provides fine-grained control over meshing behavior, quality, and performance.
    /// </summary>
    public sealed class MesherOptions
    {
        private const double MAX_REFINEMENT_BAND = 1e4;
        private bool _validated;

        /// <summary>
        /// Gets or sets the target edge length for the XY plane.
        /// </summary>
        public EdgeLength TargetEdgeLengthXY { get; set; } = EdgeLength.From(2.0);

        /// <summary>
        /// Gets or sets the target edge length for the Z axis.
        /// </summary>
        public EdgeLength TargetEdgeLengthZ { get; set; } = EdgeLength.From(2.0);

        /// <summary>
        /// Gets or sets a value indicating whether to generate the bottom cap of the prism.
        /// </summary>
        public bool GenerateBottomCap { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate the top cap of the prism.
        /// </summary>
        public bool GenerateTopCap { get; set; } = true;

        /// <summary>
        /// Gets or sets the epsilon tolerance for geometric calculations.
        /// </summary>
        public Tolerance Epsilon { get; set; } = Tolerance.From(1e-9);

        /// <summary>
        /// Gets or sets the target edge length near holes for mesh refinement.
        /// When set, creates finer mesh near hole boundaries.
        /// </summary>
        public EdgeLength? TargetEdgeLengthXYNearHoles { get; set; }

        /// <summary>
        /// Gets or sets the band width around holes for mesh refinement.
        /// </summary>
        public double HoleRefineBand { get; set; }

        /// <summary>
        /// Gets or sets the target edge length near segments for mesh refinement.
        /// When set, creates finer mesh near segment boundaries.
        /// </summary>
        public EdgeLength? TargetEdgeLengthXYNearSegments { get; set; }

        /// <summary>
        /// Gets or sets the band width around segments for mesh refinement.
        /// </summary>
        public double SegmentRefineBand { get; set; }

        /// <summary>
        /// Gets or sets the minimum quality threshold for cap quadrilaterals.
        /// Quads below this threshold may be converted to triangles.
        /// </summary>
        public double MinCapQuadQuality { get; set; } = 0.3;

        /// <summary>
        /// Gets or sets a value indicating whether to output rejected cap triangles
        /// instead of low-quality quadrilaterals.
        /// </summary>
        public bool OutputRejectedCapTriangles { get; set; }

        /// <summary>
        /// Creates a new builder for constructing MesherOptions with fluent API.
        /// </summary>
        /// <returns>A new MesherOptionsBuilder instance.</returns>
        public static MesherOptionsBuilder CreateBuilder() => new MesherOptionsBuilder();

        /// <summary>
        /// Validates the current configuration and returns any validation errors.
        /// </summary>
        /// <returns>A Result indicating success or containing validation errors.</returns>
        public Result Validate()
        {
            if (_validated)
            {
                return Result.Success();
            }
            var errors = new List<Error>();
            if (MinCapQuadQuality < 0 || MinCapQuadQuality > 1)
            {
                errors.Add(new Error("Validation.MinCapQuadQuality", "Quality must be between 0 and 1"));
            }
            if (TargetEdgeLengthXYNearHoles is { } h && h.Value > TargetEdgeLengthXY.Value)
            {
                errors.Add(new Error("Validation.TargetEdgeLengthXYNearHoles", "Refined length near holes must be <= base target"));
            }
            if (HoleRefineBand != 0.0)
            {
                var bandErrors = ValidateRefinementBand(HoleRefineBand, nameof(HoleRefineBand));
                errors.AddRange(bandErrors);
            }
            if (TargetEdgeLengthXYNearSegments is { } s && s.Value > TargetEdgeLengthXY.Value)
            {
                errors.Add(new Error("Validation.TargetEdgeLengthXYNearSegments", "Refined length near segments must be <= base target"));
            }
            if (SegmentRefineBand != 0.0)
            {
                var bandErrors = ValidateRefinementBand(SegmentRefineBand, nameof(SegmentRefineBand));
                errors.AddRange(bandErrors);
            }
            if (double.IsNaN(MinCapQuadQuality))
            {
                errors.Add(new Error("Validation.MinCapQuadQuality", "Quality cannot be NaN"));
            }
            if (errors.Count > 0)
            {
                return Result.Failure(new Error("Validation.MultipleErrors", string.Join("; ", errors.ConvertAll(e => e.Description))));
            }
            _validated = true;
            return Result.Success();
        }

        /// <summary>
        /// Resets the validation state, forcing re-validation on the next call to Validate().
        /// </summary>
        public void ResetValidation() => _validated = false;

        private static List<Error> ValidateRefinementBand(double value, string paramName)
        {
            var errors = new List<Error>();
            if (value < 0)
            {
                errors.Add(new Error($"Validation.{paramName}", "Refinement band must be non-negative"));
            }
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                errors.Add(new Error($"Validation.{paramName}", "Refinement band must be finite"));
            }
            if (value > MAX_REFINEMENT_BAND)
            {
                errors.Add(new Error($"Validation.{paramName}", $"Refinement band must be <= {MAX_REFINEMENT_BAND}"));
            }
            return errors;
        }
    }
}
