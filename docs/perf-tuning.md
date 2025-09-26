# Performance & Tuning

This guide offers tips to balance mesh quality, performance, and file size.

## 1. Footprint Shape
- Axis?aligned rectangles trigger a fast structured cap path (fewer artifacts; predictable quads).
- Non?rectangular ? tessellation + quadification (slightly more CPU; quality threshold pruning).

## 2. Edge Length Targets
| Setting | Effect | Guidance |
|---------|--------|----------|
| `TargetEdgeLengthXY` | Global horizontal density | Start here; refine later only where needed |
| `TargetEdgeLengthZ`  | Vertical slices | Use coarser value; add explicit constraint levels for critical planes |

## 3. Local Refinement
- Use `HoleRefineBand` + `TargetEdgeLengthXYNearHoles` sparingly; large bands negate performance gains.
- Same for `SegmentRefineBand`.

## 4. Quad Quality Threshold
- `MinCapQuadQuality` too high ? more degenerate fallback quads (two identical vertices). Keep 0.6–0.8 for balance.

## 5. Epsilon
- Default `1e-9` okay for typical coordinate magnitudes (? 1e6). If coordinates are huge (e.g. UTM meters, > 1e7), consider a larger epsilon to avoid numeric drift duplicates.

## 6. Memory Considerations
- Structured rectangle path: O(n) quads predictable.
- Generic path: LibTessDotNet intermediate triangles (3 * poly vertex count + holes). Reuse mesher instance if extending for pooling.

## 7. Export Size
| Exporter | Size Characteristics | Tip |
|----------|---------------------|-----|
| Text | Largest (ASCII) | Compress or switch to OBJ/glTF for distribution |
| OBJ  | Moderate | Remove comments if size critical |
| glTF (embedded) | Base64 overhead | Future `.glb` could reduce size |
| SVG  | Depends on edge count | Filter internal edges for light preview |

## 8. Profiling Checklist
- Measure time per stage (side faces vs caps) for large footprints.
- Compare quality scores distribution; lowering threshold may reduce pairing loops.

## 9. Scaling Up
| Scenario | Adjustment |
|----------|------------|
| Huge footprint, few Z levels | Increase `TargetEdgeLengthXY` aggressively |
| Many Z slices, thin vertical spacing | Raise `TargetEdgeLengthZ` + explicit constraints |
| Dense hole refinement | Narrow `HoleRefineBand` |

## 10. Roadmap Ideas (Future Optimization)
- Parallel cap generation.
- Buffer pooling & Span<T> adjacency build.
- Binary `.glb` exporter.

