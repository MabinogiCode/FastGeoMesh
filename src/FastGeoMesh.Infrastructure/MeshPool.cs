using System.Runtime.CompilerServices;
using FastGeoMesh.Domain;
using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Utils
{
    /// <summary>
    /// Advanced object pooling for Mesh instances to reduce GC pressure.
    /// Provides high-performance mesh reuse for frequent meshing operations.
    /// </summary>
    public static class MeshPool
    {
        private static readonly DefaultObjectPoolProvider _provider = new();

        /// <summary>
        /// Pool for Mesh instances optimized for typical meshing workloads.
        /// Pre-sized for common scenarios to minimize reallocations.
        /// </summary>
        public static readonly ObjectPool<Mesh> Instance =
            _provider.Create(new MeshPoolPolicy());

        /// <summary>Get a mesh instance from the pool.</summary>
        /// <returns>Clean mesh ready for use.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mesh Get() => Instance.Get();

        /// <summary>Return a mesh instance to the pool.</summary>
        /// <param name="mesh">Mesh to return (will be cleared).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(Mesh mesh) => Instance.Return(mesh);
    }

    /// <summary>
    /// Pooling policy for Mesh instances with optimized capacity settings.
    /// </summary>
    internal sealed class MeshPoolPolicy : PooledObjectPolicy<Mesh>
    {
        // Optimized initial capacities based on typical workloads
        private const int DefaultQuadCapacity = 1024;    // Most meshes have < 1K quads
        private const int DefaultTriangleCapacity = 512; // Fewer triangles typically
        private const int DefaultPointCapacity = 64;     // Limited auxiliary points
        private const int DefaultSegmentCapacity = 32;   // Few internal segments

        // Maximum retained capacity to prevent memory bloat
        private const int MaxRetainedQuads = 8192;

        public override Mesh Create()
        {
            return new Mesh();
        }

        public override bool Return(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            // Clear all data to ensure clean state
            mesh.Clear();

            // Only retain meshes that haven't grown too large
            // This prevents memory bloat from exceptional cases
            return mesh.QuadCount == 0 &&
                   mesh.TriangleCount == 0 &&
                   mesh.Points.Count == 0 &&
                   mesh.InternalSegments.Count == 0;
        }
    }

    /// <summary>
    /// High-performance extension methods for pooled mesh operations.
    /// </summary>
    public static class PooledMeshExtensions
    {
        /// <summary>
        /// Execute an operation with a pooled mesh, automatically returning it when done.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute with the mesh.</param>
        /// <returns>Operation result.</returns>
        public static T WithPooledMesh<T>(Func<Mesh, T> operation)
        {
            var mesh = MeshPool.Get();
            try
            {
                return operation(mesh);
            }
            finally
            {
                MeshPool.Return(mesh);
            }
        }

        /// <summary>
        /// Execute an operation with a pooled mesh, automatically returning it when done.
        /// </summary>
        /// <param name="operation">Operation to execute with the mesh.</param>
        public static void WithPooledMesh(Action<Mesh> operation)
        {
            var mesh = MeshPool.Get();
            try
            {
                operation(mesh);
            }
            finally
            {
                MeshPool.Return(mesh);
            }
        }
    }
}
