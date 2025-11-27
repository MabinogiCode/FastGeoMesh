using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Tests for class PropertyBasedTestHelper.
    /// </summary>
    public static class PropertyBasedTestHelper
    {
        /// <summary>
        /// Runs test IsCapQuad.
        /// </summary>
        public static bool IsCapQuad(Quad quad)
        {
            const double epsilon = 1e-12;
            return Math.Abs(quad.V0.Z - quad.V1.Z) < epsilon &&
                   Math.Abs(quad.V1.Z - quad.V2.Z) < epsilon &&
                   Math.Abs(quad.V2.Z - quad.V3.Z) < epsilon;
        }
        /// <summary>
        /// Runs test CalculateLength.
        /// </summary>
        public static double CalculateLength(Vec3 vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }
        /// <summary>
        /// Runs test AreVerticesWithinBounds.
        /// </summary>
        public static bool AreVerticesWithinBounds(
            IEnumerable<Vec3> vertices,
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ,
            double tolerance = 0.1)
        {
            return vertices.All(v =>
                v.X >= minX - tolerance && v.X <= maxX + tolerance &&
                v.Y >= minY - tolerance && v.Y <= maxY + tolerance &&
                v.Z >= minZ - tolerance && v.Z <= maxZ + tolerance);
        }
        /// <summary>
        /// Runs test ContainsNoNaNVertices.
        /// </summary>
        public static bool ContainsNoNaNVertices(IEnumerable<Vec3> vertices)
        {
            return vertices.All(v =>
                !double.IsNaN(v.X) && !double.IsNaN(v.Y) && !double.IsNaN(v.Z));
        }
        /// <summary>
        /// Runs test AreTrianglesValid.
        /// </summary>
        public static bool AreTrianglesValid(IEnumerable<Triangle> triangles)
        {
            return triangles.All(t =>
                !double.IsNaN(t.V0.X) && !double.IsNaN(t.V0.Y) && !double.IsNaN(t.V0.Z) &&
                !double.IsNaN(t.V1.X) && !double.IsNaN(t.V1.Y) && !double.IsNaN(t.V1.Z) &&
                !double.IsNaN(t.V2.X) && !double.IsNaN(t.V2.Y) && !double.IsNaN(t.V2.Z));
        }
        /// <summary>
        /// Runs test DoQuadEdgesRespectMaxLength.
        /// </summary>
        public static bool DoQuadEdgesRespectMaxLength(IEnumerable<Quad> quads, double maxLength, int sampleSize = 3)
        {
            var tolerance = maxLength + 0.1; // Small tolerance for numerical precision
            var sample = quads.Take(sampleSize);

            return sample.All(q =>
            {
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
