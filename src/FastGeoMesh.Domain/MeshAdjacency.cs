using System.Collections.ObjectModel;

namespace FastGeoMesh.Domain
{
    /// <summary>Adjacency data for a quad mesh: per-edge neighbors, boundary edges and non-manifold edges.</summary>
    public sealed class MeshAdjacency
    {
        /// <summary>Total quad count.</summary>
        public int QuadCount { get; }
        /// <summary>Neighbor quad indices for each quad (array of length 4, -1 if no neighbor).</summary>
        public IReadOnlyList<int[]> Neighbors => _neighbors; private readonly int[][] _neighbors;
        /// <summary>Boundary edges (edges incident to a single quad).</summary>
        public ReadOnlyCollection<(int a, int b)> BoundaryEdges => _boundaryEdges.AsReadOnly(); private readonly List<(int a, int b)> _boundaryEdges = new();
        /// <summary>Edges incident to more than two quads.</summary>
        public ReadOnlyCollection<(int a, int b)> NonManifoldEdges => _nonManifoldEdges.AsReadOnly(); private readonly List<(int a, int b)> _nonManifoldEdges = new();
        private MeshAdjacency(int quadCount)
        {
            QuadCount = quadCount;
            _neighbors = new int[quadCount][];
            for (int q = 0; q < quadCount; q++)
            {
                _neighbors[q] = new[] { -1, -1, -1, -1 };
            }
        }
        /// <summary>Build adjacency information for an indexed mesh.</summary>
        public static MeshAdjacency Build(IndexedMesh im)
        {
            ArgumentNullException.ThrowIfNull(im);
            var adj = new MeshAdjacency(im.Quads.Count);
            var edgeIncidence = new Dictionary<(int, int), List<(int q, int e)>>();
            for (int qi = 0; qi < im.Quads.Count; qi++)
            {
                var q = im.Quads[qi];
                Add(q.Item1, q.Item2, qi, 0);
                Add(q.Item2, q.Item3, qi, 1);
                Add(q.Item3, q.Item4, qi, 2);
                Add(q.Item4, q.Item1, qi, 3);
            }
            foreach (var kvp in edgeIncidence)
            {
                var list = kvp.Value;
                if (list.Count == 1)
                {
                    adj._boundaryEdges.Add(kvp.Key);
                }
                else if (list.Count == 2)
                {
                    var (qA, eA) = list[0];
                    var (qB, eB) = list[1];
                    adj._neighbors[qA][eA] = qB;
                    adj._neighbors[qB][eB] = qA;
                }
                else if (list.Count > 2)
                {
                    adj._nonManifoldEdges.Add(kvp.Key);
                }
            }
            return adj;
            void Add(int i, int j, int quad, int edge)
            {
                var key = i < j ? (i, j) : (j, i);
                if (!edgeIncidence.TryGetValue(key, out var list))
                {
                    list = new List<(int q, int e)>(2);
                    edgeIncidence[key] = list;
                }
                list.Add((quad, edge));
            }
        }
    }
}
