using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests;
/// <summary>
/// Tests for class TestMeshOptions.
/// </summary>
public static class TestMeshOptions
{
    /// <summary>
    /// Public API used by tests.
    /// </summary>
    public static readonly EdgeLength DefaultTargetEdgeLengthXY = EdgeLength.From(1.0);
    /// <summary>
    /// Public API used by tests.
    /// </summary>
    public static readonly EdgeLength DefaultTargetEdgeLengthZ = EdgeLength.From(0.5);
    /// <summary>
    /// Public API used by tests.
    /// </summary>
    public static readonly EdgeLength FineTargetEdgeLengthXY = EdgeLength.From(0.5);
    /// <summary>
    /// Public API used by tests.
    /// </summary>
    public static readonly EdgeLength FineTargetEdgeLengthZ = EdgeLength.From(0.25);
    /// <summary>
    /// Public API used by tests.
    /// </summary>
    public static readonly EdgeLength CoarseTargetEdgeLengthXY = EdgeLength.From(2.0);
    /// <summary>
    /// Public API used by tests.
    /// </summary>
    public static readonly EdgeLength CoarseTargetEdgeLengthZ = EdgeLength.From(1.0);
}
