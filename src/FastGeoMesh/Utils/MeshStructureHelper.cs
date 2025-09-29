using System;
using System.Collections.Generic;
using System.Linq;
using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Structures;

namespace FastGeoMesh.Utils
{
    /// <summary>Helper class for mesh structure operations and Z-level calculations.</summary>
    public static class MeshStructureHelper
    {
        /// <summary>Build sorted distinct list of Z levels for prism subdivision (optimized: single list + in-place sort/unique).</summary>
        public static IReadOnlyList<double> BuildZLevels(double z0, double z1, MesherOptions options, PrismStructureDefinition structure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(structure);
            var levels = new List<double>(32) { z0, z1 };
            // Base uniform division
            int vDiv = Math.Max(1, (int)Math.Ceiling((z1 - z0) / options.TargetEdgeLengthZ));
            for (int i = 1; i < vDiv; i++)
            {
                levels.Add(z0 + (z1 - z0) * (i / (double)vDiv));
            }
            double zMin = z0 + options.Epsilon;
            double zMax = z1 - options.Epsilon;
            void AddIfIn(double z)
            {
                if (z > zMin && z < zMax)
                {
                    levels.Add(z);
                }
            }
            foreach (var (_, z) in structure.ConstraintSegments)
            {
                AddIfIn(z);
            }
            foreach (var p in structure.Geometry.Points)
            {
                AddIfIn(p.Z);
            }
            foreach (var s in structure.Geometry.Segments)
            {
                AddIfIn(s.A.Z); AddIfIn(s.B.Z);
            }
            foreach (var plate in structure.InternalSurfaces)
            {
                AddIfIn(plate.Elevation);
            }
            // Sort + unique in place
            levels.Sort();
            int w = 1;
            for (int r = 1; r < levels.Count; r++)
            {
                if (Math.Abs(levels[r] - levels[w - 1]) > options.Epsilon)
                {
                    levels[w++] = levels[r];
                }
            }
            if (w < levels.Count)
            {
                levels.RemoveRange(w, levels.Count - w);
            }
            return levels;
        }

        /// <summary>Check if point is near any hole boundary within given distance.</summary>
        public static bool IsNearAnyHole(PrismStructureDefinition structure, double x, double y, double band)
        {
            ArgumentNullException.ThrowIfNull(structure);
            foreach (var h in structure.Holes)
            {
                var vertices = h.Vertices;
                for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
                {
                    var a = vertices[j];
                    var b = vertices[i];
                    double d = GeometryHelper.DistancePointToSegment(new Vec2(x, y), a, b);
                    if (d <= band)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>Check if point is near any internal segment within given distance.</summary>
        public static bool IsNearAnySegment(PrismStructureDefinition structure, double x, double y, double band)
        {
            ArgumentNullException.ThrowIfNull(structure);
            var p = new Vec2(x, y);
            foreach (var s in structure.Geometry.Segments)
            {
                var a = new Vec2(s.A.X, s.A.Y);
                var b = new Vec2(s.B.X, s.B.Y);
                if (GeometryHelper.DistancePointToSegment(p, a, b) <= band)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Check if point is inside any hole using spatial indices.</summary>
        public static bool IsInsideAnyHole(SpatialPolygonIndex[] holeIndices, double x, double y)
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
        public static bool IsInsideAnyHole(PrismStructureDefinition structure, double x, double y)
        {
            ArgumentNullException.ThrowIfNull(structure);
            foreach (var h in structure.Holes)
            {
                // Convert IReadOnlyList to ReadOnlySpan for the modern API
                ReadOnlySpan<Vec2> span = h.Vertices is List<Vec2> list
                    ? System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list)
                    : h.Vertices is Vec2[] array
                        ? array.AsSpan()
                        : h.Vertices.ToArray().AsSpan();

                if (GeometryHelper.PointInPolygon(span, x, y))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
