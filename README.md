# FastGeoMesh

Fast quad meshing for prismatic volumes from 2D footprints and Z elevations. Geometry-only: build side faces and caps, refine near features, and export an indexed mesh.

Features
- Prism mesher (counter-clockwise quads)
  - Side faces: XY subdivision using TargetEdgeLengthXY
  - Z subdivision: honors TargetEdgeLengthZ, constraint Zs (segments at Z), and geometry Zs
- Caps (top/bottom)
  - Rectangle fast-path: grid quads with optional refinement near holes and near geometry segments
  - Generic: LibTessDotNet triangulation + quadification with quality scoring and threshold
- Quality
  - Quad.QualityScore exposed on caps quads (null on side faces)
  - MinCapQuadQuality (default 0.75) to reject poor triangle pairings
- Integration of pure geometry
  - Points and 3D segments are carried through into the output Mesh
  - Constraint segments at a given Z affect vertical levels
- IndexedMesh utilities
  - Vertex/edge/quad indexing, adjacency builder, simple text format IO (read/write)
- Exporters
  - OBJ export (quads as f v0 v1 v2 v3)
  - glTF 2.0 export (.gltf JSON with embedded base64), quads triangulated
- Tests: various shapes, holes refinement, adjacency, quadification quality

Install / Build
- Requires .NET 8 SDK
- dotnet build
- dotnet test
- dotnet pack src/FastGeoMesh/FastGeoMesh.csproj -c Release

Quick start
```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

// Footprint (CCW)
var poly = Polygon2D.FromPoints(new[] {
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5)
});

// Prism definition with base/top elevations
var structure = new PrismStructureDefinition(poly, z0: -10, z1: 10);

// Optional: holes
// structure.AddHole(Polygon2D.FromPoints(new[]{ new Vec2(8,2), new Vec2(9,2), new Vec2(9,3), new Vec2(8,3) }));

// Optional: constraint segment (lierne) at Z = 2.5
structure.AddConstraintSegment(new Segment2D(new Vec2(0,0), new Vec2(20,0)), 2.5);

// Optional: pure geometry (points/segments)
structure.Geometry
    .AddPoint(new Vec3(0, 4, 2))
    .AddPoint(new Vec3(20, 4, 4))
    .AddSegment(new Segment3D(new Vec3(0, 4, 2), new Vec3(20, 4, 4)));

// Mesher options
var options = new MesherOptions
{
    TargetEdgeLengthXY = 0.5,
    TargetEdgeLengthZ = 1.0,
    GenerateTopAndBottomCaps = true,
    HoleRefineBand = 1.0,
    SegmentRefineBand = 1.0,
    TargetEdgeLengthXYNearHoles = 0.25,
    TargetEdgeLengthXYNearSegments = 0.25,
    MinCapQuadQuality = 0.75
};

// Mesh
var mesh = new PrismMesher().Mesh(structure, options);

// Convert to indexed mesh and export
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
indexed.WriteCustomTxt("mesh.txt");

// Export (OBJ / glTF)
using FastGeoMesh.Meshing.Exporters;
ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
```

Key options
- `TargetEdgeLengthXY`: target edge length in XY (side faces and caps grids)
- `TargetEdgeLengthZ`: target edge length along Z (vertical levels)
- `GenerateTopAndBottomCaps`: include caps
- `HoleRefineBand`: refine caps near holes within this band (rectangle fast-path)
- `SegmentRefineBand`: refine caps near 3D segments (projected XY) within this band (rectangle fast-path)
- `TargetEdgeLengthXYNearHoles`/`Segments`: finer XY near holes/segments (rectangle fast-path)
- `MinCapQuadQuality`: minimal quality [0..1] to accept pairing triangles into a quad on caps (generic path)

Text format (IndexedMesh)
- See `IndexedMesh.ReadCustomTxt`/`WriteCustomTxt`
- Points: count, then lines: id x y z
- Edges: count, then lines: id a b (1-based vertex ids)
- Quads: count, then lines: id v0 v1 v2 v3 (1-based vertex ids)

Roadmap
- See ROADMAP.md

License
- MIT, see LICENSE
