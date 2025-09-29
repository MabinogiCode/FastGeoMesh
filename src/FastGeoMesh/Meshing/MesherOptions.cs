using System;

namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Options controlling prism meshing resolution, cap generation, refinement and quality thresholds.
    /// Invariants validated by <see cref="Validate"/>:
    ///  - All target edge lengths &gt; 0
    ///  - Refined lengths (holes / segments) when present are &lt;= base target
    ///  - Quality thresholds within [0,1]
    ///  - Security limits to prevent excessive memory allocation
    /// </summary>
    public sealed class MesherOptions
    {
        // Security constants to prevent DoS attacks
        private const double MAX_TARGET_EDGE_LENGTH = 1e6;
        private const double MIN_TARGET_EDGE_LENGTH = 1e-6;
        private const double MAX_REFINEMENT_BAND = 1e4;

        /// <summary>Maximum desired edge length (XY plane) for regular regions.</summary>
        public double TargetEdgeLengthXY { get; set; } = 2.0;
        /// <summary>Maximum desired edge length (Z direction) for side face subdivision.</summary>
        public double TargetEdgeLengthZ  { get; set; } = 2.0;
        /// <summary>Generate bottom cap faces.</summary>
        public bool   GenerateBottomCap  { get; set; } = true;
        /// <summary>Generate top cap faces.</summary>
        public bool   GenerateTopCap     { get; set; } = true;
        /// <summary>Vertex merge epsilon (must be &gt; 0).</summary>
        public double  Epsilon                       { get; set; } = 1e-9;
        /// <summary>Optional finer maximum edge length near holes (must be &lt;= <see cref="TargetEdgeLengthXY"/>).</summary>
        public double? TargetEdgeLengthXYNearHoles   { get; set; }
        /// <summary>Refinement influence band distance around holes (>= 0).</summary>
        public double  HoleRefineBand                { get; set; }
        /// <summary>Optional finer maximum edge length near internal segments (must be &lt;= <see cref="TargetEdgeLengthXY"/>).</summary>
        public double? TargetEdgeLengthXYNearSegments{ get; set; }
        /// <summary>Refinement influence band distance around internal segments (>= 0).</summary>
        public double  SegmentRefineBand             { get; set; }
        /// <summary>Minimum acceptable cap quad quality (0..1) to keep a paired quad.</summary>
        public double  MinCapQuadQuality             { get; set; } = 0.3;
        /// <summary>If true, rejected low-quality quad pairs are emitted as triangles instead of forced quads.</summary>
        public bool    OutputRejectedCapTriangles    { get; set; }

        /// <summary>Validate option values. Throws <see cref="ArgumentOutOfRangeException"/> / <see cref="ArgumentException"/> on invalid configuration.</summary>
        public void Validate()
        {
            ValidateTargetEdgeLength(TargetEdgeLengthXY, nameof(TargetEdgeLengthXY));
            ValidateTargetEdgeLength(TargetEdgeLengthZ, nameof(TargetEdgeLengthZ));
            
            if (Epsilon <= 0 || double.IsNaN(Epsilon) || double.IsInfinity(Epsilon))
            {
                throw new ArgumentOutOfRangeException("Epsilon", Epsilon, "Epsilon must be a positive, finite number");
            }
            
            if (TargetEdgeLengthXYNearHoles is { } h)
            {
                ValidateTargetEdgeLength(h, nameof(TargetEdgeLengthXYNearHoles));
            }
            
            ValidateRefinementBand(HoleRefineBand, nameof(HoleRefineBand));
            
            if (TargetEdgeLengthXYNearSegments is { } s)
            {
                ValidateTargetEdgeLength(s, nameof(TargetEdgeLengthXYNearSegments));
            }
            
            ValidateRefinementBand(SegmentRefineBand, nameof(SegmentRefineBand));
            
            if (MinCapQuadQuality < 0 || MinCapQuadQuality > 1 || double.IsNaN(MinCapQuadQuality))
            {
                throw new ArgumentOutOfRangeException("MinCapQuadQuality", MinCapQuadQuality, "Quality must be between 0 and 1");
            }
            
            if (TargetEdgeLengthXYNearHoles.HasValue && TargetEdgeLengthXYNearHoles > TargetEdgeLengthXY)
            {
                throw new ArgumentException("Refined length near holes must be <= base target", "TargetEdgeLengthXYNearHoles");
            }
            
            if (TargetEdgeLengthXYNearSegments.HasValue && TargetEdgeLengthXYNearSegments > TargetEdgeLengthXY)
            {
                throw new ArgumentException("Refined length near segments must be <= base target", "TargetEdgeLengthXYNearSegments");
            }
        }

        private static void ValidateTargetEdgeLength(double value, string paramName)
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

        private static void ValidateRefinementBand(double value, string paramName)
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
