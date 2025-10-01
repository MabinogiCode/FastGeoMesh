#pragma warning disable CS1570

namespace FastGeoMesh.Meshing
{
    public sealed class MesherOptions
    {
        private const double MAX_TARGET_EDGE_LENGTH = 1e6;
        private const double MIN_TARGET_EDGE_LENGTH = 1e-6;
        private const double MAX_REFINEMENT_BAND = 1e4;

        private bool _validated;

        public double TargetEdgeLengthXY { get; set; } = 2.0;
        public double TargetEdgeLengthZ { get; set; } = 2.0;
        public bool GenerateBottomCap { get; set; } = true;
        public bool GenerateTopCap { get; set; } = true;
        public double Epsilon { get; set; } = 1e-9;
        public double? TargetEdgeLengthXYNearHoles { get; set; }
        public double HoleRefineBand { get; set; }
        public double? TargetEdgeLengthXYNearSegments { get; set; }
        public double SegmentRefineBand { get; set; }
        public double MinCapQuadQuality { get; set; } = 0.3;
        public bool OutputRejectedCapTriangles { get; set; }

        public static MesherOptionsBuilder CreateBuilder() => new MesherOptionsBuilder();

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
