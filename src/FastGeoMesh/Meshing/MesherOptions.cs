using System;

namespace FastGeoMesh.Meshing
{
    /// <summary>Options controlling prism meshing resolution, cap generation, refinement and quality thresholds.</summary>
    public sealed class MesherOptions
    {
        /// <summary>Target horizontal edge length (XY plane) for regular regions.</summary>
        public double TargetEdgeLengthXY { get; set; } = 2.0;
        /// <summary>Target vertical edge length (Z direction) for side face subdivision.</summary>
        public double TargetEdgeLengthZ  { get; set; } = 2.0;
        /// <summary>Generate bottom cap faces.</summary>
        public bool   GenerateBottomCap  { get; set; } = true;
        /// <summary>Generate top cap faces.</summary>
        public bool   GenerateTopCap     { get; set; } = true;
        /// <summary>Vertex merge epsilon for indexing / deduplication.</summary>
        public double  Epsilon                       { get; set; } = 1e-9;
        /// <summary>Optional finer target edge near holes.</summary>
        public double? TargetEdgeLengthXYNearHoles   { get; set; }
        /// <summary>Refinement influence band distance around holes.</summary>
        public double  HoleRefineBand                { get; set; }
        /// <summary>Optional finer target edge near internal segments.</summary>
        public double? TargetEdgeLengthXYNearSegments{ get; set; }
        /// <summary>Refinement influence band distance around internal segments.</summary>
        public double  SegmentRefineBand             { get; set; }
        /// <summary>Minimum acceptable cap quad quality (0..1) to keep pair; lower quality may become triangles.</summary>
        public double  MinCapQuadQuality             { get; set; } = 0.75;
        /// <summary>If true, rejected low-quality cap quad pairs are emitted as triangles.</summary>
        public bool    OutputRejectedCapTriangles    { get; set; }
        /// <summary>Validate option values and relationships.</summary>
        public void Validate()
        {
            if (TargetEdgeLengthXY <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXY));
            }
            if (TargetEdgeLengthZ  <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthZ));
            }
            if (Epsilon <= 0 || double.IsNaN(Epsilon))
            {
                throw new ArgumentOutOfRangeException(nameof(Epsilon));
            }
            if (TargetEdgeLengthXYNearHoles is { } h && h <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXYNearHoles));
            }
            if (HoleRefineBand < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(HoleRefineBand));
            }
            if (TargetEdgeLengthXYNearSegments is { } s && s <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXYNearSegments));
            }
            if (SegmentRefineBand < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(SegmentRefineBand));
            }
            if (MinCapQuadQuality < 0 || MinCapQuadQuality > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(MinCapQuadQuality));
            }
            if (TargetEdgeLengthXYNearHoles.HasValue && TargetEdgeLengthXYNearHoles > TargetEdgeLengthXY)
            {
                throw new ArgumentException("Refined length near holes must be <= base target");
            }
            if (TargetEdgeLengthXYNearSegments.HasValue && TargetEdgeLengthXYNearSegments > TargetEdgeLengthXY)
            {
                throw new ArgumentException("Refined length near segments must be <= base target");
            }
        }
    }
}
