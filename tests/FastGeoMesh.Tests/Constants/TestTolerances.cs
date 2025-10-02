namespace FastGeoMesh.Tests;

/// <summary>
/// Centralized constants for test tolerance values and numerical precision.
/// </summary>
public static class TestTolerances
{
    /// <summary>Standard epsilon for floating-point comparisons in tests.</summary>
    public const double Epsilon = 1e-9;
    
    /// <summary>Default tolerance for numerical precision tests.</summary>
    public const double DefaultTolerance = 1e-12;
    
    /// <summary>Tolerance for quad quality score comparisons.</summary>
    public const double QualityTolerance = 1e-6;
}