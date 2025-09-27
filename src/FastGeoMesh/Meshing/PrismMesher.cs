using System;
using System.Collections.Generic;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Structures;
using LibTessDotNet;
using GVec3 = FastGeoMesh.Geometry.Vec3;
using LTessVec3 = LibTessDotNet.Vec3;

namespace FastGeoMesh.Meshing
{
    /// <summary>Prism mesher producing quad-dominant meshes (side quads + cap quads, optional cap triangles).</summary>
    public sealed class PrismMesher : IMesher<PrismStructureDefinition>
    {
        /// <summary>Generate a mesh from the given prism structure definition and meshing options.</summary>
        public Mesh Mesh(PrismStructureDefinition structure, MesherOptions options)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();
            var mesh = new Mesh();
            double z0 = structure.CoteBase;
            double z1 = structure.CoteTete;
            var zLevels = BuildZLevels(z0, z1, options, structure);
            AddSideFaces(mesh, structure.Footprint.Vertices, zLevels, options, outward: true);
            foreach (var hole in structure.Holes)
            {
                AddSideFaces(mesh, hole.Vertices, zLevels, options, outward: false);
            }
            if (options.GenerateBottomCap || options.GenerateTopCap)
            {
                AddCaps(mesh, structure, options, z0, z1);
            }
            foreach (var p in structure.Geometry.Points)
            {
                mesh.AddPoint(p);
            }
            foreach (var s in structure.Geometry.Segments)
            {
                mesh.AddInternalSegment(s);
            }
            return mesh;
        }

        private static void AddSideFaces(Mesh mesh, IReadOnlyList<Vec2> loop, List<double> zLevels, MesherOptions options, bool outward)
        {
            int n = loop.Count;
            for (int i = 0; i < n; i++)
            {
                var a2 = loop[i];
                var b2 = loop[(i + 1) % n];
                var edge = new Segment2D(a2, b2);
                int hDiv = Math.Max(1, (int)Math.Ceiling(edge.Length() / options.TargetEdgeLengthXY));
                for (int hi = 0; hi < hDiv; hi++)
                {
                    double t0 = (double)hi / hDiv;
                    double t1 = (double)(hi + 1) / hDiv;
                    var a0 = Lerp(a2, b2, t0);
                    var a1 = Lerp(a2, b2, t1);
                    for (int vi = 0; vi < zLevels.Count - 1; vi++)
                    {
                        double za0 = zLevels[vi];
                        double za1 = zLevels[vi + 1];
                        var v00 = new GVec3(a0.X, a0.Y, za0);
                        var v01 = new GVec3(a1.X, a1.Y, za0);
                        var v11 = new GVec3(a1.X, a1.Y, za1);
                        var v10 = new GVec3(a0.X, a0.Y, za1);
                        mesh.AddQuad(outward ? new Quad(v00, v01, v11, v10) : new Quad(v01, v00, v10, v11));
                    }
                }
            }
        }

        private static void AddCaps(Mesh mesh, PrismStructureDefinition structure, MesherOptions options, double z0, double z1)
        {
            bool genBottom = options.GenerateBottomCap;
            bool genTop = options.GenerateTopCap;
            if (!genBottom && !genTop)
            {
                return;
            }
            bool outputTris = options.OutputRejectedCapTriangles;
            if (structure.Footprint.IsRectangleAxisAligned(out var min, out var max))
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

                bool UseFineForCell(double cx, double cy)
                {
                    bool nearHole = holeBand > 0 && IsNearAnyHole(structure, cx, cy, holeBand);
                    bool nearSeg = segBand > 0 && IsNearAnySegment(structure, cx, cy, segBand);
                    return nearHole || nearSeg;
                }
                static double LerpScalar(double a, double b, double t) => a + (b - a) * t;

                void EmitGrid(int gx, int gy, Func<double, double, bool> predicate)
                {
                    for (int i = 0; i < gx; i++)
                    {
                        for (int j = 0; j < gy; j++)
                        {
                            double x0 = LerpScalar(min.X, max.X, (double)i / gx);
                            double x1 = LerpScalar(min.X, max.X, (double)(i + 1) / gx);
                            double y0 = LerpScalar(min.Y, max.Y, (double)j / gy);
                            double y1 = LerpScalar(min.Y, max.Y, (double)(j + 1) / gy);
                            double cx = 0.5 * (x0 + x1);
                            double cy = 0.5 * (y0 + y1);
                            if (!PointInPolygon(structure.Footprint.Vertices, cx, cy))
                            {
                                continue;
                            }
                            if (IsInsideAnyHole(structure, cx, cy))
                            {
                                continue;
                            }
                            if (!predicate(cx, cy))
                            {
                                continue;
                            }
                            if (genBottom)
                            {
                                var b0 = new GVec3(x0, y0, z0);
                                var b1 = new GVec3(x0, y1, z0);
                                var b2 = new GVec3(x1, y1, z0);
                                var b3 = new GVec3(x1, y0, z0);
                                mesh.AddQuad(new Quad(b0, b1, b2, b3));
                            }
                            if (genTop)
                            {
                                var t0 = new GVec3(x0, y0, z1);
                                var t1 = new GVec3(x1, y0, z1);
                                var t2 = new GVec3(x1, y1, z1);
                                var t3 = new GVec3(x0, y1, z1);
                                mesh.AddQuad(new Quad(t0, t1, t2, t3));
                            }
                        }
                    }
                }

                EmitGrid(nx, ny, (cx, cy) => !UseFineForCell(cx, cy));
                if (holeBand > 0 && fineHoles < options.TargetEdgeLengthXY)
                {
                    EmitGrid(nxFineH, nyFineH, (cx, cy) => IsNearAnyHole(structure, cx, cy, holeBand));
                }
                if (segBand > 0 && fineSegs < options.TargetEdgeLengthXY)
                {
                    EmitGrid(nxFineS, nyFineS, (cx, cy) => IsNearAnySegment(structure, cx, cy, segBand));
                }
                return;
            }

            var tess = new Tess();
            void AddContour(Polygon2D poly)
            {
                var contour = new ContourVertex[poly.Count];
                for (int i = 0; i < poly.Count; i++)
                {
                    contour[i].Position = new LTessVec3((float)poly.Vertices[i].X, (float)poly.Vertices[i].Y, 0f);
                    contour[i].Data = null;
                }
                tess.AddContour(contour, ContourOrientation.Original);
            }
            AddContour(structure.Footprint);
            foreach (var h in structure.Holes)
            {
                AddContour(h);
            }
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
            var verts = tess.Vertices;
            var elements = tess.Elements;
            int triCount = tess.ElementCount;
            var triangles = new List<(int a, int b, int c)>();
            for (int i = 0; i < triCount; i++)
            {
                int a = elements[i * 3 + 0];
                int b = elements[i * 3 + 1];
                int c = elements[i * 3 + 2];
                if (a == -1 || b == -1 || c == -1)
                {
                    continue;
                }
                triangles.Add((a, b, c));
            }
            var edgeToTris = new Dictionary<(int, int), List<int>>();
            for (int ti = 0; ti < triangles.Count; ti++)
            {
                var (a, b, c) = triangles[ti];
                AddEdge(a, b, ti);
                AddEdge(b, c, ti);
                AddEdge(c, a, ti);
            }
            var candidates = new List<(double score, int t0, int t1, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) quad)>();
            foreach (var kv in edgeToTris)
            {
                var inc = kv.Value;
                if (inc.Count != 2)
                {
                    continue;
                }
                int t0 = inc[0];
                int t1 = inc[1];
                var quad = MakeQuadFromTriPair(triangles[t0], triangles[t1], verts);
                if (!quad.HasValue)
                {
                    continue;
                }
                var q = quad.Value;
                double score = ScoreQuad(q);
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
            for (int ti = 0; ti < triangles.Count; ti++)
            {
                if (paired[ti])
                {
                    continue;
                }
                var (a, b, c) = triangles[ti];
                var v0 = new Vec2((float)verts[a].Position.X, (float)verts[a].Position.Y);
                var v1 = new Vec2((float)verts[b].Position.X, (float)verts[b].Position.Y);
                var v2 = new Vec2((float)verts[c].Position.X, (float)verts[c].Position.Y);
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
                    EmitQuad(mesh, (v0, v1, v2, v2), z0, z1, genBottom, genTop);
                }
            }
            return;

            void AddEdge(int i, int j, int tri)
            {
                var key = i < j ? (i, j) : (j, i);
                if (!edgeToTris.TryGetValue(key, out var list))
                {
                    list = new List<int>(2);
                    edgeToTris[key] = list;
                }
                list.Add(tri);
            }

            (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3)? MakeQuadFromTriPair((int a, int b, int c) t0, (int a, int b, int c) t1, ContourVertex[] v)
            {
                var set0 = new HashSet<int> { t0.a, t0.b, t0.c };
                var set1 = new HashSet<int> { t1.a, t1.b, t1.c };
                var shared = set0.Intersect(set1).ToArray();
                if (shared.Length != 2)
                {
                    return null;
                }
                int s0 = shared[0];
                int s1 = shared[1];
                int u0 = set0.Except(shared).First();
                int u1 = set1.Except(shared).First();
                var va = new Vec2((float)v[s0].Position.X, (float)v[s0].Position.Y);
                var vb = new Vec2((float)v[s1].Position.X, (float)v[s1].Position.Y);
                var vc = new Vec2((float)v[u0].Position.X, (float)v[u0].Position.Y);
                var vd = new Vec2((float)v[u1].Position.X, (float)v[u1].Position.Y);
                var quad = (va, vc, vb, vd);
                if (IsConvex(quad))
                {
                    return quad;
                }
                quad = (va, vd, vb, vc);
                return IsConvex(quad) ? quad : null;
            }

            static bool IsConvex((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) q)
            {
                double c1 = (q.v1 - q.v0).Cross(q.v2 - q.v1);
                double c2 = (q.v2 - q.v1).Cross(q.v3 - q.v2);
                double c3 = (q.v3 - q.v2).Cross(q.v0 - q.v3);
                double c4 = (q.v0 - q.v3).Cross(q.v1 - q.v0);
                return c1 >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0;
            }

            static double ScoreQuad((Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) q)
            {
                double l0 = (q.v1 - q.v0).Length();
                double l1 = (q.v2 - q.v1).Length();
                double l2 = (q.v3 - q.v2).Length();
                double l3 = (q.v0 - q.v3).Length();
                double minL = Math.Min(Math.Min(l0, l1), Math.Min(l2, l3));
                double maxL = Math.Max(Math.Max(l0, l1), Math.Max(l2, l3));
                double aspect = minL <= 1e-9 ? 0 : minL / maxL;
                double o0 = Ortho((q.v1 - q.v0), (q.v2 - q.v1));
                double o1 = Ortho((q.v2 - q.v1), (q.v3 - q.v2));
                double o2 = Ortho((q.v3 - q.v2), (q.v0 - q.v3));
                double o3 = Ortho((q.v0 - q.v3), (q.v1 - q.v0));
                double ortho = 1.0 - 0.25 * (o0 + o1 + o2 + o3);
                double area = Math.Abs(Polygon2D.SignedArea(new[] { q.v0, q.v1, q.v2, q.v3 }));
                double areaScore = area > 1e-12 ? 1.0 : 0.0;
                return 0.6 * aspect + 0.35 * ortho + 0.05 * areaScore;
            }

            static double Ortho(Vec2 a, Vec2 b)
            {
                double na = Math.Sqrt(a.Dot(a));
                double nb = Math.Sqrt(b.Dot(b));
                if (na <= 1e-12 || nb <= 1e-12)
                {
                    return 0;
                }
                return 1.0 - Math.Abs(a.Dot(b) / (na * nb));
            }

            void EmitQuad(Mesh m, (Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3) q, double zb, double zt, bool emitBottom, bool emitTop)
            {
                double score = ScoreQuad(q);
                if (emitBottom)
                {
                    var b0 = new GVec3(q.v0.X, q.v0.Y, zb);
                    var b1 = new GVec3(q.v1.X, q.v1.Y, zb);
                    var b2 = new GVec3(q.v2.X, q.v2.Y, zb);
                    var b3 = new GVec3(q.v3.X, q.v3.Y, zb);
                    mesh.AddQuad(new Quad(b0, b1, b2, b3) { QualityScore = score });
                }
                if (emitTop)
                {
                    var t0 = new GVec3(q.v0.X, q.v0.Y, zt);
                    var t1 = new GVec3(q.v1.X, q.v1.Y, zt);
                    var t2 = new GVec3(q.v2.X, q.v2.Y, zt);
                    var t3 = new GVec3(q.v3.X, q.v3.Y, zt);
                    mesh.AddQuad(new Quad(t0, t1, t2, t3) { QualityScore = score });
                }
            }
        }

        private static bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band)
        {
            var p = new Vec2(x, y);
            foreach (var s in structure.Geometry.Segments)
            {
                var a = new Vec2(s.A.X, s.A.Y);
                var b = new Vec2(s.B.X, s.B.Y);
                if (DistancePointToSegment(p, a, b) <= band)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band)
        {
            foreach (var h in structure.Holes)
            {
                var verts = h.Vertices;
                for (int i = 0, j = verts.Count - 1; i < verts.Count; j = i++)
                {
                    var a = verts[j];
                    var b = verts[i];
                    double d = DistancePointToSegment(new Vec2(x, y), a, b);
                    if (d <= band)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y)
        {
            foreach (var h in structure.Holes)
            {
                if (PointInPolygon(h.Vertices, x, y))
                {
                    return true;
                }
            }
            return false;
        }

        private static double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b)
        {
            var ab = b - a;
            var ap = p - a;
            double t = Math.Max(0, Math.Min(1, (ab.Dot(ap)) / (ab.Dot(ab) + double.Epsilon)));
            var c = new Vec2(a.X + ab.X * t, a.Y + ab.Y * t);
            return (p - c).Length();
        }

        private static bool PointInPolygon(IReadOnlyList<Vec2> verts, double x, double y)
        {
            bool inside = false;
            for (int i = 0, j = verts.Count - 1; i < verts.Count; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                bool intersect = ((vi.Y > y) != (vj.Y > y)) &&
                                 (x < (vj.X - vi.X) * (y - vi.Y) / ((vj.Y - vi.Y) + double.Epsilon) + vi.X);
                if (intersect)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static List<double> BuildZLevels(double z0, double z1, MesherOptions options, PrismStructureDefinition structure)
        {
            var levels = new SortedSet<double> { z0, z1 };
            int vDiv = Math.Max(1, (int)Math.Ceiling((z1 - z0) / options.TargetEdgeLengthZ));
            for (int i = 1; i < vDiv; i++)
            {
                _ = levels.Add(z0 + (z1 - z0) * ((double)i / vDiv));
            }
            foreach (var (_, z) in structure.ConstraintSegments)
            {
                if (z > z0 + options.Epsilon && z < z1 - options.Epsilon)
                {
                    _ = levels.Add(z);
                }
            }
            foreach (var p in structure.Geometry.Points)
            {
                if (p.Z > z0 + options.Epsilon && p.Z < z1 - options.Epsilon)
                {
                    _ = levels.Add(p.Z);
                }
            }
            foreach (var s in structure.Geometry.Segments)
            {
                if (s.A.Z > z0 + options.Epsilon && s.A.Z < z1 - options.Epsilon)
                {
                    _ = levels.Add(s.A.Z);
                }
                if (s.B.Z > z0 + options.Epsilon && s.B.Z < z1 - options.Epsilon)
                {
                    _ = levels.Add(s.B.Z);
                }
            }
            return levels.ToList();
        }

        private static Vec2 Lerp(in Vec2 a, in Vec2 b, double t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    }
}
