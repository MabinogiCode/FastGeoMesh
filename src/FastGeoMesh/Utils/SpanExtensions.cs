using System.Runtime.CompilerServices;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Utils
{
    /// <summary>
    /// High-performance span-based extensions for geometric operations.
    /// Provides zero-allocation APIs for hot paths in meshing operations.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Computes the centroid of vertices without allocations.
        /// </summary>
        /// <param name="vertices">Span of vertices to process.</param>
        /// <returns>Centroid position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 ComputeCentroid(this ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.IsEmpty)
            {
                return Vec2.Zero;
            }

            double sumX = 0, sumY = 0;
            foreach (var vertex in vertices)
            {
                sumX += vertex.X;
                sumY += vertex.Y;
            }

            double count = vertices.Length;
            return new Vec2(sumX / count, sumY / count);
        }

        /// <summary>
        /// Computes the centroid of 3D vertices without allocations.
        /// </summary>
        /// <param name="vertices">Span of vertices to process.</param>
        /// <returns>Centroid position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 ComputeCentroid(this ReadOnlySpan<Vec3> vertices)
        {
            if (vertices.IsEmpty)
            {
                return Vec3.Zero;
            }

            double sumX = 0, sumY = 0, sumZ = 0;
            foreach (var vertex in vertices)
            {
                sumX += vertex.X;
                sumY += vertex.Y;
                sumZ += vertex.Z;
            }

            double count = vertices.Length;
            return new Vec3(sumX / count, sumY / count, sumZ / count);
        }

        /// <summary>
        /// Transforms a span of 2D vertices to 3D at specified Z level.
        /// </summary>
        /// <param name="source">Source 2D vertices.</param>
        /// <param name="destination">Destination 3D vertices span.</param>
        /// <param name="z">Z coordinate for all vertices.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void TransformTo3D(this ReadOnlySpan<Vec2> source, Span<Vec3> destination, double z)
        {
            if (source.Length > destination.Length)
            {
                throw new ArgumentException("Destination span too small", nameof(destination));
            }

            for (int i = 0; i < source.Length; i++)
            {
                var v2 = source[i];
                destination[i] = new Vec3(v2.X, v2.Y, z);
            }
        }

        /// <summary>
        /// Computes bounding box of vertices without allocations.
        /// </summary>
        /// <param name="vertices">Vertices to bound.</param>
        /// <returns>Min and max points of bounding box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static (Vec2 min, Vec2 max) ComputeBounds(this ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.IsEmpty)
            {
                return (Vec2.Zero, Vec2.Zero);
            }

            var first = vertices[0];
            double minX = first.X, maxX = first.X;
            double minY = first.Y, maxY = first.Y;

            for (int i = 1; i < vertices.Length; i++)
            {
                var v = vertices[i];
                if (v.X < minX)
                {
                    minX = v.X;
                }

                if (v.X > maxX)
                {
                    maxX = v.X;
                }

                if (v.Y < minY)
                {
                    minY = v.Y;
                }

                if (v.Y > maxY)
                {
                    maxY = v.Y;
                }
            }

            return (new Vec2(minX, minY), new Vec2(maxX, maxY));
        }
    }
}
