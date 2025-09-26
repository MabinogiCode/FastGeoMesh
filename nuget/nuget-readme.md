# FastGeoMesh

Fast quad meshing for prismatic volumes from 2D footprints and Z elevations.  
Geometry-only; builds side faces and caps, supports refinement near features,  
and exports an indexed quad mesh.

## Install
```bash
dotnet add package FastGeoMesh
```

## Quick start
```csharp
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;
using FastGeoMesh.Exporters;

// Define footprint (counter-clockwise polygon)
var poly = Polygon2D.FromPoints(new[] {
    new Vec2(0,0), new Vec2(20,0), new Vec2(20,5), new Vec2(0,5)
});

// Define prism and meshing options
var structure = new PrismStructureDefinition(poly, z0: 0, z1: 10);
var options = new MesherOptions {
    TargetEdgeLengthXY = 0.5,
    TargetEdgeLengthZ = 1.0,
    GenerateTopAndBottomCaps = true
};

// Generate mesh and convert to indexed form
var mesh = new PrismMesher().Mesh(structure, options);
var indexed = IndexedMesh.FromMesh(mesh, options.Epsilon);

// Export to common formats
ObjExporter.Write(indexed, "mesh.obj");
GltfExporter.Write(indexed, "mesh.gltf");
SvgExporter.Write(indexed, "mesh.svg");
```

## Key features
- Prism mesher with XY/Z control
- Caps (rectangle fast-path or generic tessellation + quadification)
- Quad quality scoring & threshold
- Exporters: OBJ (quads), glTF (triangulated), SVG (top view)

## Links
- Source & docs: https://github.com/MabinogiCode/FastGeoMesh
- License: MIT
