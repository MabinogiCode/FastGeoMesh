namespace FastGeoMesh.Tests;

/// <summary>
/// Standard test geometry dimensions and coordinates.
/// </summary>
public static class TestGeometries
{
    // Rectangle dimensions

    /// <summary>Width of the standard test rectangle (20 units).</summary>
    public const double StandardRectangleWidth = 20.0;

    /// <summary>Height of the standard test rectangle (5 units).</summary>
    public const double StandardRectangleHeight = 5.0;

    /// <summary>Width of the small test rectangle (4 units).</summary>
    public const double SmallRectangleWidth = 4.0;

    /// <summary>Height of the small test rectangle (2 units).</summary>
    public const double SmallRectangleHeight = 2.0;

    // Square dimensions

    /// <summary>Side length of the standard test square (10 units).</summary>
    public const double StandardSquareSide = 10.0;

    /// <summary>Side length of the small test square (5 units).</summary>
    public const double SmallSquareSide = 5.0;

    /// <summary>Side length of the unit test square (1 unit).</summary>
    public const double UnitSquareSide = 1.0;

    // Z-levels for extrusion

    /// <summary>Standard bottom Z coordinate for test prisms (-10 units).</summary>
    public const double StandardBottomZ = -10.0;

    /// <summary>Standard top Z coordinate for test prisms (10 units).</summary>
    public const double StandardTopZ = 10.0;

    /// <summary>Standard middle Z coordinate for test constraints (0 units).</summary>
    public const double StandardMidZ = 0.0;

    /// <summary>Standard constraint Z coordinate for test segments (2.5 units).</summary>
    public const double StandardConstraintZ = 2.5;

    // Hole dimensions

    /// <summary>Standard size for test holes (1 unit).</summary>
    public const double StandardHoleSize = 1.0;

    /// <summary>Standard offset for test hole placement (2 units).</summary>
    public const double StandardHoleOffset = 2.0;
}
