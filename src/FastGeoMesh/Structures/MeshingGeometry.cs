using System.Collections.Generic;
using System.Collections.ObjectModel;
using FastGeoMesh.Geometry;

namespace FastGeoMesh.Structures
{
    /// <summary>
    /// Collection of auxiliary geometry (points / 3D segments) to be preserved in the output mesh.
    /// Used to derive refinement levels or exported as-is.
    /// </summary>
    public sealed class MeshingGeometry
    {
        private readonly List<Vec3> _points = new();
        private readonly List<Segment3D> _segments = new();
        private readonly object _syncLock = new();
        
        // Cache ReadOnlyCollection to avoid repeated allocations
        private volatile ReadOnlyCollection<Vec3>? _pointsReadOnly;
        private volatile ReadOnlyCollection<Segment3D>? _segmentsReadOnly;

        /// <summary>Read-only list of registered points.</summary>
        public ReadOnlyCollection<Vec3> Points
        {
            get
            {
                if (_pointsReadOnly is not null)
                {
                    return _pointsReadOnly;
                }
                    
                lock (_syncLock)
                {
                    return _pointsReadOnly ??= _points.AsReadOnly();
                }
            }
        }
        
        /// <summary>Read-only list of registered 3D segments.</summary>
        public ReadOnlyCollection<Segment3D> Segments
        {
            get
            {
                if (_segmentsReadOnly is not null)
                {
                    return _segmentsReadOnly;
                }
                    
                lock (_syncLock)
                {
                    return _segmentsReadOnly ??= _segments.AsReadOnly();
                }
            }
        }

        /// <summary>Add a point to the geometry set.</summary>
        /// <param name="p">Point to add.</param>
        /// <returns>This instance for method chaining.</returns>
        public MeshingGeometry AddPoint(Vec3 p) 
        { 
            lock (_syncLock)
            {
                _points.Add(p); 
                _pointsReadOnly = null; // Invalidate cache
            }
            return this; 
        }
        
        /// <summary>Add a 3D segment to the geometry set.</summary>
        /// <param name="s">Segment to add.</param>
        /// <returns>This instance for method chaining.</returns>
        public MeshingGeometry AddSegment(Segment3D s) 
        { 
            lock (_syncLock)
            {
                _segments.Add(s); 
                _segmentsReadOnly = null; // Invalidate cache
            }
            return this; 
        }

        /// <summary>Add multiple points efficiently.</summary>
        /// <param name="points">Points to add.</param>
        /// <returns>This instance for method chaining.</returns>
        public MeshingGeometry AddPoints(IEnumerable<Vec3> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            
            lock (_syncLock)
            {
                _points.AddRange(points);
                _pointsReadOnly = null;
            }
            return this;
        }

        /// <summary>Add multiple segments efficiently.</summary>
        /// <param name="segments">Segments to add.</param>
        /// <returns>This instance for method chaining.</returns>
        public MeshingGeometry AddSegments(IEnumerable<Segment3D> segments)
        {
            ArgumentNullException.ThrowIfNull(segments);
            
            lock (_syncLock)
            {
                _segments.AddRange(segments);
                _segmentsReadOnly = null;
            }
            return this;
        }
    }
}
