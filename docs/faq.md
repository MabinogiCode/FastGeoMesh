# FAQ

### Why must polygons be CCW?
The mesher assumes CCW for outward normal consistency. CW input is auto?reversed, but explicit CCW avoids ambiguity.

### Are self?intersections or duplicated vertices allowed?
No. Validation rejects degenerate or self?intersecting footprints (throws `ArgumentException`).

### Can I mesh non?prismatic / sloped shapes?
Not in this version. Only vertical extrusion between z0 and z1. Sloped / curved surfaces would require a different mesher pipeline.

### How are quads ordered?
Vertices stored CCW. Side faces follow loop order per vertical band; caps follow either structured grid or quadified tessellation.

### Why do some quads have duplicate vertices?
When a triangle pair fails quality threshold, remaining single triangle becomes a degenerate quad (v2==v3) to keep uniform quad storage.

### How do I get triangles instead of quads?
Use the glTF exporter (triangulated) or post?process OBJ/Text with a triangulation library.

### What happens if MinCapQuadQuality is set to 0?
All triangle pairs accepted ? more quads but potentially poor aspect/skew.

### Why no normals / UVs in exporters?
Scope focus: structural quad layout. Add them downstream (compute normals; unwrap if needed) pending future exporter enhancements.

### How do refinement bands overlap (holes + segments)?
If a cell is within either band it’s re?emitted with finer target (hole precedence is not exclusive; first matching rule applies). Duplicate emission avoided by predicate logic.

### Can I change Epsilon dynamically per axis?
Currently single scalar. For anisotropic scaling, consider pre?normalizing coordinates or contribute a feature.

### Large coordinates cause vertex merging artifacts—what do I adjust?
Increase `Epsilon` (e.g. to 1e?7) so quantization bins separate close points reliably.

### Is the format stable?
Yes for 1.x. Minor additive fields (e.g. normals) may appear in future exporters; core text format stable.

### License?
MIT. See LICENSE.
