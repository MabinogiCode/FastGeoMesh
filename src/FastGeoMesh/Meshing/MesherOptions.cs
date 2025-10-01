#pragma warning disable CS1570

namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Configuration options for mesh generation, including target edge lengths, cap generation,
    /// refinement parameters, and quality thresholds. Use <see cref="MesherOptionsBuilder"/> for fluent configuration.
    /// </summary>
    public sealed class MesherOptions
    {
        private const double MAX_TARGET_EDGE_LENGTH = 1e6;
        private const double MIN_TARGET_EDGE_LENGTH = 1e-6;
        private const double MAX_REFINEMENT_BAND = 1e4;

        private bool _validated;

        /// <summary>
        /// Gets or sets the target edge length for mesh discretization in the XY plane.
        /// Must be positive. Default is 2.0.
        /// </summary>
        public double TargetEdgeLengthXY { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the target edge length for mesh discretization in the Z direction.
        /// Must be positive. Default is 2.0.
        /// </summary>
        public double TargetEdgeLengthZ { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets whether to generate the bottom cap of the prism.
        /// Default is true.
        /// </summary>
        public bool GenerateBottomCap { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to generate the top cap of the prism.
        /// Default is true.
        /// </summary>
        public bool GenerateTopCap { get; set; } = true;

        /// <summary>
        /// Gets or sets the epsilon tolerance for geometric calculations and vertex merging.
        /// Must be positive and finite. Default is 1e-9.
        /// </summary>
        public double Epsilon { get; set; } = 1e-9;

        /// <summary>
        /// Gets or sets the target edge length for mesh refinement near hole boundaries.
        /// If null, no refinement is applied near holes. Must be positive and not exceed TargetEdgeLengthXY.
        /// </summary>
        public double? TargetEdgeLengthXYNearHoles { get; set; }

        /// <summary>
        /// Gets or sets the distance from hole boundaries where refinement is applied.
        /// Only used when TargetEdgeLengthXYNearHoles is set. Must be non-negative.
        /// </summary>
        public double HoleRefineBand { get; set; }

        /// <summary>
        /// Gets or sets the target edge length for mesh refinement near segment boundaries.
        /// If null, no refinement is applied near segments. Must be positive and not exceed TargetEdgeLengthXY.
        /// </summary>
        public double? TargetEdgeLengthXYNearSegments { get; set; }

        /// <summary>
        /// Gets or sets the distance from segment boundaries where refinement is applied.
        /// Only used when TargetEdgeLengthXYNearSegments is set. Must be non-negative.
        /// </summary>
        public double SegmentRefineBand { get; set; }

        /// <summary>
        /// Gets or sets the minimum quality threshold for quad elements in caps.
        /// Quads with quality below this threshold will be rejected. Must be between 0.0 and 1.0. Default is 0.3.
        /// </summary>
        public double MinCapQuadQuality { get; set; } = 0.3;

        /// <summary>
        /// Gets or sets whether to output triangles for rejected low-quality quads in caps.
        /// When true, triangles are generated to replace rejected quads. Default is false.
        /// </summary>
        public bool OutputRejectedCapTriangles { get; set; }

        /// <summary>
        /// Creates a new <see cref="MesherOptionsBuilder"/> for fluent configuration of meshing options.
        /// </summary>
        /// <returns>A new builder instance ready for configuration.</returns>
        public static MesherOptionsBuilder CreateBuilder() => new MesherOptionsBuilder();

        /// <summary>
        /// Validates all configured parameters and ensures they form a consistent configuration.
        /// This method is automatically called by <see cref="MesherOptionsBuilder.Build()"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any parameter is outside valid range.</exception>
        /// <exception cref="ArgumentException">Thrown if parameter combinations are invalid.</exception>
        public void Validate()
        {
            if (_validated)
            {
                return;
            }

            // 🚀 OPTIMIZATION: Early validation of most common cases
            if (TargetEdgeLengthXY <= 0 || TargetEdgeLengthZ <= 0)
            {
                throw new ArgumentOutOfRangeException("Target edge lengths must be positive");
            }

            if (MinCapQuadQuality < 0 || MinCapQuadQuality > 1)
            {
                throw new ArgumentOutOfRangeException("MinCapQuadQuality", MinCapQuadQuality, "Quality must be between 0 and 1");
            }

            // Detailed validation only if basic checks pass
            ValidateTargetEdgeLength(TargetEdgeLengthXY, nameof(TargetEdgeLengthXY), "XY");
            ValidateTargetEdgeLength(TargetEdgeLengthZ, nameof(TargetEdgeLengthZ), "Z");

            if (Epsilon <= 0 || double.IsNaN(Epsilon) || double.IsInfinity(Epsilon))
            {
                throw new ArgumentOutOfRangeException("Epsilon", Epsilon, "Epsilon must be a positive, finite number");
            }

            if (TargetEdgeLengthXYNearHoles is { } h)
            {
                ValidateTargetEdgeLength(h, nameof(TargetEdgeLengthXYNearHoles), "HoleRefinement");

                if (h > TargetEdgeLengthXY)
                {
                    throw new ArgumentException("Refined length near holes must be <= base target", "TargetEdgeLengthXYNearHoles");
                }
            }

            if (HoleRefineBand != 0.0) // Only validate if used
            {
                ValidateRefinementBand(HoleRefineBand, nameof(HoleRefineBand), "HoleRefinement");
            }

            if (TargetEdgeLengthXYNearSegments is { } s)
            {
                ValidateTargetEdgeLength(s, nameof(TargetEdgeLengthXYNearSegments), "SegmentRefinement");

                if (s > TargetEdgeLengthXY)
                {
                    throw new ArgumentException("Refined length near segments must be <= base target", "TargetEdgeLengthXYNearSegments");
                }
            }

            if (SegmentRefineBand != 0.0) // Only validate if used
            {
                ValidateRefinementBand(SegmentRefineBand, nameof(SegmentRefineBand), "SegmentRefinement");
            }

            if (double.IsNaN(MinCapQuadQuality))
            {
                throw new ArgumentOutOfRangeException("MinCapQuadQuality", MinCapQuadQuality, "Quality cannot be NaN");
            }

            _validated = true;
        }

        /// <summary>
        /// Resets the validation state, forcing re-validation on next <see cref="Validate()"/> call.
        /// This is automatically called by builder methods when parameters change.
        /// </summary>
        public void ResetValidation() => _validated = false;

        private static void ValidateTargetEdgeLength(double value, string paramName, string category)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Target edge length must be positive");
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Target edge length must be a finite number");
            }

            if (value < MIN_TARGET_EDGE_LENGTH)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Target edge length must be >= {MIN_TARGET_EDGE_LENGTH}");
            }

            if (value > MAX_TARGET_EDGE_LENGTH)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Target edge length must be <= {MAX_TARGET_EDGE_LENGTH}");
            }
        }

        private static void ValidateRefinementBand(double value, string paramName, string category)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Refinement band must be non-negative");
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Refinement band must be finite");
            }

            if (value > MAX_REFINEMENT_BAND)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Refinement band must be <= {MAX_REFINEMENT_BAND}");
            }
        }
    }
}
