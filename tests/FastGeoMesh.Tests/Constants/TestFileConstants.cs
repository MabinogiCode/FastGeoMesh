namespace FastGeoMesh.Tests;

/// <summary>
/// File paths and naming conventions for tests.
/// </summary>
public static class TestFileConstants
{
    /// <summary>Standard filename for legacy mesh files.</summary>
    public const string LegacyMeshFileName = "0_maill.txt";
    
    /// <summary>Prefix for temporary test files.</summary>
    public const string TestFilePrefix = "fgm_test_";
    
    /// <summary>Prefix for sample mesh output files.</summary>
    public const string SampleMeshPrefix = "sample_mesh";
    
    /// <summary>
    /// Gets the full path to a legacy resource file in the specified folder.
    /// </summary>
    /// <param name="folder">The folder name containing the legacy mesh file.</param>
    /// <returns>The full path to the legacy mesh file.</returns>
    public static string GetLegacyResourcePath(string folder) => 
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", folder, LegacyMeshFileName);
}