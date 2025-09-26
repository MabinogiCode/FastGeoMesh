# FastGeoMesh

Fast quad meshing for prismatic volumes from 2D footprints and Z elevations.

Features:
- Prism mesher (side faces + caps)
- Rectangle fast-path + generic tessellation
- Quad quality scoring & threshold
- Constraint Z levels & geometry integration
- Exporters: OBJ, glTF (triangulated), SVG (top view)

Quick start:
```csharp
var poly = Polygon2D.FromPoints(new[]{ new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5) });
var st = new PrismStructureDefinition(poly, -10, 10);
st.AddConstraintSegment(new Segment2D(new Vec2(0,0), new Vec2(20,0)), 2.5);
var opt = new MesherOptions { TargetEdgeLengthXY = 0.5, TargetEdgeLengthZ = 1.0 };
var mesh = new PrismMesher().Mesh(st, opt);
var indexed = IndexedMesh.FromMesh(mesh, opt.Epsilon);
ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
SvgExporter.Write(indexed, "mesh.svg");
```

License: MIT

Docs & source: https://github.com/MabinogiCode/FastGeoMesh
