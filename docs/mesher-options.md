# Mesher Options Deep Dive

This document expands on all properties in `MesherOptions`.

| Option | Type | Default | Description | Notes |
|--------|------|---------|-------------|-------|
| TargetEdgeLengthXY | double | 2.0 | Target subdivision length in XY for side faces & caps | Minimum  > 0 |
| TargetEdgeLengthZ  | double | 2.0 | Target vertical subdivision length | Combined with constraint / geometry levels |
| GenerateBottomCap  | bool   | true | Emit bottom cap quads | Independent of top |
| GenerateTopCap     | bool   | true | Emit top cap quads |  |
| TargetEdgeLengthXYNearHoles | double? | null | Finer XY length near holes | Rectangle fast?path only |
| HoleRefineBand | double | 0 | Distance band around hole perimeters for refinement | 0 disables |
| TargetEdgeLengthXYNearSegments | double? | null | Finer XY length near geometry segments | Rectangle fast?path only |
| SegmentRefineBand | double | 0 | Distance band around segments for refinement | 0 disables |
| MinCapQuadQuality | double | 0.75 | Discard triangle pairings below score | Affects generic (non?rectangle) cap path |
| Epsilon | double | 1e-9 | Tolerance for vertex dedup / Z comparisons | Avoid too small if coordinates large |

## Quality Score
Weighted: aspect (60%) + orthogonality (35%) + non?degenerate area (5%). Range [0,1]. Stored in `Quad.QualityScore` for cap quads.

## Constraint Segments
`PrismStructureDefinition.AddConstraintSegment(Segment2D, z)` inserts a forced Z?level if `z` ? (z0, z1). Helps align geometry with horizontal bands.

## Refinement Strategy
1. Start with coarse grid (rectangle fast?path).  
2. Re?emit finer grid cells where `IsNearAnyHole` or `IsNearAnySegment` within their bands.  
3. Non?rectangle footprints use tessellation + quad pairing; XY refinement near features currently rectangle?only.

## Guidelines
- Choose `TargetEdgeLengthXY` first (global density), then add selective refinement via bands.  
- Keep `MinCapQuadQuality` moderate (0.6–0.8) to avoid excessive degenerate quads.  
- Increase `Epsilon` if coordinates are very large (e.g. world coordinates in meters) to avoid near?duplicate splitting.  
- For very tall prisms, use a larger `TargetEdgeLengthZ` plus explicit constraint levels where needed.

## Examples
### Minimal (coarse)
```csharp
var opt = new MesherOptions { TargetEdgeLengthXY = 5, TargetEdgeLengthZ = 5, GenerateTopCap = false };
```

### Fine caps near a hole
```csharp
var opt = new MesherOptions {
  TargetEdgeLengthXY = 1.5,
  TargetEdgeLengthZ = 1.0,
  TargetEdgeLengthXYNearHoles = 0.5,
  HoleRefineBand = 1.0
};
```

### Strict high quality cap quads
```csharp
var opt = new MesherOptions {
  TargetEdgeLengthXY = 0.8,
  TargetEdgeLengthZ = 0.8,
  MinCapQuadQuality = 0.85
};
```

## Future (Roadmap)
- Per?hole / per?segment refinement overrides.  
- Anisotropic XY targets (different X vs Y).  
- Quality re?pairing / smoothing passes.  
