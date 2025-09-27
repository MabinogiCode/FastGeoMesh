# Roadmap

Versions follow Semantic Versioning. Minor (x.Y) add features backward?compatible; major introduce breaking changes.

## 1.1.x (Stabilisation / Maintenance)
### Goals
- Harden triangle emission path (multiple holes, thin slivers)
- Add performance baseline (BenchmarkDotNet) for rectangle vs generic caps
- Expose `QuadQuality` utility (public static) for external filtering
- Add validation script: `dotnet pack` dry run + NuGet icon sanity
- Improve test coverage >90% (holes+triangles)

### Tasks
- [ ] Benchmark project `FastGeoMesh.Benchmarks`
- [ ] Public method `QualityEvaluator.ScoreQuad(Vec2[])`
- [ ] Additional tests: multi-hole, tiny notch, high refinement
- [ ] CI: optional benchmark job (manual dispatch)

## 1.2 (Refinement & Multi-Volume)
### Goals
- Support meshing several `PrismStructureDefinition` and merging results
- Z offsets per hole (local base/top delta)
- OBJ grouping (g side / g cap_top / g cap_bottom)
- glTF optional normals (simple face averages)
- SVG coloring: internal segments vs boundary

### Tasks
- [ ] `CompositePrismMesher` (list -> merged Mesh)
- [ ] Hole metadata: local elevation delta
- [ ] OBJ exporter grouping
- [ ] glTF normals flag (`IncludeNormals`)
- [ ] SVG exporter color options

## 1.3 (Performance & Simplification)
### Goals
- Parallel rectangle cap generation
- Quadification cache (avoid recomputing same triangle pair ordering)
- Post-process: merge adjacent coplanar quads (optional)
- New quality criteria: skew & area ratio

### Tasks
- [ ] Parallel rectangle tiling (Partitioner)
- [ ] Pair candidate hash + memo
- [ ] `MeshSimplifier.MergeCoplanarQuads(epsilon)`
- [ ] Extend quality struct (aspect, orthogonality, skew, area ratio)

## 1.4 (Geometry Extensions)
### Goals
- Open polylines refinement (not full holes)
- Weighted segments (priority refinement radius)
- Local density points (radius + target XY override)

### Tasks
- [ ] `RefinementPolyline` support
- [ ] Segment weight -> dynamic local target length
- [ ] Point density rule evaluation pass

## 1.5 (Export & Interop)
### Goals
- glTF binary (.glb)
- PLY export (triangulated)
- Simple OBJ import -> `IndexedMesh`
- JSON metadata export (options + stats)

### Tasks
- [ ] `GltfExporter.WriteGlb`
- [ ] `PlyExporter`
- [ ] `ObjImporter`
- [ ] `MeshMetadata` (serialize JSON)

## 1.6 (Tooling & CLI)
### Goals
- CLI tool `fastgeomesh` for scripting
- GeoJSON footprint ingestion
- Auto doc generation (DocFX or xml->md)
- Bench suite integrated (command flag)

### Tasks
- [ ] New project `FastGeoMesh.Cli`
- [ ] GeoJSON parser (footprint + holes)
- [ ] Doc generation pipeline
- [ ] Benchmark command

## 2.0 (Planned Breaking Changes)
### Goals
- Immutable `MesherOptions` (record + builder)
- Interface-based refinement rules (`IRefinementRule`)
- Float precision mode (compile symbol FASTGEOMESH_SINGLE)
- Unified export interface `IMeshWriter`
- Source generator for options validation

### Tasks
- [ ] `MesherOptions` refactor
- [ ] Rule pipeline & registration
- [ ] Conditional float structs
- [ ] Export registry
- [ ] Analyzer / source generator project

## Backlog / R&D
- Adaptive curvature-guided subdivision (future non-prismatic variants)
- Global quad optimization (improve shape regularity) heuristic pass
- WebAssembly sample (Blazor) for interactive meshing

## Priorities Snapshot
P0: Triangle stability, benchmarks, normals support.  
P1: Multi-volume, CLI, quad simplification.  
P2: Additional exporters, refinement plugins.  
P3: 2.0 refactor tasks.

## Definition of Done (feature)
- Tests updated / added, coverage unchanged or higher
- Bench (if perf-sensitive) shows no regression >10%
- CHANGELOG entry, README updated if user-visible
- No new analyzer warnings (warnings-as-errors maintained)
