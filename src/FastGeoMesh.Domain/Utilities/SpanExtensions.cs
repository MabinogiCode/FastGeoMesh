using System.Runtime.CompilerServices;

namespace FastGeoMesh.Domain {
    /// <summary>
    /// Extension helpers for span-based geometric operations (domain-level).
    /// </summary>
    public static class SpanExtensions {
        /// <summary>Compute centroid of 2D vertices.</summary>
        /// <returns>Centroid of the provided 2D vertices.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 ComputeCentroid(this ReadOnlySpan<Vec2> vertices) {
            if (vertices.IsEmpty) {
                return Vec2.Zero;
            }

            double sx = 0.0;
            double sy = 0.0;
            for (int i = 0; i < vertices.Length; i++) {
                sx += vertices[i].X;
                sy += vertices[i].Y;
            }

            double inv = 1.0 / vertices.Length;
            return new Vec2(sx * inv, sy * inv);
        }

        /// <summary>Compute centroid of 3D vertices.</summary>
        /// <returns>Centroid of the provided 3D vertices.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 ComputeCentroid(this ReadOnlySpan<Vec3> vertices) {
            if (vertices.IsEmpty) {
                return Vec3.Zero;
            }

            double sx = 0.0;
            double sy = 0.0;
            double sz = 0.0;
            for (int i = 0; i < vertices.Length; i++) {
                sx += vertices[i].X;
                sy += vertices[i].Y;
                sz += vertices[i].Z;
            }

            double inv = 1.0 / vertices.Length;
            return new Vec3(sx * inv, sy * inv, sz * inv);
        }

        /// <summary>Compute signed area of polygon vertices.</summary>
        /// <returns>Signed area (positive if CCW) of the polygon vertices.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ComputeSignedArea(this ReadOnlySpan<Vec2> vertices) => GeometryHelper.SignedArea(vertices);

        /// <summary>Compute axis-aligned bounds of vertices.</summary>
        /// <returns>Tuple containing the minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vec2 min, Vec2 max) ComputeBounds(this ReadOnlySpan<Vec2> vertices) {
            if (vertices.IsEmpty) {
                return (Vec2.Zero, Vec2.Zero);
            }

            var first = vertices[0];
            double minX = first.X;
            double maxX = first.X;
            double minY = first.Y;
            double maxY = first.Y;

            for (int i = 1; i < vertices.Length; i++) {
                var v = vertices[i];
                if (v.X < minX) {
                    minX = v.X;
                }
                if (v.X > maxX) {
                    maxX = v.X;
                }
                if (v.Y < minY) {
                    minY = v.Y;
                }
                if (v.Y > maxY) {
                    maxY = v.Y;
                }
            }

            return (new Vec2(minX, minY), new Vec2(maxX, maxY));
        }

        /// <summary>Check whether the polygon contains the given point.</summary>
        /// <returns>True if polygon contains the point or it's on the boundary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsPoint(this ReadOnlySpan<Vec2> polygon, Vec2 point) => GeometryHelper.ContainsPoint(polygon, point);

        /// <summary>Batch contains points against polygon.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ContainsPoints(this ReadOnlySpan<Vec2> polygon, ReadOnlySpan<Vec2> points, Span<bool> results) => GeometryHelper.ContainsPoints(polygon, points, results);

        /// <summary>Transform 2D vertices to 3D at given Z coordinate.</summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void TransformTo3D(this ReadOnlySpan<Vec2> source, Span<Vec3> destination, double z) {
            if (source.Length > destination.Length) {
                throw new ArgumentException("Destination span too small", nameof(destination));
            }

            for (int i = 0; i < source.Length; i++) {
                destination[i] = new Vec3(source[i].X, source[i].Y, z);
            }
        }

        /// <summary>Compute padded bounds (min,max) with optional padding; optimized for common sizes.</summary>
        /// <returns>Tuple of padded (min,max) corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static (Vec2 min, Vec2 max) ComputePaddedBounds(this ReadOnlySpan<Vec2> vertices, double padding = 0.0) {
            if (vertices.IsEmpty) {
                return (Vec2.Zero, Vec2.Zero);
            }

            var first = vertices[0];
            double minX = first.X;
            double maxX = first.X;
            double minY = first.Y;
            double maxY = first.Y;

            int i = 1;
            int safeEnd = vertices.Length >= 4 ? vertices.Length - 3 : vertices.Length;

            for (; i < safeEnd; i += 4) {
                var v1 = vertices[i];
                if (v1.X < minX) {
                    minX = v1.X;
                }
                if (v1.X > maxX) {
                    maxX = v1.X;
                }
                if (v1.Y < minY) {
                    minY = v1.Y;
                }
                if (v1.Y > maxY) {
                    maxY = v1.Y;
                }

                if (i + 1 < vertices.Length) {
                    var v2 = vertices[i + 1];
                    if (v2.X < minX) {
                        minX = v2.X;
                    }
                    if (v2.X > maxX) {
                        maxX = v2.X;
                    }
                    if (v2.Y < minY) {
                        minY = v2.Y;
                    }
                    if (v2.Y > maxY) {
                        maxY = v2.Y;
                    }
                }

                if (i + 2 < vertices.Length) {
                    var v3 = vertices[i + 2];
                    if (v3.X < minX) {
                        minX = v3.X;
                    }
                    if (v3.X > maxX) {
                        maxX = v3.X;
                    }
                    if (v3.Y < minY) {
                        minY = v3.Y;
                    }
                    if (v3.Y > maxY) {
                        maxY = v3.Y;
                    }
                }

                if (i + 3 < vertices.Length) {
                    var v4 = vertices[i + 3];
                    if (v4.X < minX) {
                        minX = v4.X;
                    }
                    if (v4.X > maxX) {
                        maxX = v4.X;
                    }
                    if (v4.Y < minY) {
                        minY = v4.Y;
                    }
                    if (v4.Y > maxY) {
                        maxY = v4.Y;
                    }
                }
            }

            for (; i < vertices.Length; i++) {
                var v = vertices[i];
                if (v.X < minX) {
                    minX = v.X;
                }
                if (v.X > maxX) {
                    maxX = v.X;
                }
                if (v.Y < minY) {
                    minY = v.Y;
                }
                if (v.Y > maxY) {
                    maxY = v.Y;
                }
            }

            return (new Vec2(minX - padding, minY - padding), new Vec2(maxX + padding, maxY + padding));
        }
    }
}
