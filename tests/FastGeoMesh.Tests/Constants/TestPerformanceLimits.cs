namespace FastGeoMesh.Tests;

/// <summary>
/// Performance test limits and constraints.
/// </summary>
public static class TestPerformanceLimits
{
    /// <summary>Maximum number of iterations for performance tests.</summary>
    public const int MaxTestIterations = 1000;

    /// <summary>Minimum number of iterations for performance tests.</summary>
    public const int MinTestIterations = 10;

    /// <summary>Iteration limit for property-based tests.</summary>
    public const int PropertyBasedTestLimit = 20;
}
