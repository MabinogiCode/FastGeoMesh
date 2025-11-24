using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    // Minimal proximity checker that uses basic geometry operations.
    internal sealed class ProximityCheckerImpl : IProximityChecker
    {
        public bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService)
        {
            foreach (var hole in structure.Holes)
            {
                var verts = hole.Vertices;
                for (int i = 0; i < verts.Count; i++)
                {
                    var a = verts[i];
                    var b = verts[(i + 1) % verts.Count];
                    if (geometryService.DistancePointToSegment(new Vec2(x, y), a, b) <= band)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService)
        {
            foreach (var seg in structure.Geometry.Segments)
            {
                // Convert 3D segment endpoints to 2D by dropping Z coordinate
                var start2 = new Vec2(seg.Start.X, seg.Start.Y);
                var end2 = new Vec2(seg.End.X, seg.End.Y);
                if (geometryService.DistancePointToSegment(new Vec2(x, y), start2, end2) <= band)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y, IGeometryService geometryService)
        {
            foreach (var hole in structure.Holes)
            {
                // Polygon.Vertices is an IReadOnlyList<Vec2> - create an array to get a span
                var vertsArray = hole.Vertices.ToArray();
                if (geometryService.PointInPolygon(vertsArray.AsSpan(), x, y))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
