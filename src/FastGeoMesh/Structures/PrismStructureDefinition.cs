using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Structures
{
    /// <summary>Prismatic structure definition: footprint polygon and vertical extent, plus constraints and holes.</summary>
    public sealed class PrismStructureDefinition
    {
        /// <summary>Outer footprint (CCW polygon).</summary>
        public Polygon2D Footprint { get; }
        /// <summary>Base elevation (Z min).</summary>
        public double CoteBase { get; }
        /// <summary>Top elevation (Z max).</summary>
        public double CoteTete { get; }
        /// <summary>Constraint segments at specified Z levels.</summary>
        public ReadOnlyCollection<(Segment2D segment, double z)> ConstraintSegments => _constraintSegments.AsReadOnly(); private readonly List<(Segment2D segment, double z)> _constraintSegments = new();
        /// <summary>Auxiliary geometry to integrate.</summary>
        public MeshingGeometry Geometry { get; } = new();
        /// <summary>Inner hole polygons.</summary>
        public ReadOnlyCollection<Polygon2D> Holes => _holes.AsReadOnly(); private readonly List<Polygon2D> _holes = new();
        /// <summary>Create structure with footprint and elevations.</summary>
        public PrismStructureDefinition(Polygon2D footprint, double coteBase, double coteTete)
        {
            if (coteTete <= coteBase)
            {
                throw new ArgumentException("CoteTete must be greater than CoteBase.");
            }
            Footprint = footprint ?? throw new ArgumentNullException(nameof(footprint));
            CoteBase = coteBase; CoteTete = coteTete;
        }
        /// <summary>Add a constraint segment at elevation z.</summary>
        public PrismStructureDefinition AddConstraintSegment(Segment2D segment, double z)
        {
            if (z < CoteBase || z > CoteTete)
            {
                throw new ArgumentOutOfRangeException(nameof(z), "Constraint Z must be within [CoteBase, CoteTete].");
            }
            _constraintSegments.Add((segment, z)); return this;
        }
        /// <summary>Add a hole (inner contour).</summary>
        public PrismStructureDefinition AddHole(Polygon2D hole)
        {
            _holes.Add(hole ?? throw new ArgumentNullException(nameof(hole))); return this;
        }
    }
}
