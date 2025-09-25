# FastGeoMesh Documentation

Overview
- Fast quad meshing for prismatic domains (geometry-only)
- Inputs: Polygon2D footprint (CCW), z0 (base), z1 (top), optional holes, optional geometry (points/segments), optional constraint segments at Z
- Output: Mesh (quads + geometry) and IndexedMesh export

Core types
- Polygon2D: CCW polygon, validation, area, perimeter, rectangle detection
- PrismStructureDefinition: footprint + Zs, holes, constraint segments (Segment2D at Z), geometry (points/segments)
- MesherOptions: tunable parameters (edge lengths, caps, refinement bands, quad quality threshold)
- PrismMesher: main mesher (side faces, caps)
- Mesh: quads + pure geometry
- IndexedMesh: indexed representation + adjacency + text IO

Using the mesher
1) Create a valid CCW footprint polygon
2) Create PrismStructureDefinition(footprint, z0, z1)
3) Optionally add holes: AddHole(Polygon2D)
4) Optionally add constraint segments at Z: AddConstraintSegment(Segment2D, z)
5) Optionally add geometry: Geometry.AddPoint(Vec3)/AddSegment(Segment3D)
6) Configure MesherOptions
7) Call new PrismMesher().Mesh(structure, options)
8) Optionally convert to IndexedMesh and export (text, OBJ, glTF)

Quality controls
- Quad.QualityScore: [0..1] quality for caps quads (null on side quads)
- MesherOptions.MinCapQuadQuality: reject low-quality pairs (default 0.75)

Refinement
- Rectangular caps fast-path supports finer XY near holes or segments
- Use HoleRefineBand, SegmentRefineBand and TargetEdgeLengthXYNear* to drive refinement

Export
- Custom text format: IndexedMesh.WriteCustomTxt/ReadCustomTxt
- OBJ: FastGeoMesh.Meshing.Exporters.ObjExporter.Write(indexed, path)
- glTF 2.0 (.gltf): FastGeoMesh.Meshing.Exporters.GltfExporter.Write(indexed, path)

Examples
- See samples/FastGeoMesh.Sample/Program.cs (writes mesh.txt, mesh.obj; glTF can be added similarly)
- See tests for shapes, holes refinement and quality threshold
