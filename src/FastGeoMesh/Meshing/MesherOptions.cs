namespace FastGeoMesh.Meshing;

public sealed class MesherOptions
{
    /// <summary>Approximate target edge length for subdivision in XY.</summary>
    public double TargetEdgeLengthXY { get; set; } = 2.0;

    /// <summary>Approximate target edge length for subdivision along Z.</summary>
    public double TargetEdgeLengthZ { get; set; } = 2.0;

    /// <summary>Generate top and bottom caps if possible.</summary>
    public bool GenerateTopAndBottomCaps { get; set; } = true;

    /// <summary>Numerical tolerance.</summary>
    public double Epsilon { get; set; } = double.Epsilon;

    /// <summary>
    /// Optional finer XY target length to apply around holes edges and caps near holes. If null, uses TargetEdgeLengthXY.
    /// </summary>
    public double? TargetEdgeLengthXYNearHoles { get; set; }

    /// <summary>
    /// Band distance (in meters) around holes to apply near-holes refinement on caps. 0 disables.
    /// </summary>
    public double HoleRefineBand { get; set; }

    /// <summary>
    /// Optional finer XY target length to apply around geometry segments (projected to XY). If null, uses TargetEdgeLengthXY.
    /// </summary>
    public double? TargetEdgeLengthXYNearSegments { get; set; }

    /// <summary>
    /// Band distance (in meters) around geometry segments to apply near-segments refinement on caps. 0 disables.
    /// </summary>
    public double SegmentRefineBand { get; set; }

    /// <summary>
    /// Minimum acceptable quality for pairing triangles into caps quads in the generic tessellation path.
    /// In [0,1]. Candidates with lower score are rejected and left as degenerate quads.
    /// </summary>
    public double MinCapQuadQuality { get; set; } = 0.75;
}
