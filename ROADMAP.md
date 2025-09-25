# Roadmap

Scope (v0)
- Prism meshing from CCW polygon footprint + [z0,z1]
- Side faces with XY subdivision (TargetEdgeLengthXY)
- Z subdivision honoring TargetEdgeLengthZ, constraint segments Z, geometry Z
- Caps generation
  - Rectangle fast-path with refinement: near holes and near segments
  - Generic: LibTessDotNet triangulation + quad pairing with quality
- Geometry integration: points and 3D segments collected to output
- Indexed mesh export + adjacency + IO for custom txt
- Tests: holes refinement, adjacency, quadification, L/T excavations

Quality and robustness
- Quad quality score exposure and default MinCapQuadQuality=0.75
- Degenerate tri quads fallback when pairing is poor
- Epsilon-based indexing for vertex deduplication in IndexedMesh

Next (v1)
- Optional export to common formats (OBJ for quads via pairs, GLTF?)
- More refinement heuristics around internal segments (anisotropy)
- Option to generate only one cap (top or bottom)
- Per-edge alignment options (snap to constraints in XY)
- Mesh smoothing pass (Laplacian constrained) on caps

Later
- Support for variable TargetEdgeLengthZ per band / schedule
- Multi-material or tags propagation on caps cells
- Performance: pooling LibTessDotNet objects, span-based indexing

Notes
- Keep polygons CCW and simple (no self-intersections)
- Holes must be inside the footprint and non-overlapping
