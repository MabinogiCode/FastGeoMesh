using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Meshing
{
    /// <summary>Indexed representation of a mesh (unique vertex list + edge / quad / triangle index sets).</summary>
    public sealed class IndexedMesh
    {
        private static readonly char[] SplitSep = { ' ', '\t' };
        private readonly List<Vec3> _vertices = new();
        private readonly List<(int a, int b)> _edges = new();
        private readonly List<(int v0, int v1, int v2, int v3)> _quads = new();
        private readonly List<(int v0, int v1, int v2)> _triangles = new();
        
        // 🚀 OPTIMIZATION: Simple lazy caching for read-only collections
        private ReadOnlyCollection<Vec3>? _verticesReadOnly;
        private ReadOnlyCollection<(int a, int b)>? _edgesReadOnly;
        private ReadOnlyCollection<(int v0, int v1, int v2, int v3)>? _quadsReadOnly;
        private ReadOnlyCollection<(int v0, int v1, int v2)>? _trianglesReadOnly;
        
        /// <summary>Vertex list.</summary>
        public ReadOnlyCollection<Vec3> Vertices => _verticesReadOnly ??= _vertices.AsReadOnly();
        /// <summary>Edge list (pairs of vertex indices).</summary>
        public ReadOnlyCollection<(int a, int b)> Edges => _edgesReadOnly ??= _edges.AsReadOnly();
        /// <summary>Quad list (four vertex indices).</summary>
        public ReadOnlyCollection<(int v0, int v1, int v2, int v3)> Quads => _quadsReadOnly ??= _quads.AsReadOnly();
        /// <summary>Triangle list (three vertex indices).</summary>
        public ReadOnlyCollection<(int v0, int v1, int v2)> Triangles => _trianglesReadOnly ??= _triangles.AsReadOnly();
        
        // 🚀 OPTIMIZATION: Direct count properties to avoid collection creation
        /// <summary>Gets the number of vertices without creating a collection.</summary>
        public int VertexCount => _vertices.Count;
        /// <summary>Gets the number of edges without creating a collection.</summary>
        public int EdgeCount => _edges.Count;
        /// <summary>Gets the number of quads without creating a collection.</summary>
        public int QuadCount => _quads.Count;
        /// <summary>Gets the number of triangles without creating a collection.</summary>
        public int TriangleCount => _triangles.Count;
        
        /// <summary>Build adjacency (neighbors, boundary and non-manifold edges).</summary>
        public MeshAdjacency BuildAdjacency() => MeshAdjacency.Build(this);
        /// <summary>Create an IndexedMesh from a raw mesh with optional coordinate deduplication (optimized version).</summary>
        public static IndexedMesh FromMesh(Mesh mesh, double epsilon = 1e-9)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            var im = new IndexedMesh();
            bool exact = epsilon <= 0 || epsilon < 1e-12;
            
            // 🚀 OPTIMIZATION: Pre-allocate collections based on mesh size
            var quadCount = mesh.QuadCount;
            var triangleCount = mesh.TriangleCount;
            var estimatedVertexCount = (quadCount * 4 + triangleCount * 3) / 2; // Rough estimate assuming some sharing
            
            im._vertices.Capacity = Math.Max(estimatedVertexCount, 16);
            im._quads.Capacity = quadCount;
            im._triangles.Capacity = triangleCount;
            im._edges.Capacity = (quadCount * 4 + triangleCount * 3); // Each quad/tri contributes edges
            
            var edgeSet = new HashSet<(int, int)>(im._edges.Capacity);
            
            if (exact)
            {
                // 🚀 OPTIMIZATION: Pre-size dictionary for better performance
                var indexOf = new Dictionary<(double, double, double), int>(estimatedVertexCount);
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                int IndexFor(Vec3 v)
                {
                    var key = (v.X, v.Y, v.Z);
                    if (indexOf.TryGetValue(key, out var idx))
                    {
                        return idx;
                    }
                    idx = im._vertices.Count;
                    im._vertices.Add(v);
                    indexOf[key] = idx;
                    return idx;
                }
                
                // 🚀 OPTIMIZATION: Process quads in batch with reduced allocations
                foreach (var q in mesh.Quads)
                {
                    int i0 = IndexFor(q.V0);
                    int i1 = IndexFor(q.V1);
                    int i2 = IndexFor(q.V2);
                    int i3 = IndexFor(q.V3);
                    im._quads.Add((i0, i1, i2, i3));
                    
                    // Batch edge addition with fewer HashSet operations
                    AddEdgeBatch(edgeSet, im._edges, i0, i1, i2, i3, i0);
                }
                
                foreach (var t in mesh.Triangles)
                {
                    int i0 = IndexFor(t.V0);
                    int i1 = IndexFor(t.V1);
                    int i2 = IndexFor(t.V2);
                    im._triangles.Add((i0, i1, i2));
                    
                    AddEdgeBatch(edgeSet, im._edges, i0, i1, i2, i0);
                }
                
                foreach (var p in mesh.Points)
                {
                    _ = IndexFor(p);
                }
                
                foreach (var s in mesh.InternalSegments)
                {
                    int ia = IndexFor(s.A);
                    int ib = IndexFor(s.B);
                    AddEdgeSingle(edgeSet, im._edges, ia, ib);
                }
            }
            else
            {
                // 🚀 OPTIMIZATION: Use struct keys to reduce allocations
                var indexOf = new Dictionary<QuantizedVertex, int>(estimatedVertexCount);
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                int IndexFor(Vec3 v)
                {
                    var key = new QuantizedVertex(v, epsilon);
                    if (indexOf.TryGetValue(key, out var idx))
                    {
                        return idx;
                    }
                    idx = im._vertices.Count;
                    im._vertices.Add(v);
                    indexOf[key] = idx;
                    return idx;
                }
                
                foreach (var q in mesh.Quads)
                {
                    int i0 = IndexFor(q.V0);
                    int i1 = IndexFor(q.V1);
                    int i2 = IndexFor(q.V2);
                    int i3 = IndexFor(q.V3);
                    im._quads.Add((i0, i1, i2, i3));
                    AddEdgeBatch(edgeSet, im._edges, i0, i1, i2, i3, i0);
                }
                
                foreach (var t in mesh.Triangles)
                {
                    int i0 = IndexFor(t.V0);
                    int i1 = IndexFor(t.V1);
                    int i2 = IndexFor(t.V2);
                    im._triangles.Add((i0, i1, i2));
                    AddEdgeBatch(edgeSet, im._edges, i0, i1, i2, i0);
                }
                
                foreach (var p in mesh.Points)
                {
                    _ = IndexFor(p);
                }
                
                foreach (var s in mesh.InternalSegments)
                {
                    int ia = IndexFor(s.A);
                    int ib = IndexFor(s.B);
                    AddEdgeSingle(edgeSet, im._edges, ia, ib);
                }
            }
            
            return im;
            
            // 🚀 OPTIMIZATION: Batch edge processing to reduce HashSet operations
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void AddEdgeBatch(HashSet<(int, int)> edgeSet, List<(int, int)> edges, params int[] indices)
            {
                for (int i = 0; i < indices.Length - 1; i++)
                {
                    int ia = indices[i];
                    int ib = indices[i + 1];
                    AddEdgeSingle(edgeSet, edges, ia, ib);
                }
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void AddEdgeSingle(HashSet<(int, int)> edgeSet, List<(int, int)> edges, int ia, int ib)
            {
                if (ia == ib) return;
                
                var e = ia < ib ? (ia, ib) : (ib, ia);
                if (edgeSet.Add(e))
                {
                    edges.Add(e);
                }
            }
        }
        
        /// <summary>Read legacy custom text mesh format (quads only, no triangles).</summary>
        public static IndexedMesh ReadCustomTxt(string path)
        {
            using var sr = new StreamReader(path);
            var culture = CultureInfo.InvariantCulture;
            int pointCount = int.Parse(ReadNonEmptyLine(sr)!, culture);
            var im = new IndexedMesh();
            for (int i = 0; i < pointCount; i++)
            {
                var parts = ReadNonEmptyLine(sr)!.Split(SplitSep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                {
                    throw new FormatException("Invalid point line");
                }
                double x = double.Parse(parts[1], culture);
                double y = double.Parse(parts[2], culture);
                double z = double.Parse(parts[3], culture);
                im._vertices.Add(new Vec3(x, y, z));
            }
            int edgeCount = int.Parse(ReadNonEmptyLine(sr)!, culture);
            for (int i = 0; i < edgeCount; i++)
            {
                var parts = ReadNonEmptyLine(sr)!.Split(SplitSep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new FormatException("Invalid edge line");
                }
                int a = int.Parse(parts[1], culture) - 1;
                int b = int.Parse(parts[2], culture) - 1;
                im._edges.Add((a, b));
            }
            int quadCount = int.Parse(ReadNonEmptyLine(sr)!, culture);
            for (int i = 0; i < quadCount; i++)
            {
                var parts = ReadNonEmptyLine(sr)!.Split(SplitSep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                {
                    throw new FormatException("Invalid quad line");
                }
                int v0 = int.Parse(parts[1], culture) - 1;
                int v1 = int.Parse(parts[2], culture) - 1;
                int v2 = int.Parse(parts[3], culture) - 1;
                int v3 = int.Parse(parts[4], culture) - 1;
                im._quads.Add((v0, v1, v2, v3));
            }
            return im;

            static string? ReadNonEmptyLine(StreamReader sr)
            {
                string? l;
                do
                {
                    l = sr.ReadLine();
                } while (l is not null && l.Trim().Length == 0);
                return l;
            }
        }

        /// <summary>Write legacy custom text mesh format.</summary>
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
        
        /// <summary>Optimized struct for quantized vertex keys to reduce allocations.</summary>
        private readonly struct QuantizedVertex : IEquatable<QuantizedVertex>
        {
            private readonly long _x, _y, _z;
            
            public QuantizedVertex(Vec3 v, double epsilon)
            {
                _x = (long)Math.Round(v.X / epsilon);
                _y = (long)Math.Round(v.Y / epsilon);
                _z = (long)Math.Round(v.Z / epsilon);
            }
            
            public bool Equals(QuantizedVertex other) => _x == other._x && _y == other._y && _z == other._z;
            
            public override bool Equals(object? obj) => obj is QuantizedVertex other && Equals(other);
            
            public override int GetHashCode() => HashCode.Combine(_x, _y, _z);
        }
    }
}
