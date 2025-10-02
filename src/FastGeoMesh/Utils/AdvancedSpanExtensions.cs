using FastGeoMesh.Geometry;

namespace FastGeoMesh.Utils
{
    /// <summary>
    /// Advanced span-based extensions for high-performance geometric operations.
    /// Provides zero-allocation APIs for bulk processing in hot paths.
    /// </summary>
    public static class AdvancedSpanExtensions
    {
        /// <summary>
        /// Computes area of polygons from vertex spans without allocations.
        /// </summary>
        /// <param name="vertices">Polygon vertices in CCW order.</param>
        /// <returns>Signed area (positive for CCW, negative for CW).</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double ComputeSignedArea(this ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.Length < 3)
            {
                return 0.0;
            }

            double area = 0.0;
            int n = vertices.Length;

            // Use unrolled loop for better performance
            int i = 0;
            for (; i <= n - 2; i += 2)
            {
                var curr = vertices[i];
                var next = vertices[(i + 1) % n];
                var next2 = vertices[(i + 2) % n];

                area += curr.X * next.Y - next.X * curr.Y;
                area += next.X * next2.Y - next2.X * next.Y;
            }

            // Handle remaining vertex if odd count
            if (i < n)
            {
                var curr = vertices[i];
                var next = vertices[(i + 1) % n];
                area += curr.X * next.Y - next.X * curr.Y;
            }

            return area * 0.5;
        }

        /// <summary>
        /// Fast point-in-polygon test using span-based ray casting.
        /// Optimized for repeated tests with the same polygon.
        /// </summary>
        /// <param name="polygon">Polygon vertices.</param>
        /// <param name="point">Test point.</param>
        /// <returns>True if point is inside polygon.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool ContainsPoint(this ReadOnlySpan<Vec2> polygon, Vec2 point)
        {
            if (polygon.Length < 3)
            {
                return false;
            }

            bool inside = false;
            int n = polygon.Length;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var vi = polygon[i];
                var vj = polygon[j];

                if (((vi.Y > point.Y) != (vj.Y > point.Y)) &&
                    (point.X < (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y) + vi.X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Batch point-in-polygon tests for multiple points against the same polygon.
        /// Highly optimized for scenarios like mesh refinement near holes.
        /// </summary>
        /// <param name="polygon">Polygon vertices.</param>
        /// <param name="points">Points to test.</param>
        /// <param name="results">Results array (true = inside).</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ContainsPoints(this ReadOnlySpan<Vec2> polygon, ReadOnlySpan<Vec2> points, Span<bool> results)
        {
            if (results.Length < points.Length)
            {
                throw new ArgumentException("Results span too small", nameof(results));
            }

            if (polygon.Length < 3)
            {
                results.Slice(0, points.Length).Clear();
                return;
            }

            // Process points in batches for better cache locality
            for (int i = 0; i < points.Length; i++)
            {
                results[i] = polygon.ContainsPoint(points[i]);
            }
        }

        /// <summary>
        /// Computes minimum bounding rectangle with optional padding.
        /// </summary>
        /// <param name="vertices">Vertices to bound.</param>
        /// <param name="padding">Optional padding around bounds.</param>
        /// <returns>Min and max points of padded bounding box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static (Vec2 min, Vec2 max) ComputePaddedBounds(this ReadOnlySpan<Vec2> vertices, double padding = 0.0)
        {
            if (vertices.IsEmpty)
            {
                return (Vec2.Zero, Vec2.Zero);
            }

            var first = vertices[0];
            double minX = first.X, maxX = first.X;
            double minY = first.Y, maxY = first.Y;

            // Safe bounds checking for unrolled loop
            int i = 1;
            int safeEnd = vertices.Length >= 4 ? vertices.Length - 3 : vertices.Length;

            for (; i < safeEnd; i += 4)
            {
                var v1 = vertices[i];
                if (v1.X < minX)
                {
                    minX = v1.X;
                }

                if (v1.X > maxX)
                {
                    maxX = v1.X;
                }

                if (v1.Y < minY)
                {
                    minY = v1.Y;
                }

                if (v1.Y > maxY)
                {
                    maxY = v1.Y;
                }

                if (i + 1 < vertices.Length)
                {
                    var v2 = vertices[i + 1];
                    if (v2.X < minX)
                    {
                        minX = v2.X;
                    }

                    if (v2.X > maxX)
                    {
                        maxX = v2.X;
                    }

                    if (v2.Y < minY)
                    {
                        minY = v2.Y;
                    }

                    if (v2.Y > maxY)
                    {
                        maxY = v2.Y;
                    }
                }

                if (i + 2 < vertices.Length)
                {
                    var v3 = vertices[i + 2];
                    if (v3.X < minX)
                    {
                        minX = v3.X;
                    }

                    if (v3.X > maxX)
                    {
                        maxX = v3.X;
                    }

                    if (v3.Y < minY)
                    {
                        minY = v3.Y;
                    }

                    if (v3.Y > maxY)
                    {
                        maxY = v3.Y;
                    }
                }

                if (i + 3 < vertices.Length)
                {
                    var v4 = vertices[i + 3];
                    if (v4.X < minX)
                    {
                        minX = v4.X;
                    }

                    if (v4.X > maxX)
                    {
                        maxX = v4.X;
                    }

                    if (v4.Y < minY)
                    {
                        minY = v4.Y;
                    }

                    if (v4.Y > maxY)
                    {
                        maxY = v4.Y;
                    }
                }
            }

            // Handle remaining vertices
            for (; i < vertices.Length; i++)
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

            return (new Vec2(minX - padding, minY - padding),
                    new Vec2(maxX + padding, maxY + padding));
        }

        /// <summary>
        /// Fast distance computation between point and line segment.
        /// Optimized for mesh quality calculations.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <param name="segmentStart">Segment start.</param>
        /// <param name="segmentEnd">Segment end.</param>
        /// <returns>Minimum distance from point to segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static double DistanceToSegment(Vec2 point, Vec2 segmentStart, Vec2 segmentEnd)
        {
            var segment = segmentEnd - segmentStart;
            var segmentLengthSq = segment.LengthSquared();

            if (segmentLengthSq == 0.0)
            {
                return (point - segmentStart).Length();
            }

            var t = Math.Max(0.0, Math.Min(1.0, (point - segmentStart).Dot(segment) / segmentLengthSq));
            var projection = segmentStart + t * segment;

            return (point - projection).Length();
        }
    }
}
