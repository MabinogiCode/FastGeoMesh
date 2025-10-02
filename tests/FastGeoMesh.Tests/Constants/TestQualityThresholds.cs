namespace FastGeoMesh.Tests;

/// <summary>
/// Quality thresholds for quad scoring tests.
/// </summary>
public static class TestQualityThresholds
{
    /// <summary>High quality threshold for quad scoring (0.8).</summary>
    public const double HighQualityThreshold = 0.8;

    /// <summary>Medium quality threshold for quad scoring (0.6).</summary>
    public const double MediumQualityThreshold = 0.6;

    /// <summary>Low quality threshold for quad scoring (0.3).</summary>
    public const double LowQualityThreshold = 0.3;

    /// <summary>Minimum quality expected for perfect squares (0.8).</summary>
    public const double PerfectSquareMinQuality = 0.8;
}
