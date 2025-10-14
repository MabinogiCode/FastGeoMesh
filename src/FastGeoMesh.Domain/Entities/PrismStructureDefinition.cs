namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Immutable prismatic structure definition: footprint polygon (CCW), vertical extent and optional
    /// holes / constraint segments / auxiliary geometry / internal horizontal surfaces.
    /// </summary>
    public sealed class PrismStructureDefinition
    {
        /// <summary>Outer footprint (CCW polygon).</summary>
        public Polygon2D Footprint { get; }
        /// <summary>Base elevation (Z min).</summary>
        public double BaseElevation { get; }
        /// <summary>Top elevation (Z max).</summary>
        public double TopElevation { get; }
        /// <summary>Constraint segments (segment, z).</summary>
        public IReadOnlyList<(Segment2D segment, double z)> ConstraintSegments { get; }
        /// <summary>Inner hole polygons.</summary>
        public IReadOnlyList<Polygon2D> Holes { get; }
        /// <summary>Auxiliary meshing geometry (not part of equality; copied when cloning for safety).</summary>
        public MeshingGeometry Geometry { get; } = new();
        /// <summary>Internal (non-extruded) horizontal surfaces at intermediate elevations.</summary>
        public IReadOnlyList<InternalSurfaceDefinition> InternalSurfaces { get; }

        /// <summary>Create base structure.</summary>
        public PrismStructureDefinition(Polygon2D footprint, double baseElevation, double topElevation)
            : this(footprint, baseElevation, topElevation, Array.Empty<(Segment2D, double)>(), Array.Empty<Polygon2D>(), Array.Empty<InternalSurfaceDefinition>(), null) { }

        private PrismStructureDefinition(
            Polygon2D footprint,
            double baseElevation,
            double topElevation,
            IReadOnlyList<(Segment2D segment, double z)> constraints,
            IReadOnlyList<Polygon2D> holes,
            IReadOnlyList<InternalSurfaceDefinition> internalSurfaces,
            MeshingGeometry? geometry)
        {
            ArgumentNullException.ThrowIfNull(footprint);
            if (topElevation <= baseElevation)
            {
                throw new ArgumentException("TopElevation must be greater than BaseElevation.", nameof(topElevation));
            }
            Footprint = footprint;
            BaseElevation = baseElevation;
            TopElevation = topElevation;
            ConstraintSegments = constraints;
            Holes = holes;
            InternalSurfaces = internalSurfaces;
            if (geometry is not null)
            {
                foreach (var p in geometry.Points)
                {
                    Geometry.AddPoint(p);
                }
                foreach (var s in geometry.Segments)
                {
                    Geometry.AddSegment(s);
                }
            }
        }

        /// <summary>Return a new structure with an added constraint segment at elevation z.</summary>
        public PrismStructureDefinition AddConstraintSegment(Segment2D segment, double z)
        {
            if (z < BaseElevation || z > TopElevation)
            {
                throw new ArgumentOutOfRangeException(nameof(z), "Constraint Z must be within [BaseElevation, TopElevation].");
            }
            var list = new List<(Segment2D, double)>(ConstraintSegments.Count + 1);
            list.AddRange(ConstraintSegments);
            list.Add((segment, z));
            return new PrismStructureDefinition(Footprint, BaseElevation, TopElevation, list, Holes, InternalSurfaces, Geometry);
        }

        /// <summary>Return a new structure with an added inner hole polygon.</summary>
        public PrismStructureDefinition AddHole(Polygon2D hole)
        {
            ArgumentNullException.ThrowIfNull(hole);
            var list = new List<Polygon2D>(Holes.Count + 1);
            list.AddRange(Holes);
            list.Add(hole);
            return new PrismStructureDefinition(Footprint, BaseElevation, TopElevation, ConstraintSegments, list, InternalSurfaces, Geometry);
        }

        /// <summary>Add a non-extruded internal surface (plate) at elevation z with optional holes.</summary>
        public PrismStructureDefinition AddInternalSurface(Polygon2D outer, double z, params Polygon2D[] holes)
        {
            ArgumentNullException.ThrowIfNull(outer);
            if (z <= BaseElevation || z >= TopElevation)
            {
                throw new ArgumentOutOfRangeException(nameof(z), "Internal surface Z must be strictly inside (BaseElevation, TopElevation).");
            }
            var list = new List<InternalSurfaceDefinition>(InternalSurfaces.Count + 1);
            list.AddRange(InternalSurfaces);
            list.Add(new InternalSurfaceDefinition(outer, z, holes));
            return new PrismStructureDefinition(Footprint, BaseElevation, TopElevation, ConstraintSegments, Holes, list, Geometry);
        }
    }
}
