using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests.Helpers {
    /// <summary>
    /// Helper utilities for property-based testing of mesh geometry.
    /// Contains static methods for geometric calculations and mesh analysis.
    /// </summary>
    public static class PropertyBasedTestHelper {
        /// <summary>
        /// Determines if a quad is a cap quad (all vertices have the same Z coordinate).
        /// Cap quads are horizontal faces at the top or bottom of prism structures.
        /// </summary>
        /// <param name="quad">The quad to analyze.</param>
        /// <returns>True if the quad is a cap quad; otherwise, false.</returns>
        public static bool IsCapQuad(Quad quad) {
            const double epsilon = 1e-12;
            return Math.Abs(quad.V0.Z - quad.V1.Z) < epsilon &&
                   Math.Abs(quad.V1.Z - quad.V2.Z) < epsilon &&
                   Math.Abs(quad.V2.Z - quad.V3.Z) < epsilon;
        }

        /// <summary>
        /// Calculates the length (magnitude) of a 3D vector.
        /// Used for measuring edge lengths in mesh validation.
        /// </summary>
        /// <param name="vector">The vector to measure.</param>
        /// <returns>The length of the vector.</returns>
        public static double CalculateLength(Vec3 vector) {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        /// <summary>
        /// Validates that all vertices in a collection are within specified bounds.
        /// Used in property-based tests to ensure mesh geometry stays within expected limits.
        /// </summary>
        /// <param name="vertices">The vertices to validate.</param>
        /// <param name="minX">Minimum X coordinate.</param>
        /// <param name="maxX">Maximum X coordinate.</param>
        /// <param name="minY">Minimum Y coordinate.</param>
        /// <param name="maxY">Maximum Y coordinate.</param>
        /// <param name="minZ">Minimum Z coordinate.</param>
        /// <param name="maxZ">Maximum Z coordinate.</param>
        /// <param name="tolerance">Tolerance for boundary checking.</param>
        /// <returns>True if all vertices are within bounds; otherwise, false.</returns>
        public static bool AreVerticesWithinBounds(
            IEnumerable<Vec3> vertices,
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ,
            double tolerance = 0.1) {
            return vertices.All(v =>
                v.X >= minX - tolerance && v.X <= maxX + tolerance &&
                v.Y >= minY - tolerance && v.Y <= maxY + tolerance &&
                v.Z >= minZ - tolerance && v.Z <= maxZ + tolerance);
        }

        /// <summary>
        /// Validates that no vertices contain NaN values.
        /// Used to ensure mesh generation produces valid geometric data.
        /// </summary>
        /// <param name="vertices">The vertices to validate.</param>
        /// <returns>True if no vertices contain NaN; otherwise, false.</returns>
        public static bool ContainsNoNaNVertices(IEnumerable<Vec3> vertices) {
            return vertices.All(v =>
                !double.IsNaN(v.X) && !double.IsNaN(v.Y) && !double.IsNaN(v.Z));
        }

        /// <summary>
        /// Validates that all triangles have valid (non-NaN) vertices.
        /// Used in property-based tests to ensure triangle geometry integrity.
        /// </summary>
        /// <param name="triangles">The triangles to validate.</param>
        /// <returns>True if all triangles have valid vertices; otherwise, false.</returns>
        public static bool AreTrianglesValid(IEnumerable<Triangle> triangles) {
            return triangles.All(t =>
                !double.IsNaN(t.V0.X) && !double.IsNaN(t.V0.Y) && !double.IsNaN(t.V0.Z) &&
                !double.IsNaN(t.V1.X) && !double.IsNaN(t.V1.Y) && !double.IsNaN(t.V1.Z) &&
                !double.IsNaN(t.V2.X) && !double.IsNaN(t.V2.Y) && !double.IsNaN(t.V2.Z));
        }

        /// <summary>
        /// Validates that quad edges respect a maximum length constraint.
        /// Used to verify that meshing parameters are properly applied to edge sizing.
        /// </summary>
        /// <param name="quads">The quads to validate.</param>
        /// <param name="maxLength">Maximum allowed edge length.</param>
        /// <param name="sampleSize">Number of quads to sample for testing.</param>
        /// <returns>True if sampled quad edges respect the constraint; otherwise, false.</returns>
        public static bool DoQuadEdgesRespectMaxLength(IEnumerable<Quad> quads, double maxLength, int sampleSize = 3) {
            var tolerance = maxLength + 0.1; // Small tolerance for numerical precision
            var sample = quads.Take(sampleSize);

            return sample.All(q => {
                var edge1 = CalculateLength(q.V1 - q.V0);
                var edge2 = CalculateLength(q.V2 - q.V1);
                var edge3 = CalculateLength(q.V3 - q.V2);
                var edge4 = CalculateLength(q.V0 - q.V3);

                // Edges should respect the maximum constraint (can be smaller, but not larger)
                return edge1 <= tolerance && edge2 <= tolerance &&
                       edge3 <= tolerance && edge4 <= tolerance;
            });
        }
    }
}
