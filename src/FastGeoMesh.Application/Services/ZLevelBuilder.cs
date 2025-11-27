using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Services
{
    /// <summary>
    /// Service for building Z-level subdivisions for prism meshing.
    /// </summary>
    public sealed class ZLevelBuilder : IZLevelBuilder
    {
        /// <summary>Build sorted distinct list of Z levels for prism subdivision.</summary>
        public IReadOnlyList<double> BuildZLevels(double z0, double z1, MesherOptions options, PrismStructureDefinition structure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(structure);

            var levels = new List<double>(32) { z0, z1 };

            AddUniformLevels(levels, z0, z1, options.TargetEdgeLengthZ.Value);
            AddLevelsFromStructure(levels, z0, z1, options.Epsilon.Value, structure);

            return SortAndMakeUnique(levels, options.Epsilon.Value);
        }

        /// <summary>Adds uniformly spaced Z-levels based on the target edge length.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822:Mark members as static", Justification = "Coding guideline prohibits mixing static and instance methods")]
        private void AddUniformLevels(List<double> levels, double z0, double z1, double targetEdgeLengthZ)
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
        private void AddLevelsFromStructure(List<double> levels, double z0, double z1, double epsilon, PrismStructureDefinition structure)
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822:Mark members as static", Justification = "Coding guideline prohibits mixing static and instance methods")]
        private void AddIfInRange(List<double> levels, double z, double zMin, double zMax)
        {
            if (z > zMin && z < zMax)
            {
                levels.Add(z);
            }
        }

        /// <summary>Sorts the list of levels and removes duplicates using a tolerance.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822:Mark members as static", Justification = "Coding guideline prohibits mixing static and instance methods")]
        private List<double> SortAndMakeUnique(List<double> levels, double epsilon)
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
    }
}
