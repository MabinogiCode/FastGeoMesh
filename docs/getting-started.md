# Getting Started

This guide walks through installing FastGeoMesh, generating a simple prism mesh, applying basic refinement, and exporting to common formats.

## 1. Install
```bash
dotnet add package FastGeoMesh
```

## 2. Define a footprint
The mesher expects a CCW (counter?clockwise) simple polygon:
```csharp
var footprint = Polygon2D.FromPoints(new[] {
    new Vec2(0,0), new Vec2(12,0), new Vec2(12,4), new Vec2(0,4)
});
```
If the points are CW, FastGeoMesh automatically reverses them.

## 3. Create the prism structure
```csharp
var structure = new PrismStructureDefinition(footprint, z0: 0, z1: 6);
```
Add (optional) holes:
```csharp
// structure.AddHole(Polygon2D.FromPoints(new[]{ new Vec2(4,1), new Vec2(5,1), new Vec2(5,2), new Vec2(4,2) }));
```

## 4. Add constraints / geometry (optional)
Constraint segments force a horizontal Z level:
```csharp
structure.AddConstraintSegment(new Segment2D(new Vec2(0,0), new Vec2(12,0)), 2.5);
```
Add auxiliary geometry (points / 3D segments) that will appear in the raw mesh:
```csharp
structure.Geometry
    .AddPoint(new Vec3(1, 3, 1))
    .AddPoint(new Vec3(10, 3, 4))
    .AddSegment(new Segment3D(new Vec3(1, 3, 1), new Vec3(10, 3, 4)));
```

## 5. Meshing options
```csharp
var options = new MesherOptions
{
    TargetEdgeLengthXY = 0.75,
    TargetEdgeLengthZ  = 1.0,
    HoleRefineBand = 1.0,
    SegmentRefineBand = 1.0,
    TargetEdgeLengthXYNearHoles = 0.4,
    TargetEdgeLengthXYNearSegments = 0.5,
    MinCapQuadQuality = 0.7
};
```

## 6. Generate the mesh
```csharp
var mesh = new PrismMesher().Mesh(structure, options);
```
Convert to indexed form (deduplicated vertices, edges, quads):
```csharp
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);
```

## 7. Export
```csharp
ObjExporter.Write(indexed, "example.obj");      // Quads preserved
GltfExporter.Write(indexed, "example.gltf");    // Triangulated
SvgExporter.Write(indexed, "example.svg");      // Top view edges
indexed.WriteCustomTxt("example.txt");          // Custom text format
```

## 8. Basic adjacency / quality checks
```csharp
var adjacency = indexed.BuildAdjacency();
if (adjacency.NonManifoldEdges.Count > 0)
    Console.WriteLine("Warning: non?manifold edges present");
```
Quality scores are attached to cap quads (`Quad.QualityScore`). Side faces have `null`.

## 9. Troubleshooting
| Issue | Cause | Fix |
|-------|-------|-----|
| Empty mesh | Invalid polygon (degenerate / self?intersections) | Validate footprint polygon first |
| Missing constraint level | Z not strictly inside (z0,z1) | Ensure z0 < Z < z1 |
| Too many quads | Reduce refinement bands or increase target lengths | Adjust `HoleRefineBand`, `SegmentRefineBand` |
| Jagged caps | Quality threshold too high | Lower `MinCapQuadQuality` |

## 10. Next steps
- See `mesher-options.md` for a deep dive on configuration.
- Check `exporters-*.md` pages for format specifics.
- Review `indexed-mesh-format.md` for interchange text format.
