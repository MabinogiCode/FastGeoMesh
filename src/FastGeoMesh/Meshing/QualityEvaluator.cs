using FastGeoMesh.Geometry;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Public utility for quad quality evaluation and external filtering.
    /// Provides access to the internal quality scoring algorithms for user applications.
    /// </summary>
    public static class QualityEvaluator
    {
        /// <summary>
        /// Scores a quad based on its geometric properties.
        /// Returns a quality score in range [0,1] where 1 is perfect (square) and 0 is degenerate.
        /// </summary>
        /// <param name="vertices">Four vertices of the quad in order (typically CCW).</param>
        /// <returns>Quality score between 0 and 1.</returns>
        /// <exception cref="ArgumentException">Thrown if span length is not exactly 4 vertices.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ScoreQuad(ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.Length != 4)
            {
                throw new ArgumentException("Quad must have exactly 4 vertices", nameof(vertices));
            }

            var quad = (vertices[0], vertices[1], vertices[2], vertices[3]);
            return QuadQualityHelper.ScoreQuad(quad);
        }

        /// <summary>
        /// Scores a quad based on its geometric properties.
        /// Returns a quality score in range [0,1] where 1 is perfect (square) and 0 is degenerate.
        /// </summary>
        /// <param name="v0">First vertex of the quad.</param>
        /// <param name="v1">Second vertex of the quad.</param>
        /// <param name="v2">Third vertex of the quad.</param>
        /// <param name="v3">Fourth vertex of the quad.</param>
        /// <returns>Quality score between 0 and 1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ScoreQuad(Vec2 v0, Vec2 v1, Vec2 v2, Vec2 v3)
        {
            return QuadQualityHelper.ScoreQuad((v0, v1, v2, v3));
        }

        /// <summary>
        /// Scores a 3D quad by projecting to 2D and evaluating quality.
        /// Uses XY projection, assuming quads are primarily planar in the XY plane.
        /// </summary>
        /// <param name="vertices">Four 3D vertices of the quad in order.</param>
        /// <returns>Quality score between 0 and 1.</returns>
        /// <exception cref="ArgumentException">Thrown if span length is not exactly 4 vertices.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ScoreQuad3D(ReadOnlySpan<Vec3> vertices)
        {
            if (vertices.Length != 4)
            {
                throw new ArgumentException("Quad must have exactly 4 vertices", nameof(vertices));
            }

            // Project to XY plane for quality evaluation
            var quad2D = (
                new Vec2(vertices[0].X, vertices[0].Y),
                new Vec2(vertices[1].X, vertices[1].Y),
                new Vec2(vertices[2].X, vertices[2].Y),
                new Vec2(vertices[3].X, vertices[3].Y)
            );

            return QuadQualityHelper.ScoreQuad(quad2D);
        }

        /// <summary>
        /// Scores a 3D quad by projecting to 2D and evaluating quality.
        /// Uses XY projection, assuming quads are primarily planar in the XY plane.
        /// </summary>
        /// <param name="v0">First vertex of the 3D quad.</param>
        /// <param name="v1">Second vertex of the 3D quad.</param>
        /// <param name="v2">Third vertex of the 3D quad.</param>
        /// <param name="v3">Fourth vertex of the 3D quad.</param>
        /// <returns>Quality score between 0 and 1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ScoreQuad3D(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 v3)
        {
            var quad2D = (
                new Vec2(v0.X, v0.Y),
                new Vec2(v1.X, v1.Y),
                new Vec2(v2.X, v2.Y),
                new Vec2(v3.X, v3.Y)
            );

            return QuadQualityHelper.ScoreQuad(quad2D);
        }

        /// <summary>
        /// Determines if a quad meets the specified quality threshold.
        /// This is a convenience method that combines scoring with threshold testing.
        /// </summary>
        /// <param name="vertices">Four vertices of the quad in order.</param>
        /// <param name="qualityThreshold">Minimum quality threshold (0-1).</param>
        /// <returns>True if the quad quality meets or exceeds the threshold.</returns>
        /// <exception cref="ArgumentException">Thrown if span length is not exactly 4 vertices.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if threshold is not between 0 and 1.</exception>
        public static bool MeetsQualityThreshold(ReadOnlySpan<Vec2> vertices, double qualityThreshold)
        {
            if (qualityThreshold < 0.0 || qualityThreshold > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(qualityThreshold), qualityThreshold, "Quality threshold must be between 0 and 1");
            }

            return ScoreQuad(vertices) >= qualityThreshold;
        }

        /// <summary>
        /// Evaluates quad quality and provides detailed quality metrics.
        /// </summary>
        /// <param name="vertices">Four vertices of the quad in order.</param>
        /// <returns>Detailed quality metrics for the quad.</returns>
        /// <exception cref="ArgumentException">Thrown if span length is not exactly 4 vertices.</exception>
        public static QuadQualityMetrics EvaluateDetailedQuality(ReadOnlySpan<Vec2> vertices)
        {
            if (vertices.Length != 4)
            {
                throw new ArgumentException("Quad must have exactly 4 vertices", nameof(vertices));
            }

            var quad = (vertices[0], vertices[1], vertices[2], vertices[3]);

            // Calculate edge lengths
            double edge0 = (quad.Item2 - quad.Item1).Length();
            double edge1 = (quad.Item3 - quad.Item2).Length();
            double edge2 = (quad.Item4 - quad.Item3).Length();
            double edge3 = (quad.Item1 - quad.Item4).Length();

            double minEdge = Math.Min(Math.Min(edge0, edge1), Math.Min(edge2, edge3));
            double maxEdge = Math.Max(Math.Max(edge0, edge1), Math.Max(edge2, edge3));

            // Calculate area using shoelace formula
            double area = Math.Abs(
                (quad.Item1.X * (quad.Item2.Y - quad.Item4.Y) +
                 quad.Item2.X * (quad.Item3.Y - quad.Item1.Y) +
                 quad.Item3.X * (quad.Item4.Y - quad.Item2.Y) +
                 quad.Item4.X * (quad.Item1.Y - quad.Item3.Y)) * 0.5
            );

            // Calculate diagonal lengths
            double diag0 = (quad.Item3 - quad.Item1).Length();
            double diag1 = (quad.Item4 - quad.Item2).Length();

            double aspectRatio = maxEdge > 0 ? minEdge / maxEdge : 0.0;
            double diagonalRatio = Math.Min(diag0, diag1) / Math.Max(diag0, diag1);
            double overallScore = QuadQualityHelper.ScoreQuad(quad);

            return new QuadQualityMetrics(
                overallScore,
                aspectRatio,
                diagonalRatio,
                area,
                minEdge,
                maxEdge,
                (diag0 + diag1) * 0.5
            );
        }
    }
}
