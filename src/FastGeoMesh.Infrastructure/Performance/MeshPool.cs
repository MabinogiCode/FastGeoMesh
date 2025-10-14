using System.Runtime.CompilerServices;
using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure
{
    /// <summary>
    /// Factory for creating ImmutableMesh instances.
    /// Note: Since ImmutableMesh is immutable, traditional object pooling is not applicable.
    /// This factory provides a consistent API for mesh creation.
    /// </summary>
    public static class MeshPool
    {
        /// <summary>Get a new empty mesh instance.</summary>
        /// <returns>Clean mesh ready for use.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImmutableMesh Get() => ImmutableMesh.Empty;

        /// <summary>Return operation is a no-op since ImmutableMesh is immutable.</summary>
        /// <param name="mesh">Mesh instance (ignored since immutable).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(ImmutableMesh mesh)
        {
            // No-op: Immutable objects don't need to be pooled
        }
    }

    /// <summary>
    /// High-performance extension methods for mesh operations.
    /// </summary>
    public static class PooledMeshExtensions
    {
        /// <summary>
        /// Execute an operation with a new mesh instance.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute with the mesh.</param>
        /// <returns>Operation result.</returns>
        public static T WithPooledMesh<T>(Func<ImmutableMesh, T> operation)
        {
            var mesh = MeshPool.Get();
            return operation(mesh);
        }

        /// <summary>
        /// Execute an operation with a new mesh instance.
        /// </summary>
        /// <param name="operation">Operation to execute with the mesh.</param>
        public static void WithPooledMesh(Action<ImmutableMesh> operation)
        {
            var mesh = MeshPool.Get();
            operation(mesh);
        }
    }
}
