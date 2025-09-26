# Exporter: glTF 2.0

FastGeoMesh emits a `.gltf` (JSON) file with an embedded base64 buffer:
- Positions (FLOAT, VEC3)
- Indices (UNSIGNED_INT) — quads triangulated (two triangles each)
- Single mesh / single node / single scene

## Triangle Generation
Each quad (v0,v1,v2,v3) becomes triangles:
```
(v0,v1,v2) and (v0,v2,v3)
```
Preserves winding (CCW) for consistent front face.

## Example Snippet
```json
{
  "asset": { "version": "2.0", "generator": "FastGeoMesh" },
  "buffers": [ { "uri": "data:application/octet-stream;base64,...", "byteLength": 360 } ],
  "bufferViews": [ ... ],
  "accessors": [ ... ],
  "meshes": [ { "primitives": [ { "attributes": { "POSITION": 0 }, "indices": 1, "mode": 4 } ] } ],
  "nodes": [ { "mesh": 0 } ],
  "scenes": [ { "nodes": [0] } ],
  "scene": 0
}
```

## Limitations
| Aspect | Support | Notes |
|--------|---------|-------|
| Positions | Yes | Single accessor |
| Indices   | Yes | Triangulated quads |
| Normals   | No | Viewer will auto-compute or post-process |
| UVs       | No | Not generated |
| Colors    | No | Flat geometry only |
| Multiple meshes | No | All quads in single primitive |
| Binary `.glb` | No | Future enhancement |

## Usage
```csharp
GltfExporter.Write(indexedMesh, "mesh.gltf");
```

## Interop Notes
- Most viewers require normals; they will usually compute them if absent.  
- To add normals/UVs, post-process with a 3D library (Assimp, glTF pipeline, etc.).  
- File size may grow with base64; a future `.glb` exporter would reduce overhead.

## Roadmap Ideas
- `.glb` binary output
- Optional normal generation
- Optional per-quad metadata to per-face attributes
