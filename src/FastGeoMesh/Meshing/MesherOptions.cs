namespace FastGeoMesh.Meshing;

/// <summary>Mesher configuration options.</summary>
public sealed class MesherOptions
{
    /// <summary>Approximate target edge length for subdivision in XY (>0).</summary>
    public double TargetEdgeLengthXY { get; set; } = 2.0;
    /// <summary>Approximate target edge length for subdivision along Z (>0).</summary>
    public double TargetEdgeLengthZ { get; set; } = 2.0;
    /// <summary>Generate bottom cap?</summary>
    public bool GenerateBottomCap { get; set; } = true;
    /// <summary>Generate top cap?</summary>
    public bool GenerateTopCap { get; set; } = true;
    /// <summary>Legacy combined flag. Setting sets both top and bottom. Getting returns logical AND.</summary>
    [Obsolete("Use GenerateBottomCap / GenerateTopCap")] 
    public bool GenerateTopAndBottomCaps { get => GenerateBottomCap && GenerateTopCap; set { GenerateBottomCap = value; GenerateTopCap = value; } }

    /// <summary>Numerical tolerance used for coordinate deduplication and level comparisons.</summary>
    public double Epsilon { get; set; } = 1e-9;

    /// <summary>Optional finer XY target length near holes (null = base).</summary>
    public double? TargetEdgeLengthXYNearHoles { get; set; }

    /// <summary>Refinement influence distance around holes.</summary>
    public double HoleRefineBand { get; set; }

    /// <summary>Optional finer XY target length near segments (null = base).</summary>
    public double? TargetEdgeLengthXYNearSegments { get; set; }

    /// <summary>Refinement influence distance around segments.</summary>
    public double SegmentRefineBand { get; set; }

    /// <summary>Minimum acceptable cap quad pairing quality [0,1].</summary>
    public double MinCapQuadQuality { get; set; } = 0.75;

    /// <summary>Emit leftover (unpaired) cap triangles as true triangles instead of degenerate quads?</summary>
    public bool OutputRejectedCapTriangles { get; set; }

    /// <summary>Validate option ranges.</summary>
    public void Validate()
    {
        if (TargetEdgeLengthXY <= 0) throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXY));
        if (TargetEdgeLengthZ <= 0) throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthZ));
        if (Epsilon <= 0 || double.IsNaN(Epsilon)) throw new ArgumentOutOfRangeException(nameof(Epsilon));
        if (TargetEdgeLengthXYNearHoles is { } h && h <= 0) throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXYNearHoles));
        if (HoleRefineBand < 0) throw new ArgumentOutOfRangeException(nameof(HoleRefineBand));
        if (TargetEdgeLengthXYNearSegments is { } s && s <= 0) throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXYNearSegments));
        if (SegmentRefineBand < 0) throw new ArgumentOutOfRangeException(nameof(SegmentRefineBand));
        if (MinCapQuadQuality < 0 || MinCapQuadQuality > 1) throw new ArgumentOutOfRangeException(nameof(MinCapQuadQuality));
        if (TargetEdgeLengthXYNearHoles.HasValue && TargetEdgeLengthXYNearHoles > TargetEdgeLengthXY)
            throw new ArgumentException("Refined length near holes must be <= base target");
        if (TargetEdgeLengthXYNearSegments.HasValue && TargetEdgeLengthXYNearSegments > TargetEdgeLengthXY)
            throw new ArgumentException("Refined length near segments must be <= base target");
    }
}
