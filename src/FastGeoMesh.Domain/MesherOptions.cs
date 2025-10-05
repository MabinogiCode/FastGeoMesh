using FastGeoMesh.Domain;

namespace FastGeoMesh.Meshing
{
    public sealed class MesherOptions
    {
        private const double MAX_REFINEMENT_BAND = 1e4;
        private bool _validated;
        public EdgeLength TargetEdgeLengthXY { get; set; } = EdgeLength.From(2.0);
        public EdgeLength TargetEdgeLengthZ { get; set; } = EdgeLength.From(2.0);
        public bool GenerateBottomCap { get; set; } = true;
        public bool GenerateTopCap { get; set; } = true;
        public Tolerance Epsilon { get; set; } = Tolerance.From(1e-9);
        public EdgeLength? TargetEdgeLengthXYNearHoles { get; set; }
        public double HoleRefineBand { get; set; }
        public EdgeLength? TargetEdgeLengthXYNearSegments { get; set; }
        public double SegmentRefineBand { get; set; }
        public double MinCapQuadQuality { get; set; } = 0.3;
        public bool OutputRejectedCapTriangles { get; set; }
        public static MesherOptionsBuilder CreateBuilder() => new MesherOptionsBuilder();
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
