# Roadmap

Status
- v0 delivered: core prism mesher, caps (rectangle fast?path + generic), holes, constraint Z levels, quad quality, geometry integration, indexed/exporters, docs/tests.
- v1+: refinement controls, alignment, performance, additional exporters & quality metrics.

v0 (delivered)
- [x] Prism meshing from CCW polygon footprint + [z0,z1]
- [x] Side faces (TargetEdgeLengthXY)
- [x] Z subdivision (TargetEdgeLengthZ + constraint / geometry levels)
- [x] Caps generation
  - [x] Rectangle fast?path with local refinement (holes / segments)
  - [x] Generic LibTessDotNet triangulation + quad pairing
- [x] Quad quality: score + MinCapQuadQuality filter
- [x] Geometry integration (points + 3D segments)
- [x] Indexed mesh (edges, adjacency, custom txt IO)
- [x] Exporters: OBJ, glTF, SVG top view
- [x] Tests & docs

v1 (planned)
Refinement
- [ ] Per?hole / per?segment refinement parameters (override global band & length)
- [ ] Anisotropic / directional refinement (different X vs Y target)

Caps & Alignment
- [ ] Deterministic cell alignment when mixing coarse/fine bands (no overlap emission)
- [ ] Optional snap of rectangular grid to user-supplied origin & spacing

Quality & Post?processing
- [ ] Extra metrics (skew, max angle deviation, aspect variance)
- [ ] Optional smoothing (Laplacian) on generic caps (boundary + segment constrained)
- [ ] Quality-driven re?pairing attempt (improve low score quads)

API / Extensibility
- [ ] IExporter abstraction (unify OBJ / glTF / SVG)
- [ ] Fluent MesherOptions builder + preset profiles
- [ ] CancellationToken support in mesher methods

Performance
- [ ] Object pooling for tessellation buffers
- [ ] Span/struct enumerators to reduce allocations in FromMesh / adjacency
- [ ] Parallel cap generation (rectangle path) when large grids

Export
- [ ] Binary glTF (.glb)
- [ ] STL (triangulated) optional
- [ ] Colored SVG (per Z band / per refinement region)

Advanced
- [ ] Variable vertical schedule (per band TargetEdgeLengthZ)
- [ ] Metadata / tagging on quads (originating feature, level index)
- [ ] Simple smoothing for side faces when large vertical aspect ratios

Nice to have / Exploration
- [ ] Multi-footprint stacking (layered prisms) -> compound mesh
- [ ] Optional triangulated side faces export
- [ ] Heightfield export utility (rasterization of top cap)

Notes
- Input polygons must be simple (no self-intersections) and CCW
- Holes must be strictly inside footprint, non-overlapping
- Epsilon tuning: 1e-9 default (adjust if coordinates are large magnitude)
