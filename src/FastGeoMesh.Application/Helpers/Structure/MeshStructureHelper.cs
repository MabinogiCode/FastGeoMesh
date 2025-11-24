#pragma warning disable S3267, S4136, S3358
using System.Runtime.InteropServices;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Helpers.Structure
{
    /// <summary>Helper class for mesh structure operations and Z-level calculations.</summary>
    internal static class MeshStructureHelper
    {
        /// <summary>Build sorted distinct list of Z levels for prism subdivision.</summary>
        internal static IReadOnlyList<double> BuildZLevels(double z0, double z1, MesherOptions options, PrismStructureDefinition structure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(structure);

            var levels = new List<double>(32) { z0, z1 };

            AddUniformLevels(levels, z0, z1, options.TargetEdgeLengthZ.Value);
            AddLevelsFromStructure(levels, z0, z1, options.Epsilon.Value, structure);

            return SortAndMakeUnique(levels, options.Epsilon.Value);
        }

        /// <summary>Adds uniformly spaced Z-levels based on the target edge length.</summary>
        private static void AddUniformLevels(List<double> levels, double z0, double z1, double targetEdgeLengthZ)
        {
            if (targetEdgeLengthZ <= 0)
            {
                return;
            }

            double range = z1 - z0;
            if (range <= 0)
            {
                return;
            }

            int vDiv = Math.Max(1, (int)Math.Ceiling(range / targetEdgeLengthZ));
            if (vDiv <= 1)
            {
                return;
            }

            for (int i = 1; i < vDiv; i++)
            {
                levels.Add(z0 + range * (i / (double)vDiv));
            }
        }

        /// <summary>Adds Z-levels from the geometry and constraints of the prism structure.</summary>
        private static void AddLevelsFromStructure(List<double> levels, double z0, double z1, double epsilon, PrismStructureDefinition structure)
        {
            double zMin = z0 + epsilon;
            double zMax = z1 - epsilon;

            foreach (var (_, z) in structure.ConstraintSegments)
            {
                AddIfInRange(levels, z, zMin, zMax);
            }

            foreach (var p in structure.Geometry.Points)
            {
                AddIfInRange(levels, p.Z, zMin, zMax);
            }

            foreach (var s in structure.Geometry.Segments)
            {
                AddIfInRange(levels, s.Start.Z, zMin, zMax);
                AddIfInRange(levels, s.End.Z, zMin, zMax);
            }

            foreach (var plate in structure.InternalSurfaces)
            {
                AddIfInRange(levels, plate.Elevation, zMin, zMax);
            }
        }

        /// <summary>Adds a Z value to the levels list if it's within the valid range.</summary>
        private static void AddIfInRange(List<double> levels, double z, double zMin, double zMax)
        {
            if (z > zMin && z < zMax)
            {
                levels.Add(z);
            }
        }

        /// <summary>Sorts the list of levels and removes duplicates using a tolerance.</summary>
        private static List<double> SortAndMakeUnique(List<double> levels, double epsilon)
        {
            if (levels.Count < 2)
            {
                return levels;
            }

            levels.Sort();

            int writeIndex = 1;
            for (int readIndex = 1; readIndex < levels.Count; readIndex++)
            {
                if (Math.Abs(levels[readIndex] - levels[writeIndex - 1]) > epsilon)
                {
                    if (writeIndex != readIndex)
                    {
                        levels[writeIndex] = levels[readIndex];
                    }
                    writeIndex++;
                }
            }

            if (writeIndex < levels.Count)
            {
                levels.RemoveRange(writeIndex, levels.Count - writeIndex);
            }

            return levels;
        }

        /// <summary>Check if point is near any hole boundary within given distance.</summary>
        internal static bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(geometryService);

            // iterate over hole vertex lists directly
            foreach (var vertices in structure.Holes.Select(h => h.Vertices))
            {
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
        internal static bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band, IGeometryService geometryService)
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

        /// <summary>Check if point is inside any hole using spatial indices.</summary>
        internal static bool IsInsideAnyHole(ISpatialPolygonIndex[] holeIndices, double x, double y)
        {
            ArgumentNullException.ThrowIfNull(holeIndices);
            for (int i = 0; i < holeIndices.Length; i++)
            {
                if (holeIndices[i].IsInside(x, y))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Check if point is inside any hole using standard polygon test.</summary>
        internal static bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y, IGeometryService geometryService)
        {
            ArgumentNullException.ThrowIfNull(structure);
            ArgumentNullException.ThrowIfNull(geometryService);

            foreach (var hole in structure.Holes)
            {
                // Convert IReadOnlyList to ReadOnlySpan for the modern API
                ReadOnlySpan<Vec2> span;
                if (hole.Vertices is List<Vec2> list)
                {
                    span = CollectionsMarshal.AsSpan(list);
                }
                else if (hole.Vertices is Vec2[] array)
                {
                    span = array.AsSpan();
                }
                else
                {
                    var tmp = hole.Vertices.ToArray();
                    span = tmp.AsSpan();
                }

                if (geometryService.PointInPolygon(span, x, y))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#pragma warning restore S3267, S4136, S3358
