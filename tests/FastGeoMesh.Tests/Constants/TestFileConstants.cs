namespace FastGeoMesh.Tests;
/// <summary>
/// Tests for class TestFileConstants.
/// </summary>
public static class TestFileConstants
{
    /// <summary>
    /// Constant LegacyMeshFileName used in tests.
    /// </summary>
    public const string LegacyMeshFileName = "0_maill.txt";
    /// <summary>
    /// Constant TestFilePrefix used in tests.
    /// </summary>
    public const string TestFilePrefix = "fgm_test_";
    /// <summary>
    /// Constant SampleMeshPrefix used in tests.
    /// </summary>
    public const string SampleMeshPrefix = "sample_mesh";
    /// <summary>
    /// Runs test GetLegacyResourcePath.
    /// </summary>
    public static string GetLegacyResourcePath(string folder) =>
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", folder, LegacyMeshFileName);
}
