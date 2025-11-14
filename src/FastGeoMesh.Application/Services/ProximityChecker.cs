using System.Runtime.InteropServices;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>
    /// Service for checking proximity of points to geometric features.
    /// </summary>
    public sealed class ProximityChecker : IProximityChecker
    {
        /// <summary>Check if point is near any hole boundary within given distance.</summary>
        public bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(geometryService);

            foreach (var h in structure.Holes)
            {
                var vertices = h.Vertices;
                for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
                {
                    var a = vertices[j];
                    var b = vertices[i];
                    double d = geometryService.DistancePointToSegment(new Vec2(x, y), a, b);
                    if (d <= band)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Check if point is near any internal segment within given distance.</summary>
        public bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(geometryService);

            var p = new Vec2(x, y);
            foreach (var s in structure.Geometry.Segments)
            {
                var a = new Vec2(s.Start.X, s.Start.Y);
                var b = new Vec2(s.End.X, s.End.Y);
                if (geometryService.DistancePointToSegment(p, a, b) <= band)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Check if point is inside any hole using standard polygon test.</summary>
        public bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y, IGeometryService geometryService)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(geometryService);

            foreach (var h in structure.Holes)
            {
                // Convert IReadOnlyList to ReadOnlySpan for the modern API
                ReadOnlySpan<Vec2> span = h.Vertices is List<Vec2> list
                    ? CollectionsMarshal.AsSpan(list)
                    : h.Vertices is Vec2[] array
                        ? array.AsSpan()
                        : h.Vertices.ToArray().AsSpan();

                if (geometryService.PointInPolygon(span, x, y))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
