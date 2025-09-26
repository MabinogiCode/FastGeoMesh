# CHANGELOG

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.0] - 2025-09-26
### Added
- `MesherOptions.OutputRejectedCapTriangles`: emit leftover cap faces as true triangles (non?degenerate) instead of forming degenerate quads.
- Triangle pipeline support: `Mesh.Triangles`, `IndexedMesh.Triangles`, OBJ exporter now outputs both quad and triangle faces, glTF exporter includes standalone triangles.
- Tests: `CapTriangleFallbackTests`, `ObjTriangleExportTests`.

### Changed
- README & nuget-readme updated (triangle support, new option documented).
- Package description & tags now mention triangles.

## [1.0.0] - 2025-09-26
### Added
- Prism mesher for CCW polygon footprints with vertical extrusion.
- Side face subdivision (XY) and vertical level generation (Z) with constraint segments.
- Caps: rectangle fast-path + generic tessellation + quad pairing with quality scoring.
- Quad quality metric & `MinCapQuadQuality` threshold.
- Geometry integration (points, 3D segments) into raw mesh & indexing.
- Indexed mesh utilities: edges, adjacency builder, custom text IO.
- Exporters: OBJ (quads), glTF (triangulated), SVG (top view), custom text.
- Mesher options validation & refinement bands (holes / segments).
- Comprehensive XML documentation for public API.
- CI workflow (build + test) and publish-on-tag workflow (NuGet).
- Documentation set (getting started, mesher options, exporters, performance, FAQ, indexed format).

### Changed
- Default `Epsilon` to 1e-9 (practical precision) instead of `double.Epsilon`.

### Removed
- Legacy debug builder methods from `IndexedMesh`.

### Security
- N/A

[1.1.0]: https://github.com/MabinogiCode/FastGeoMesh/releases/tag/v1.1.0
[1.0.0]: https://github.com/MabinogiCode/FastGeoMesh/releases/tag/v1.0.0
