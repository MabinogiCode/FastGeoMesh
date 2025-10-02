namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Detailed quality metrics for a quad.
    /// </summary>
    /// <param name="OverallScore">Overall quality score (0-1).</param>
    /// <param name="AspectRatio">Ratio of minimum to maximum edge length (0-1).</param>
    /// <param name="DiagonalRatio">Ratio of shorter to longer diagonal (0-1).</param>
    /// <param name="Area">Area of the quad.</param>
    /// <param name="MinEdgeLength">Length of the shortest edge.</param>
    /// <param name="MaxEdgeLength">Length of the longest edge.</param>
    /// <param name="AverageDiagonalLength">Average length of the two diagonals.</param>
    public readonly record struct QuadQualityMetrics(
        double OverallScore,
        double AspectRatio,
        double DiagonalRatio,
        double Area,
        double MinEdgeLength,
        double MaxEdgeLength,
        double AverageDiagonalLength
    );
}
