using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Structures;

/// <summary>
/// Structure defined by an XY footprint and Z elevations (CoteBase, CoteTete).
/// Holds geometry to enforce during meshing (points/segments) as well.
/// </summary>
public sealed class PrismStructureDefinition
{
    public Polygon2D Footprint { get; }
    public double CoteBase { get; }
    public double CoteTete { get; }

    /// <summary>Constraint segments at a given Z (e.g., liernes).</summary>
    public ReadOnlyCollection<(Segment2D segment, double z)> ConstraintSegments => _constraintSegments.AsReadOnly();
    private readonly List<(Segment2D segment, double z)> _constraintSegments = new();

    /// <summary>Additional geometry to integrate in meshing (3D points/segments).</summary>
    public MeshingGeometry Geometry { get; } = new();

    /// <summary>Optional inner voids (holes) footprints to exclude from caps and to generate internal side faces.</summary>
    public ReadOnlyCollection<Polygon2D> Holes => _holes.AsReadOnly();
    private readonly List<Polygon2D> _holes = new();

    public PrismStructureDefinition(Polygon2D footprint, double coteBase, double coteTete)
    {
        if (coteTete <= coteBase)
            throw new ArgumentException("CoteTete must be greater than CoteBase.");
        Footprint = footprint ?? throw new ArgumentNullException(nameof(footprint));
        CoteBase = coteBase;
        CoteTete = coteTete;
    }

    public PrismStructureDefinition AddConstraintSegment(Segment2D segment, double z)
    {
        if (z < CoteBase || z > CoteTete)
            throw new ArgumentOutOfRangeException(nameof(z), "Constraint Z must be within [CoteBase, CoteTete].");
        _constraintSegments.Add((segment, z));
        return this;
    }

    /// <summary>Adds a hole (inner contour). The polygon should be CCW and inside the outer footprint.</summary>
    public PrismStructureDefinition AddHole(Polygon2D hole)
    {
        _holes.Add(hole ?? throw new ArgumentNullException(nameof(hole)));
        return this;
    }
}
