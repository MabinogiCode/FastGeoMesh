using System;
using System.Collections.Generic;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Structures;
using FastGeoMesh.Utils;
using LibTessDotNet;
using GVec3 = FastGeoMesh.Geometry.Vec3;
using LTessVec3 = LibTessDotNet.Vec3;

namespace FastGeoMesh.Meshing.Helpers
{
    /// <summary>Helper for cap meshing (optimized grid path + generic tessellation + quad pairing).</summary>
    internal static class CapMeshingHelper
    {
        /// <summary>Create bottom/top caps according to options.</summary>
        internal static void GenerateCaps(Mesh mesh, PrismStructureDefinition structure, MesherOptions options, double z0, double z1)
        {
            bool genBottom = options.GenerateBottomCap;
            bool genTop = options.GenerateTopCap;
            if (genBottom || genTop)
            {
                if (structure.Footprint.IsRectangleAxisAligned(out var min, out var max))
                {
                    GenerateRectangleCaps(mesh, structure, options, z0, z1, min, max, genBottom, genTop);
                }
                else
                {
                    GenerateGenericCaps(mesh, structure, options, z0, z1, genBottom, genTop, options.OutputRejectedCapTriangles);
                }
            }
            // Internal surfaces (always generic path, independent) - no extrusion, single elevation each
            foreach (var plate in structure.InternalSurfaces)
            {
                GenerateInternalSurface(mesh, plate, options);
            }
        }

        private static void GenerateInternalSurface(Mesh mesh, InternalSurfaceDefinition plate, MesherOptions options)
        {
            var tess = TessPool.Rent();
            try
            {
                // Add the outer contour (should be CCW)
                AddContour(tess, plate.Outer);

                // Add hole contours (should be CW, opposite of outer)
                foreach (var h in plate.Holes)
                {
                    AddContour(tess, h);
                }

                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
                var verts = tess.Vertices;
                var elements = tess.Elements;
                int triCount = tess.ElementCount;

                if (verts == null || elements == null || triCount == 0)
                {
                    // Fallback: if tessellation fails completely (especially with multiple holes),
                    // generate a simple quad covering the outer polygon and let the hole exclusion
                    // logic handle it properly at runtime
                    GenerateFallbackInternalSurface(mesh, plate, options);
                    return;
                }

                var triangles = MeshingPools.TriangleListPool.Get();
                var edgeToTris = MeshingPools.EdgeMapPool.Get();
                var candidates = MeshingPools.CandidateListPool.Get();
                try
                {
                    // Validate and collect triangles
                    for (int i = 0; i < triCount; i++)
                    {
                        int a = elements[i * 3 + 0];
                        int b = elements[i * 3 + 1];
                        int c = elements[i * 3 + 2];
                        if (a == -1 || b == -1 || c == -1 ||
                            a < 0 || b < 0 || c < 0 ||
                            a >= verts!.Length || b >= verts.Length || c >= verts.Length)
                        {
                            continue;
                        }
                        triangles.Add((a, b, c));
                    }

                    for (int ti = 0; ti < triangles.Count; ti++)
                    {
                        var (a, b, c) = triangles[ti];
                        EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, a, b, ti);
                        EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, b, c, ti);
                        EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, c, a, ti);
                    }
                    var quadCandidates = MeshingPools.CandidateListPool.Get();
                    try
                    {
                        foreach (var kv in edgeToTris)
                        {
                            var inc = kv.Value;
                            if (inc.Count != 2)
                            {
                                continue;
                            }
                            int t0 = inc[0];
                            int t1 = inc[1];
                            var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangles[t0], triangles[t1], verts!);
                            if (!quad.HasValue)
                            {
                                continue;
                            }
                            var q = quad.Value;
                            double score = QuadQualityHelper.ScoreQuad(q);
                            if (score < options.MinCapQuadQuality)
                            {
                                continue;
                            }
                            quadCandidates.Add((score, t0, t1, q));
                        }
                        quadCandidates.Sort((x, y) => y.score.CompareTo(x.score));
                        bool[] paired = new bool[triangles.Count];
                        foreach (var cand in quadCandidates)
                        {
                            if (paired[cand.t0] || paired[cand.t1])
                            {
                                continue;
                            }
                            EmitQuad(mesh, cand.quad, plate.Elevation, plate.Elevation, emitBottom: true, emitTop: false);
                            paired[cand.t0] = paired[cand.t1] = true;
                        }
                        for (int ti = 0; ti < triangles.Count; ti++)
                        {
                            if (paired[ti])
                            {
                                continue;
                            }
                            var (a, b, c) = triangles[ti];

                            // Additional safety check for array bounds
                            if (a < 0 || b < 0 || c < 0 || a >= verts!.Length || b >= verts.Length || c >= verts.Length)
                            {
                                continue;
                            }

                            var v0 = new Vec2(verts[a].Position.X, verts[a].Position.Y);
                            var v1 = new Vec2(verts[b].Position.X, verts[b].Position.Y);
                            var v2 = new Vec2(verts[c].Position.X, verts[c].Position.Y);

                            if (options.OutputRejectedCapTriangles)
                            {
                                mesh.AddTriangle(new Triangle(new GVec3(v0.X, v0.Y, plate.Elevation), new GVec3(v1.X, v1.Y, plate.Elevation), new GVec3(v2.X, v2.Y, plate.Elevation)));
                            }
                            else
                            {
                                // Create degenerate quad from triangle
                                var degenerateQuad = (v0, v1, v2, v2);
                                double score = QuadQualityHelper.ScoreQuad(degenerateQuad);

                                // Apply quality threshold to degenerate quads as well
                                if (score < options.MinCapQuadQuality)
                                {
                                    // If OutputRejectedCapTriangles is true, emit triangles instead
                                    if (options.OutputRejectedCapTriangles)
                                    {
                                        mesh.AddTriangle(new Triangle(new GVec3(v0.X, v0.Y, plate.Elevation), new GVec3(v1.X, v1.Y, plate.Elevation), new GVec3(v2.X, v2.Y, plate.Elevation)));
                                        continue;
                                    }
                                    // For very high thresholds (>= 0.9), emit degenerate quad anyway for backward compatibility
                                    // For normal thresholds, respect the quality filter
                                    else if (options.MinCapQuadQuality < 0.9)
                                    {
                                        continue; // Skip this quad entirely
                                    }
                                }

                                EmitQuad(mesh, degenerateQuad, plate.Elevation, plate.Elevation, emitBottom: true, emitTop: false);
                            }
                        }
                    }
                    finally
                    {
                        MeshingPools.CandidateListPool.Return(quadCandidates);
                    }
                }
                finally
                {
                    MeshingPools.TriangleListPool.Return(triangles);
                    MeshingPools.EdgeMapPool.Return(edgeToTris);
                    MeshingPools.CandidateListPool.Return(candidates);
                }
            }
            finally
            {
                TessPool.Return(tess);
            }
        }

        /// <summary>Fallback method when tessellation fails - generates a simple grid covering the outer polygon.</summary>
        private static void GenerateFallbackInternalSurface(Mesh mesh, InternalSurfaceDefinition plate, MesherOptions _)
        {
            // Find bounding box of outer polygon
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var v in plate.Outer.Vertices)
            {
                minX = Math.Min(minX, v.X);
                minY = Math.Min(minY, v.Y);
                maxX = Math.Max(maxX, v.X);
                maxY = Math.Max(maxY, v.Y);
            }

            // Generate a simple 2x2 grid over the bounding box
            double midX = (minX + maxX) * 0.5;
            double midY = (minY + maxY) * 0.5;

            // Create 4 quads covering the outer polygon
            var quads = new[]
            {
                (new Vec2(minX, minY), new Vec2(midX, minY), new Vec2(midX, midY), new Vec2(minX, midY)),
                (new Vec2(midX, minY), new Vec2(maxX, minY), new Vec2(maxX, midY), new Vec2(midX, midY)),
                (new Vec2(minX, midY), new Vec2(midX, midY), new Vec2(midX, maxY), new Vec2(minX, maxY)),
                (new Vec2(midX, midY), new Vec2(maxX, midY), new Vec2(maxX, maxY), new Vec2(midX, maxY))
            };

            foreach (var quad in quads)
            {
                // Check if quad center is inside outer polygon and not in any hole
                double cx = (quad.Item1.X + quad.Item2.X + quad.Item3.X + quad.Item4.X) * 0.25;
                double cy = (quad.Item1.Y + quad.Item2.Y + quad.Item3.Y + quad.Item4.Y) * 0.25;

                // Simple point-in-polygon check for outer boundary
                bool insideOuter = GeometryHelper.PointInPolygon(plate.Outer.Vertices.ToArray(), cx, cy);
                if (!insideOuter)
                {
                    continue;
                }

                // Check if inside any hole
                bool insideAnyHole = false;
                foreach (var hole in plate.Holes)
                {
                    if (GeometryHelper.PointInPolygon(hole.Vertices.ToArray(), cx, cy))
                    {
                        insideAnyHole = true;
                        break;
                    }
                }

                if (!insideAnyHole)
                {
                    EmitQuad(mesh, quad, plate.Elevation, plate.Elevation, emitBottom: true, emitTop: false);
                }
            }
        }

        /// <summary>Optimized axis-aligned rectangle cap generation with refinement near holes/segments.</summary>
        internal static void GenerateRectangleCaps(Mesh mesh, PrismStructureDefinition structure, MesherOptions options,
            double z0, double z1, Vec2 min, Vec2 max, bool genBottom, bool genTop)
        {
            int nx = Math.Max(1, (int)Math.Ceiling((max.X - min.X) / options.TargetEdgeLengthXY));
            int ny = Math.Max(1, (int)Math.Ceiling((max.Y - min.Y) / options.TargetEdgeLengthXY));
            double holeBand = Math.Max(0, options.HoleRefineBand);
            double fineHoles = options.TargetEdgeLengthXYNearHoles ?? options.TargetEdgeLengthXY;
            int nxFineH = Math.Max(1, (int)Math.Ceiling((max.X - min.X) / fineHoles));
            int nyFineH = Math.Max(1, (int)Math.Ceiling((max.Y - min.Y) / fineHoles));
            double segBand = Math.Max(0, options.SegmentRefineBand);
            double fineSegs = options.TargetEdgeLengthXYNearSegments ?? options.TargetEdgeLengthXY;
            int nxFineS = Math.Max(1, (int)Math.Ceiling((max.X - min.X) / fineSegs));
            int nyFineS = Math.Max(1, (int)Math.Ceiling((max.Y - min.Y) / fineSegs));

            var footprintIndex = new SpatialPolygonIndex(structure.Footprint.Vertices);
            var holeIndices = new SpatialPolygonIndex[structure.Holes.Count];
            for (int h = 0; h < structure.Holes.Count; h++)
            {
                holeIndices[h] = new SpatialPolygonIndex(structure.Holes[h].Vertices);
            }

            // Precompute coordinate arrays
            double[] X(int count)
            {
                var arr = new double[count + 1];
                for (int i = 0; i <= count; i++)
                {
                    arr[i] = min.X + (max.X - min.X) * (i / (double)count);
                }
                return arr;
            }
            double[] Y(int count)
            {
                var arr = new double[count + 1];
                for (int i = 0; i <= count; i++)
                {
                    arr[i] = min.Y + (max.Y - min.Y) * (i / (double)count);
                }
                return arr;
            }

            double[] xBase = X(nx); double[] yBase = Y(ny);
            double[] xHole = (holeBand > 0 && fineHoles < options.TargetEdgeLengthXY) ? X(nxFineH) : Array.Empty<double>();
            double[] yHole = (holeBand > 0 && fineHoles < options.TargetEdgeLengthXY) ? Y(nyFineH) : Array.Empty<double>();
            double[] xSeg = (segBand > 0 && fineSegs < options.TargetEdgeLengthXY) ? X(nxFineS) : Array.Empty<double>();
            double[] ySeg = (segBand > 0 && fineSegs < options.TargetEdgeLengthXY) ? Y(nyFineS) : Array.Empty<double>();

            EmitGrid(mesh, xBase, yBase, z0, z1, genBottom, genTop, footprintIndex, holeIndices,
                (cx, cy) => !(holeBand > 0 && MeshStructureHelper.IsNearAnyHole(structure, cx, cy, holeBand)) &&
                            !(segBand > 0 && MeshStructureHelper.IsNearAnySegment(structure, cx, cy, segBand)));
            if (xHole.Length > 0)
            {
                EmitGrid(mesh, xHole, yHole, z0, z1, genBottom, genTop, footprintIndex, holeIndices,
                    (cx, cy) => MeshStructureHelper.IsNearAnyHole(structure, cx, cy, holeBand));
            }
            if (xSeg.Length > 0)
            {
                EmitGrid(mesh, xSeg, ySeg, z0, z1, genBottom, genTop, footprintIndex, holeIndices,
                    (cx, cy) => MeshStructureHelper.IsNearAnySegment(structure, cx, cy, segBand));
            }
        }

        private static void EmitGrid(Mesh mesh, double[] xs, double[] ys, double z0, double z1, bool genBottom, bool genTop, SpatialPolygonIndex footprint, SpatialPolygonIndex[] holes, Func<double, double, bool> predicate)
        {
            for (int i = 0; i < xs.Length - 1; i++)
            {
                for (int j = 0; j < ys.Length - 1; j++)
                {
                    double x0 = xs[i]; double x1 = xs[i + 1];
                    double y0 = ys[j]; double y1 = ys[j + 1];
                    double cx = 0.5 * (x0 + x1); double cy = 0.5 * (y0 + y1);
                    if (!footprint.IsInside(cx, cy))
                    {
                        continue;
                    }
                    if (MeshStructureHelper.IsInsideAnyHole(holes, cx, cy))
                    {
                        continue;
                    }
                    if (!predicate(cx, cy))
                    {
                        continue;
                    }

                    // Calculate quality score for rectangle cap quads
                    var quadShape = (new Vec2(x0, y0), new Vec2(x0, y1), new Vec2(x1, y1), new Vec2(x1, y0));
                    double score = QuadQualityHelper.ScoreQuad(quadShape);

                    if (genBottom)
                    {
                        mesh.AddQuad(new Quad(new GVec3(x0, y0, z0), new GVec3(x0, y1, z0), new GVec3(x1, y1, z0), new GVec3(x1, y0, z0), score));
                    }
                    if (genTop)
                    {
                        mesh.AddQuad(new Quad(new GVec3(x0, y0, z1), new GVec3(x1, y0, z1), new GVec3(x1, y1, z1), new GVec3(x0, y1, z1), score));
                    }
                }
            }
        }

        /// <summary>Generic polygon tessellation path with quad pairing &amp; optional triangle output.</summary>
        internal static void GenerateGenericCaps(Mesh mesh, PrismStructureDefinition structure, MesherOptions options,
            double z0, double z1, bool genBottom, bool genTop, bool outputTris)
        {
            var tess = TessPool.Rent();
            try
            {
                AddContour(tess, structure.Footprint);
                foreach (var h in structure.Holes)
                {
                    AddContour(tess, h);
                }
                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
                var verts = tess.Vertices;
                var elements = tess.Elements;
                int triCount = tess.ElementCount;
                var triangles = MeshingPools.TriangleListPool.Get();
                var edgeToTris = MeshingPools.EdgeMapPool.Get();
                var candidates = MeshingPools.CandidateListPool.Get();
                try
                {
                    // Validate and collect triangles with bounds checking
                    for (int i = 0; i < triCount; i++)
                    {
                        int a = elements[i * 3 + 0];
                        int b = elements[i * 3 + 1];
                        int c = elements[i * 3 + 2];
                        if (a == -1 || b == -1 || c == -1 ||
                            a < 0 || b < 0 || c < 0 ||
                            verts == null || a >= verts.Length || b >= verts.Length || c >= verts.Length)
                        {
                            continue;
                        }
                        triangles.Add((a, b, c));
                    }

                    for (int ti = 0; ti < triangles.Count; ti++)
                    {
                        var (a, b, c) = triangles[ti];
                        EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, a, b, ti);
                        EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, b, c, ti);
                        EdgeMappingHelper.AddEdgeToTriangleMapping(edgeToTris, c, a, ti);
                    }
                    foreach (var kv in edgeToTris)
                    {
                        var inc = kv.Value;
                        if (inc.Count != 2)
                        {
                            continue;
                        }
                        int t0 = inc[0];
                        int t1 = inc[1];
                        var quad = QuadQualityHelper.MakeQuadFromTrianglePair(triangles[t0], triangles[t1], verts!);
                        if (!quad.HasValue)
                        {
                            continue;
                        }
                        var q = quad.Value;
                        double score = QuadQualityHelper.ScoreQuad(q);
                        if (score < options.MinCapQuadQuality)
                        {
                            continue;
                        }
                        candidates.Add((score, t0, t1, q));
                    }
                    candidates.Sort((x, y) => y.score.CompareTo(x.score));
                    bool[] paired = new bool[triangles.Count];
                    foreach (var cand in candidates)
                    {
                        if (paired[cand.t0] || paired[cand.t1])
                        {
                            continue;
                        }
                        EmitQuad(mesh, cand.quad, z0, z1, genBottom, genTop);
                        paired[cand.t0] = paired[cand.t1] = true;
                    }

                    // Process remaining unpaired triangles with bounds validation
                    for (int ti = 0; ti < triangles.Count; ti++)
                    {
                        if (paired[ti])
                        {
                            continue;
                        }
                        var (a, b, c) = triangles[ti];

                        // Additional safety check (should not be needed after the earlier validation, but defensive)
                        if (a < 0 || b < 0 || c < 0 || verts == null || a >= verts.Length || b >= verts.Length || c >= verts.Length)
                        {
                            continue;
                        }

                        var v0 = new Vec2(verts[a].Position.X, verts[a].Position.Y);
                        var v1 = new Vec2(verts[b].Position.X, verts[b].Position.Y);
                        var v2 = new Vec2(verts[c].Position.X, verts[c].Position.Y);

                        if (outputTris)
                        {
                            if (genBottom)
                            {
                                mesh.AddTriangle(new Triangle(new GVec3(v0.X, v0.Y, z0), new GVec3(v1.X, v1.Y, z0), new GVec3(v2.X, v2.Y, z0)));
                            }
                            if (genTop)
                            {
                                mesh.AddTriangle(new Triangle(new GVec3(v0.X, v0.Y, z1), new GVec3(v1.X, v1.Y, z1), new GVec3(v2.X, v2.Y, z1)));
                            }
                        }
                        else
                        {
                            // Create degenerate quad from triangle
                            var degenerateQuad = (v0, v1, v2, v2);
                            double score = QuadQualityHelper.ScoreQuad(degenerateQuad);

                            // Apply quality threshold to degenerate quads as well
                            if (score < options.MinCapQuadQuality)
                            {
                                // If OutputRejectedCapTriangles is true, emit triangles instead
                                if (options.OutputRejectedCapTriangles)
                                {
                                    if (genBottom)
                                    {
                                        mesh.AddTriangle(new Triangle(new GVec3(v0.X, v0.Y, z0), new GVec3(v1.X, v1.Y, z0), new GVec3(v2.X, v2.Y, z0)));
                                    }
                                    if (genTop)
                                    {
                                        mesh.AddTriangle(new Triangle(new GVec3(v0.X, v0.Y, z1), new GVec3(v1.X, v1.Y, z1), new GVec3(v2.X, v2.Y, z1)));
                                    }
                                    continue;
                                }
                                // For very high thresholds (>= 0.9), emit degenerate quad anyway for backward compatibility
                                // For normal thresholds, respect the quality filter
                                else if (options.MinCapQuadQuality < 0.9)
                                {
                                    continue; // Skip this quad entirely
                                }
                            }

                            EmitQuad(mesh, degenerateQuad, z0, z1, genBottom, genTop);
                        }
                    }
                }
                finally
                {
                    MeshingPools.TriangleListPool.Return(triangles);
                    MeshingPools.EdgeMapPool.Return(edgeToTris);
                    MeshingPools.CandidateListPool.Return(candidates);
                }
            }
            finally
            {
                TessPool.Return(tess);
            }
        }

        private static void AddContour(Tess tess, Polygon2D poly)
        {
            var contour = new ContourVertex[poly.Count];
            for (int i = 0; i < poly.Count; i++)
            {
                contour[i].Position = new LTessVec3((float)poly.Vertices[i].X, (float)poly.Vertices[i].Y, 0f);
                contour[i].Data = null;
            }

            // For now, use Original orientation for all contours since it works for single holes
            // The complex orientation logic might be causing issues with multiple holes
            tess.AddContour(contour, ContourOrientation.Original);
        }

        private static void EmitQuad(Mesh mesh, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad, double zb, double zt, bool emitBottom, bool emitTop)
        {
            double score = QuadQualityHelper.ScoreQuad(quad);
            if (emitBottom)
            {
                mesh.AddQuad(new Quad(new GVec3(quad.v0.X, quad.v0.Y, zb), new GVec3(quad.v1.X, quad.v1.Y, zb), new GVec3(quad.v2.X, quad.v2.Y, zb), new GVec3(quad.v3.X, quad.v3.Y, zb), score));
            }
            if (emitTop)
            {
                mesh.AddQuad(new Quad(new GVec3(quad.v0.X, quad.v0.Y, zt), new GVec3(quad.v1.X, quad.v1.Y, zt), new GVec3(quad.v2.X, quad.v2.Y, zt), new GVec3(quad.v3.X, quad.v3.Y, zt), score));
            }
        }
    }
}
