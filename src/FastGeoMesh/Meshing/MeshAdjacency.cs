using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FastGeoMesh.Meshing;

/// <summary>Adjacency (neighbors, boundary edges, non-manifold edges) for an indexed quad mesh.</summary>
public sealed class MeshAdjacency
{
    /// <summary>Number of quads.</summary>
    public int QuadCount { get; }
    /// <summary>Per-quad neighbor indices (4 entries per quad, -1 if boundary).</summary>
    public IReadOnlyList<int[]> Neighbors => _neighbors; private readonly int[][] _neighbors;
    /// <summary>Boundary edges (a&lt;b).</summary>
    public ReadOnlyCollection<(int a, int b)> BoundaryEdges => _boundaryEdges.AsReadOnly(); private readonly List<(int a, int b)> _boundaryEdges = new();
    /// <summary>Edges shared by &gt;2 quads.</summary>
    public ReadOnlyCollection<(int a, int b)> NonManifoldEdges => _nonManifoldEdges.AsReadOnly(); private readonly List<(int a, int b)> _nonManifoldEdges = new();

    private MeshAdjacency(int quadCount){ QuadCount = quadCount; _neighbors = new int[quadCount][]; for (int q = 0; q < quadCount; q++) _neighbors[q] = new[] { -1, -1, -1, -1 }; }

    /// <summary>Compute adjacency for an indexed mesh.</summary>
    public static MeshAdjacency Build(IndexedMesh im)
    {
        ArgumentNullException.ThrowIfNull(im);
        var adj = new MeshAdjacency(im.Quads.Count);
        var edgeIncidence = new Dictionary<(int,int), List<(int q, int e)>>();
        for (int qi = 0; qi < im.Quads.Count; qi++){ var q = im.Quads[qi]; Add(q.v0, q.v1, qi, 0); Add(q.v1, q.v2, qi, 1); Add(q.v2, q.v3, qi, 2); Add(q.v3, q.v0, qi, 3); }
        foreach (var kvp in edgeIncidence){ var list = kvp.Value; if (list.Count == 1) adj._boundaryEdges.Add(kvp.Key); else if (list.Count == 2){ var (qA,eA)=list[0]; var (qB,eB)=list[1]; adj._neighbors[qA][eA]=qB; adj._neighbors[qB][eB]=qA; } else if (list.Count > 2) adj._nonManifoldEdges.Add(kvp.Key); }
        return adj;
        void Add(int i,int j,int quad,int edge){ var key = i < j ? (i,j) : (j,i); if (!edgeIncidence.TryGetValue(key, out var list)){ list = new List<(int q,int e)>(2); edgeIncidence[key]=list; } list.Add((quad,edge)); }
    }
}
