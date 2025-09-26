using System.Collections.ObjectModel;
using System.Globalization;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing;

/// <summary>
/// Indexed mesh (vertices, edges, quads) built from a geometry-only Mesh.
/// Provides import/export of the simple text format: count + items, 1-based indices with leading id per line.
/// Format (no legacy triangle section):
///   <pointsCount>\n
///   id x y z (pointsCount lines)\n
///   <edgesCount>\n
///   id a b (edgesCount lines, 1-based point indices)\n
///   <quadsCount>\n
///   id v0 v1 v2 v3 (quadsCount lines, 1-based point indices)
/// </summary>
public sealed class IndexedMesh
{
    private static readonly char[] SplitSep = new[] { ' ', '\t' };

    private readonly List<Vec3> _vertices = new();
    private readonly List<(int a, int b)> _edges = new();
    private readonly List<(int v0, int v1, int v2, int v3)> _quads = new();

    public ReadOnlyCollection<Vec3> Vertices => _vertices.AsReadOnly();
    public ReadOnlyCollection<(int a, int b)> Edges => _edges.AsReadOnly();
    public ReadOnlyCollection<(int v0, int v1, int v2, int v3)> Quads => _quads.AsReadOnly();

    /// <summary>
    /// Builds adjacency information for this indexed mesh.
    /// </summary>
    public MeshAdjacency BuildAdjacency() => MeshAdjacency.Build(this);

    public static IndexedMesh FromMesh(Mesh mesh, double epsilon = 1e-9)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        var im = new IndexedMesh();
        bool exact = !(epsilon > 0) || epsilon < 1e-12;
        var edgeSet = new HashSet<(int,int)>();

        if (exact)
        {
            var indexOf = new Dictionary<(double x,double y,double z), int>();
            int IndexFor(Vec3 v)
            {
                var key = (v.X, v.Y, v.Z);
                if (indexOf.TryGetValue(key, out var idx)) return idx;
                idx = im._vertices.Count;
                im._vertices.Add(v);
                indexOf[key] = idx;
                return idx;
            }
            foreach (var q in mesh.Quads)
            {
                int i0 = IndexFor(q.V0); int i1 = IndexFor(q.V1); int i2 = IndexFor(q.V2); int i3 = IndexFor(q.V3);
                im._quads.Add((i0, i1, i2, i3));
                AddEdge(i0, i1); AddEdge(i1, i2); AddEdge(i2, i3); AddEdge(i3, i0);
            }
            foreach (var p in mesh.Points) _ = IndexFor(p);
            foreach (var s in mesh.InternalSegments)
            { int ia = IndexFor(s.A); int ib = IndexFor(s.B); AddEdge(ia, ib); }
        }
        else
        {
            var indexOf = new Dictionary<(long,long,long), int>();
            int IndexFor(Vec3 v)
            {
                long qx = (long)Math.Round(v.X / epsilon);
                long qy = (long)Math.Round(v.Y / epsilon);
                long qz = (long)Math.Round(v.Z / epsilon);
                var key = (qx,qy,qz);
                if (indexOf.TryGetValue(key, out var idx)) return idx;
                idx = im._vertices.Count;
                im._vertices.Add(v);
                indexOf[key] = idx;
                return idx;
            }
            foreach (var q in mesh.Quads)
            {
                int i0 = IndexFor(q.V0); int i1 = IndexFor(q.V1); int i2 = IndexFor(q.V2); int i3 = IndexFor(q.V3);
                im._quads.Add((i0, i1, i2, i3));
                AddEdge(i0, i1); AddEdge(i1, i2); AddEdge(i2, i3); AddEdge(i3, i0);
            }
            foreach (var p in mesh.Points) _ = IndexFor(p);
            foreach (var s in mesh.InternalSegments)
            { int ia = IndexFor(s.A); int ib = IndexFor(s.B); AddEdge(ia, ib); }
        }
        return im;

        void AddEdge(int ia, int ib)
        {
            if (ia == ib) return;
            var e = ia < ib ? (ia, ib) : (ib, ia);
            if (edgeSet.Add(e)) im._edges.Add(e);
        }
    }

    public static IndexedMesh ReadCustomTxt(string path)
    {
        using var sr = new StreamReader(path);
        var culture = CultureInfo.InvariantCulture;
        int pointCount = int.Parse(ReadNonEmptyLine(sr)!, culture);
        var im = new IndexedMesh();
        for (int i = 0; i < pointCount; i++)
        {
            var parts = ReadNonEmptyLine(sr)!.Split(SplitSep, StringSplitOptions.RemoveEmptyEntries);
            double x = double.Parse(parts[1], culture);
            double y = double.Parse(parts[2], culture);
            double z = double.Parse(parts[3], culture);
            im._vertices.Add(new Vec3(x, y, z));
        }
        int edgeCount = int.Parse(ReadNonEmptyLine(sr)!, culture);
        for (int i = 0; i < edgeCount; i++)
        {
            var parts = ReadNonEmptyLine(sr)!.Split(SplitSep, StringSplitOptions.RemoveEmptyEntries);
            int a = int.Parse(parts[1], culture) - 1;
            int b = int.Parse(parts[2], culture) - 1;
            im._edges.Add((a, b));
        }
        int quadCount = int.Parse(ReadNonEmptyLine(sr)!, culture);
        for (int i = 0; i < quadCount; i++)
        {
            var parts = ReadNonEmptyLine(sr)!.Split(SplitSep, StringSplitOptions.RemoveEmptyEntries);
            int v0 = int.Parse(parts[1], culture) - 1;
            int v1 = int.Parse(parts[2], culture) - 1;
            int v2 = int.Parse(parts[3], culture) - 1;
            int v3 = int.Parse(parts[4], culture) - 1;
            im._quads.Add((v0, v1, v2, v3));
        }
        return im;

        static string? ReadNonEmptyLine(StreamReader sr)
        {
            string? l; do { l = sr.ReadLine(); } while (l != null && l.Trim().Length == 0); return l;
        }
    }

    public void WriteCustomTxt(string path)
    {
        using var sw = new StreamWriter(path);
        var culture = CultureInfo.InvariantCulture;
        sw.WriteLine(_vertices.Count.ToString(culture));
        for (int i = 0; i < _vertices.Count; i++)
        {
            var v = _vertices[i];
            sw.WriteLine(string.Format(culture, "{0} {1} {2} {3}", i + 1, v.X, v.Y, v.Z));
        }
        sw.WriteLine(_edges.Count.ToString(culture));
        for (int i = 0; i < _edges.Count; i++)
        {
            var e = _edges[i];
            sw.WriteLine(string.Format(culture, "{0} {1} {2}", i + 1, e.a + 1, e.b + 1));
        }
        sw.WriteLine(_quads.Count.ToString(culture));
        for (int i = 0; i < _quads.Count; i++)
        {
            var q = _quads[i];
            sw.WriteLine(string.Format(culture, "{0} {1} {2} {3} {4}", i + 1, q.v0 + 1, q.v1 + 1, q.v2 + 1, q.v3 + 1));
        }
    }
}
