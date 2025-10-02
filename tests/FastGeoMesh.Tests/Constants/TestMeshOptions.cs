namespace FastGeoMesh.Tests;

/// <summary>
/// Constants for mesh options used in tests.
/// </summary>
public static class TestMeshOptions
{
    /// <summary>Default target edge length for XY plane meshing.</summary>
    public const double DefaultTargetEdgeLengthXY = 1.0;
    
    /// <summary>Default target edge length for Z direction meshing.</summary>
    public const double DefaultTargetEdgeLengthZ = 0.5;
    
    /// <summary>Fine target edge length for high-quality XY plane meshing.</summary>
    public const double FineTargetEdgeLengthXY = 0.5;
    
    /// <summary>Fine target edge length for high-quality Z direction meshing.</summary>
    public const double FineTargetEdgeLengthZ = 0.25;
    
    /// <summary>Coarse target edge length for fast XY plane meshing.</summary>
    public const double CoarseTargetEdgeLengthXY = 2.0;
    
    /// <summary>Coarse target edge length for fast Z direction meshing.</summary>
    public const double CoarseTargetEdgeLengthZ = 1.0;
}