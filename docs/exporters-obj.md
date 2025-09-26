# Exporter: OBJ

FastGeoMesh writes a minimalist Wavefront OBJ file with:
- Vertex positions (`v x y z`)
- Quad faces (`f i0 i1 i2 i3`) 1?based indices

No normals, texture coordinates, groups, or materials are emitted (keeps file compact). Consumers can derive per?face normals if needed.

## Example Output
```
# FastGeoMesh OBJ export
v 0 0 0
v 4 0 0
v 4 2 0
v 0 2 0
v 0 0 1
v 4 0 1
v 4 2 1
v 0 2 1
f 1 2 3 4
f 5 6 7 8
f 1 2 6 5
f 2 3 7 6
f 3 4 8 7
f 4 1 5 8
```

## Limitations
| Aspect | Support | Notes |
|--------|---------|-------|
| Quads  | Yes | Native quad faces retained |
| Triangles | No direct | Convert externally if required |
| Normals | No | Compute externally (`vn`) if needed |
| UVs     | No | Add with downstream tool |
| Materials/MTL | No | Can be appended manually |

## Usage
```csharp
ObjExporter.Write(indexedMesh, "mesh.obj");
```

## Tips
- If a consumer cannot handle quads, run a triangulation tool afterwards (e.g. mesh processing library).  
- Keep `TargetEdgeLengthXY` moderate to avoid huge files.  
- Use the rectangle fast?path (axis?aligned rectangle footprint) for more regular grids.

## Roadmap Ideas
- Optional triangulation in exporter.  
- Optional normal and material generation toggle.  
