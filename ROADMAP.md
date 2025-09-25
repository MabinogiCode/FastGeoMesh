# Roadmap

Status summary
- v0 delivered: core prism mesher, caps (rectangle fast-path + generic), holes, vertical constraints, quad quality, geometry integration, indexed/exporters, docs/tests.
- v1 and later: more refinement/smoothing, per-cap options, alignment, performance.

v0 (delivered)
- [x] Prism meshing from CCW polygon footprint + [z0,z1]
- [x] Side faces with XY subdivision (TargetEdgeLengthXY)
- [x] Z subdivision honoring TargetEdgeLengthZ, constraint segments at Z, geometry Z
- [x] Caps generation
  - [x] Rectangle fast-path with refinement: near holes and near segments
  - [x] Generic: LibTessDotNet triangulation + quad pairing with quality
- [x] Quad quality
  - [x] Expose Quad.QualityScore on caps quads
  - [x] MinCapQuadQuality default 0.75 to reject poor pairings
- [x] Geometry integration: points and 3D segments collected to output
- [x] Indexed mesh export + adjacency + IO for custom txt
- [x] Exporters: OBJ, glTF (.gltf JSON, embedded buffer)
- [x] Docs: README, docs/README, MIT License
- [x] Tests: shapes, holes refinement, adjacency, quadification quality, constraints integration

v1 (next)
- [ ] Refinement heuristics
  - [ ] Anisotropic refinement around internal segments (aligned cells)
  - [ ] Per-hole/segment band and target values
- [ ] Caps options
  - [ ] Generate only top or only bottom cap
  - [ ] Preserve exact rectangular grid alignment when mixing fine/coarse bands
- [ ] Alignment and snapping
  - [ ] Per-edge alignment options (snap to constraints in XY)
  - [ ] Grid snapping for rectangle fast-path when requested
- [ ] Quality improvements
  - [ ] Optional smoothing pass on caps (Laplacian with boundary constraints)
  - [ ] Additional quad quality metrics (skew, angle deviation) exposed
- [ ] Samples
  - [ ] Export example (glTF viewer link)
  - [ ] Complex footprint with multiple holes

Later
- [ ] Variable TargetEdgeLengthZ per band/schedule
- [ ] Tags/metadata propagation on cells
- [ ] Performance: pooling LibTessDotNet objects, span-based indexing, zero-alloc paths

Notes
- Keep polygons CCW and simple (no self-intersections)
- Holes must be inside the footprint and non-overlapping
