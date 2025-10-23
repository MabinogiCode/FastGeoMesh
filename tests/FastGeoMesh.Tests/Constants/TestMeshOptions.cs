using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests;

/// <summary>
/// Constants for mesh options used in tests.
/// </summary>
public static class TestMeshOptions {
    /// <summary>Default target edge length for XY plane meshing.</summary>
    public static readonly EdgeLength DefaultTargetEdgeLengthXY = EdgeLength.From(1.0);

    /// <summary>Default target edge length for Z direction meshing.</summary>
    public static readonly EdgeLength DefaultTargetEdgeLengthZ = EdgeLength.From(0.5);

    /// <summary>Fine target edge length for high-quality XY plane meshing.</summary>
    public static readonly EdgeLength FineTargetEdgeLengthXY = EdgeLength.From(0.5);

    /// <summary>Fine target edge length for high-quality Z direction meshing.</summary>
    public static readonly EdgeLength FineTargetEdgeLengthZ = EdgeLength.From(0.25);

    /// <summary>Coarse target edge length for fast XY plane meshing.</summary>
    public static readonly EdgeLength CoarseTargetEdgeLengthXY = EdgeLength.From(2.0);

    /// <summary>Coarse target edge length for fast Z direction meshing.</summary>
    public static readonly EdgeLength CoarseTargetEdgeLengthZ = EdgeLength.From(1.0);
}
