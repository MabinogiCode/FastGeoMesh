namespace FastGeoMesh.Meshing;

using System;

public sealed class MesherOptions
{
    /// <summary>Approximate target edge length for subdivision in XY ( > 0 ).</summary>
    public double TargetEdgeLengthXY { get; set; } = 2.0;

    /// <summary>Approximate target edge length for subdivision along Z ( > 0 ).</summary>
    public double TargetEdgeLengthZ { get; set; } = 2.0;

    /// <summary>Generate bottom cap.</summary>
    public bool GenerateBottomCap { get; set; } = true;
    /// <summary>Generate top cap.</summary>
    public bool GenerateTopCap { get; set; } = true;

    /// <summary>Legacy combined flag. Setting sets both top and bottom. Getting returns logical AND.</summary>
    [Obsolete("Use GenerateBottomCap / GenerateTopCap")] 
    public bool GenerateTopAndBottomCaps { get => GenerateBottomCap && GenerateTopCap; set { GenerateBottomCap = value; GenerateTopCap = value; } }

    /// <summary>Numerical tolerance used for coordinate deduplication and Z level comparisons.</summary>
    public double Epsilon { get; set; } = 1e-9; // more practical default

    /// <summary>Optional finer XY target length to apply around holes edges and caps near holes. If null, uses TargetEdgeLengthXY.</summary>
    public double? TargetEdgeLengthXYNearHoles { get; set; }

    /// <summary>Band distance (in meters) around holes to apply near-holes refinement on caps. 0 disables.</summary>
    public double HoleRefineBand { get; set; }

    /// <summary>Optional finer XY target length to apply around geometry segments (projected to XY). If null, uses TargetEdgeLengthXY.</summary>
    public double? TargetEdgeLengthXYNearSegments { get; set; }

    /// <summary>Band distance (in meters) around geometry segments to apply near-segments refinement on caps. 0 disables.</summary>
    public double SegmentRefineBand { get; set; }

    /// <summary>Minimum acceptable quality for pairing triangles into caps quads in the generic tessellation path. In [0,1].</summary>
    public double MinCapQuadQuality { get; set; } = 0.75;

    /// <summary>Validate current option values; throws ArgumentOutOfRangeException / ArgumentException on invalid configuration.</summary>
    public void Validate()
    {
        if (TargetEdgeLengthXY <= 0) throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXY));
        if (TargetEdgeLengthZ <= 0) throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthZ));
        if (Epsilon <= 0 || double.IsNaN(Epsilon)) throw new ArgumentOutOfRangeException(nameof(Epsilon));
        if (TargetEdgeLengthXYNearHoles.HasValue && TargetEdgeLengthXYNearHoles.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXYNearHoles));
        if (HoleRefineBand < 0) throw new ArgumentOutOfRangeException(nameof(HoleRefineBand));
        if (TargetEdgeLengthXYNearSegments.HasValue && TargetEdgeLengthXYNearSegments.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(TargetEdgeLengthXYNearSegments));
        if (SegmentRefineBand < 0) throw new ArgumentOutOfRangeException(nameof(SegmentRefineBand));
        if (MinCapQuadQuality < 0 || MinCapQuadQuality > 1) throw new ArgumentOutOfRangeException(nameof(MinCapQuadQuality));
        if (TargetEdgeLengthXYNearHoles.HasValue && TargetEdgeLengthXYNearHoles.Value > TargetEdgeLengthXY)
            throw new ArgumentException("Refined length near holes should be <= base TargetEdgeLengthXY");
        if (TargetEdgeLengthXYNearSegments.HasValue && TargetEdgeLengthXYNearSegments.Value > TargetEdgeLengthXY)
            throw new ArgumentException("Refined length near segments should be <= base TargetEdgeLengthXY");
    }
}
