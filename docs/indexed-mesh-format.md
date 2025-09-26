# Indexed Mesh Text Format

FastGeoMesh can serialize an indexed quad mesh into a simple text format for interchange or archival.

## Structure
```
<pointCount>
 id x y z
 ... (pointCount lines)
<edgeCount>
 id a b               # 1-based vertex indices
 ... (edgeCount lines)
<quadCount>
 id v0 v1 v2 v3       # 1-based vertex indices (CCW)
 ... (quadCount lines)
```
IDs in the first column are sequential (1..N) and primarily informational; parsers may ignore them and rely on order.

## Example
```
8
1 0 0 0
2 4 0 0
3 4 2 0
4 0 2 0
5 0 0 1
6 4 0 1
7 4 2 1
8 0 2 1
12
1 1 2
2 2 3
...
6
1 1 2 3 4
2 5 6 7 8
...
```

## Rationale
- Human readable & diff?friendly.
- Minimal — no normals/UV/meta; consumers augment after import.

## Relationship to Exporters
| Exporter | Orientation | Polygon Handling | Additional Data | Use Case |
|----------|-------------|------------------|-----------------|----------|
| OBJ | Quads | Quads native | None (no normals) | Direct DCC ingestion if quads accepted |
| glTF | Triangulated | Quads?Triangles | None | Web / viewer interoperability |
| SVG | Top projection | Edges only | None | 2D visualization / quick QA |
| Text (this) | Quads | As?is | None | Diff, custom pipelines |

## Conversion Hints
- Text ? OBJ: iterate vertices ? `v`; quads ? `f` lines.
- Text ? glTF: triangulate each quad then pack binary buffer.
- Text ? SVG: project XY, build edge set (deduplicate undirected pairs).

## Limitations
| Aspect | Comment |
|--------|---------|
| Large models | Verbose, consider binary for >1e6 verts |
| Performance | Parsing string based; stream if needed |
| Validation | No checksum; rely on consumer validation |

## See Also
- exporters-obj.md
- exporters-gltf.md
- exporters-svg.md
- mesher-options.md
