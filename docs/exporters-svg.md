# Exporter: SVG (Top View)

Produces a 2D SVG projection (top?view) of the mesh edges.

## Characteristics
- Scales geometry to fit a viewport (default target width ? 800 unless `scale` supplied)
- Axis: +X ? right, +Y ? up (internally flipped to SVG coordinate system)
- Renders each edge as `<line>` with uniform stroke

## Example Output
```xml
<svg xmlns='http://www.w3.org/2000/svg' width='800' height='320' viewBox='0 0 800 320' shape-rendering='crispEdges'>
  <g stroke='#222' stroke-width='1' fill='none'>
    <line x1='0' y1='0' x2='800' y2='0' />
    ...
  </g>
</svg>
```

## Usage
```csharp
SvgExporter.Write(indexedMesh, "mesh.svg");
// Optional scale override & stroke width
SvgExporter.Write(indexedMesh, "mesh_scaled.svg", strokeWidth: 0.5, scale: 50);
```

## Parameters
| Parameter | Default | Meaning |
|-----------|---------|---------|
| strokeWidth | 1.0 | SVG line stroke width |
| scale | auto-fit | If provided, uniform scale factor applied |

## Limitations
| Aspect | Support | Notes |
|--------|---------|-------|
| Holes distinction | No | Holes edges render identical (no fill) |
| Edge classification | No | All edges same style |
| Coordinate precision | Standard double?string | Large coordinates may lose fine precision |
| Z info | Lost | Pure XY projection |

## Tips
- Post-style with CSS or DOM manipulation (color by layer, etc.).  
- For large models, consider downsampling edges or filtering internal edges first.  

## Roadmap Ideas
- Color coding for constraint levels / refinement zones
- Optional bounding box padding
- Layering (caps vs side edges)
